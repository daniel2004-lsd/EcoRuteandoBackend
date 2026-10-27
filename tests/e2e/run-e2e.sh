#!/usr/bin/env bash

set -u

BASE_URL="${BASE_URL:-http://localhost:5124}"
ADMIN_EMAIL="${ADMIN_EMAIL:-admin@ecoruteando.com}"
ADMIN_PASSWORD="${ADMIN_PASSWORD:-EcoAdmin!2026}"

PASS=0
FAIL=0

TMP_DIR="$(mktemp -d)"
trap 'rm -rf "$TMP_DIR"' EXIT

BODY="$TMP_DIR/body"

test_http() {
    local name="$1"
    local method="$2"
    local path="$3"
    local expected="$4"
    local token="${5:-}"
    local data="${6:-}"

    local -a args=(
        -sS
        -o "$BODY"
        -w '%{http_code}'
        --connect-timeout 3
        --max-time 15
        -X "$method"
        "${BASE_URL}${path}"
    )

    if [[ -n "$token" ]]; then
        args+=(-H "Authorization: Bearer $token")
    fi

    if [[ -n "$data" ]]; then
        args+=(-H 'Content-Type: application/json' -d "$data")
    fi

    local status
    if ! status="$(curl "${args[@]}")"; then
        echo "❌ $name — no se pudo completar la petición"
        ((FAIL+=1))
        return 1
    fi

    if [[ "$status" == "$expected" ]]; then
        echo "✅ $name — HTTP $status"
        ((PASS+=1))
        return 0
    else
        echo "❌ $name — esperado HTTP $expected, recibido HTTP $status"
        echo "   Respuesta: $(head -c 300 "$BODY")"
        ((FAIL+=1))
        return 1
    fi
}

echo "=========================================================="
echo " EcoRuteando — Suite Completa de Pruebas E2E Automatizadas"
echo " API: $BASE_URL"
echo "=========================================================="
echo

# 1. Endpoints Públicos
echo "--- 1. Endpoints Públicos ---"
test_http "Consultar POIs (Público)" GET "/api/pois" 200

# 2. Seguridad Negativa y Autorización
echo
echo "--- 2. Seguridad y Autenticación Negativa ---"
test_http "Perfil sin token" GET "/api/auth/me" 401
test_http "Perfil con token inválido" GET "/api/auth/me" 401 "token-invalido-xyz"
test_http "Registro con datos inválidos (Email mal formado, contraseña corta)" POST "/api/auth/register" 400 "" '{"firstName":"Test","lastName":"Runner","email":"email-invalido","password":"123"}'
test_http "Login con usuario inexistente" POST "/api/auth/login" 401 "" '{"email":"noexiste@ecoruteando.test","password":"Password123!"}'

# 3. Flujo de Registro y Verificación de Duplicados
echo
echo "--- 3. Registro y Validación de Cuentas ---"
TEST_TIMESTAMP=$(date +%s%N | cut -b1-13)
TEST_EMAIL="e2e.user.${TEST_TIMESTAMP}@ecoruteando.test"
TEST_PASS="E2eTestPass123!"

test_http "Registro de nuevo usuario válido" POST "/api/auth/register" 200 "" "{\"firstName\":\"E2E\",\"lastName\":\"User\",\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASS\"}"
test_http "Registro duplicado con mismo correo (Protección Conflict)" POST "/api/auth/register" 409 "" "{\"firstName\":\"E2E\",\"lastName\":\"User\",\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASS\"}"
test_http "Login con cuenta pendiente de verificación (requiere activación)" POST "/api/auth/login" 200 "" "{\"email\":\"$TEST_EMAIL\",\"password\":\"$TEST_PASS\"}"

# 4. Flujo de Autenticación y Sesión
echo
echo "--- 4. Autenticación y Perfil ---"
test_http "Login con credenciales válidas" POST "/api/auth/login" 200 "" "{\"email\":\"$ADMIN_EMAIL\",\"password\":\"$ADMIN_PASSWORD\"}"
AUTH_TOKEN=$(jq -r '.accessToken // empty' "$BODY")
REFRESH_TOKEN=$(jq -r '.refreshToken // empty' "$BODY")

test_http "Consultar perfil autenticado (/api/auth/me)" GET "/api/auth/me" 200 "$AUTH_TOKEN"

if [[ -n "$REFRESH_TOKEN" ]]; then
    test_http "Refrescar token de acceso (/api/auth/refresh)" POST "/api/auth/refresh" 200 "" "{\"refreshToken\":\"$REFRESH_TOKEN\"}"
fi

# 5. Flujos de Movilidad y Trayectos
echo
echo "--- 5. Movilidad y Ciclo de Vida de Trayectos ---"
test_http "Consultar rutas disponibles" GET "/api/routes" 200 "$AUTH_TOKEN"
ROUTE_ID=$(jq -r '.[0].id // empty' "$BODY")

if [[ -n "$ROUTE_ID" ]]; then
    test_http "Iniciar trayecto (StartTrip)" POST "/api/trips" 201 "$AUTH_TOKEN" "{\"routeId\":\"$ROUTE_ID\",\"transportMode\":\"bike\",\"source\":\"mobile\"}"
    USAGE_ID=$(jq -r '.id // empty' "$BODY")

    if [[ -n "$USAGE_ID" ]]; then
        test_http "Consultar trayecto activo por ID" GET "/api/trips/$USAGE_ID" 200 "$AUTH_TOKEN"
        test_http "Finalizar trayecto con métricas reales (CompleteTrip)" POST "/api/trips/$USAGE_ID/complete" 204 "$AUTH_TOKEN" "{\"usageId\":\"$USAGE_ID\",\"actualDistanceKm\":10.2,\"actualDurationMin\":28,\"actualCo2Kg\":1.5}"
        test_http "Idempotencia: Completar trayecto ya finalizado (retorna 204)" POST "/api/trips/$USAGE_ID/complete" 204 "$AUTH_TOKEN" "{\"usageId\":\"$USAGE_ID\",\"actualDistanceKm\":10.2,\"actualDurationMin\":28,\"actualCo2Kg\":1.5}"
        test_http "Asimetría de dominio: Interrumpir trayecto finalizado (lanza 400)" POST "/api/trips/$USAGE_ID/interrupt" 400 "$AUTH_TOKEN" "{\"usageId\":\"$USAGE_ID\",\"actualDistanceKm\":5.0,\"actualDurationMin\":15,\"actualCo2Kg\":0.7}"
        test_http "Verificar persistencia de métricas en trayecto finalizado" GET "/api/trips/$USAGE_ID" 200 "$AUTH_TOKEN"
    fi

    # 6. Favoritos y Calificaciones
    echo
    echo "--- 6. Favoritos y Calificaciones ---"
    test_http "Agregar ruta a favoritos" POST "/api/favorites" 204 "$AUTH_TOKEN" "{\"routeId\":\"$ROUTE_ID\",\"label\":\"Ruta Favorita E2E\"}"
    test_http "Idempotencia: Agregar favorito duplicado (retorna 204)" POST "/api/favorites" 204 "$AUTH_TOKEN" "{\"routeId\":\"$ROUTE_ID\",\"label\":\"Ruta Favorita E2E\"}"
    test_http "Consultar favoritos" GET "/api/favorites" 200 "$AUTH_TOKEN"

    test_http "Calificar ruta (5 estrellas)" POST "/api/ratings" 204 "$AUTH_TOKEN" "{\"routeId\":\"$ROUTE_ID\",\"ratingValue\":5,\"comment\":\"Excelente recorrido\"}"
    test_http "Consultar mi calificación" GET "/api/ratings/route/$ROUTE_ID/mine" 200 "$AUTH_TOKEN"
    test_http "Actualizar calificación (4 estrellas)" POST "/api/ratings" 204 "$AUTH_TOKEN" "{\"routeId\":\"$ROUTE_ID\",\"ratingValue\":4,\"comment\":\"Buen recorrido, actualizado\"}"
    test_http "Verificar calificación actualizada" GET "/api/ratings/route/$ROUTE_ID/mine" 200 "$AUTH_TOKEN"
fi

echo
echo "=========================================================="
echo " Resumen de Ejecución E2E"
echo "=========================================================="
echo "Aprobadas: $PASS"
echo "Fallidas:  $FAIL"
echo "Total:     $((PASS + FAIL))"

if (( FAIL > 0 )); then
    exit 1
fi

exit 0
