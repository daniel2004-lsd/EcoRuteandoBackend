# EcoRuteando — Backend API (.NET 9)

Backend modular de la plataforma **EcoRuteando**, implementado en **.NET 9 (C#)** siguiendo los principios de **Clean Architecture**, **Domain-Driven Design (DDD)** y el patrón **CQRS con MediatR**.

---

## 🛠️ Tecnologías y Arquitectura

- **Framework:** .NET 9.0 (ASP.NET Core Web API)
- **Patrón:** Clean Architecture modular (Presentation → Application ← Infrastructure / Domain) y CQRS
- **Base de Datos:** PostgreSQL 16 con extensión geoespacial **PostGIS**
- **Migraciones:** Liquibase (gestionado independientemente en `EcoRuteandoDb`)
- **Seguridad:**
  - JWT (JSON Web Tokens) con validación estricta de emisor, audiencia y expiración.
  - Hashing de contraseñas con **BCrypt** (work factor 12).
  - Segundo Factor de Autenticación (**TOTP / 2FA**) con cifrado **AES-256-GCM** de los secretos.
  - Autorización granular basada en roles y permisos (`HasPermission`).
  - Rate limiting centralizado por IP y endpoints sensibles.

---

## 🚀 Requisitos Previos

- [.NET 9.0 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Docker](https://www.docker.com/) y Docker Compose
- Instancia activa de PostgreSQL 16 con PostGIS habilitado (puerto por defecto `25450` local o `5432`).

---

## ⚙️ Variables de Entorno y Configuración

El servicio se configura mediante `appsettings.json`, variables de entorno o secretos del sistema:

| Variable | Descripción | Valor por Defecto / Ejemplo |
| :--- | :--- | :--- |
| `ConnectionStrings__DefaultConnection` | Cadena de conexión a PostgreSQL/PostGIS | `Host=localhost;Port=25450;Database=ecoRuteando_db;Username=...;Password=...` |
| `Jwt__SecretKey` | Clave simétrica para firma y verificación de tokens JWT | Cadena secreta de mínimo 256 bits |
| `Jwt__Issuer` | Emisor esperado del token | `EcoRuteando` |
| `Jwt__Audience` | Audiencia esperada del token | `EcoRuteandoUsers` |
| `SecurityPolicy__TotpEncryptionKey` | Clave secreta AES-256 para cifrar los secretos TOTP | Clave en Base64 de 32 bytes (256 bits) |
| `Admin__Email` | Correo del administrador inicial para bootstrap | `admin@ecoruteando.com` |
| `Admin__Password` | Contraseña del administrador inicial | Contraseña robusta según política |
| `GoogleMaps__ApiKey` | Clave del servidor para proxy de geocodificación y rutas | Clave API de Google Cloud Platform |

---

## 💻 Ejecución Local

### 1. Restaurar dependencias y compilar
```bash
dotnet restore
dotnet build -c Release
```

### 2. Ejecutar pruebas automatizadas
```bash
dotnet test tests/EcoRuteando.UnitTests/EcoRuteando.UnitTests.csproj
dotnet test tests/EcoRuteando.IntegrationTests/EcoRuteando.IntegrationTests.csproj
```

### 3. Iniciar el servicio API
```bash
dotnet run --project src/Api/EcoRuteando.Api/EcoRuteando.Api.csproj
```
El servicio escuchará localmente en:
- HTTP: `http://localhost:5124`
- HTTPS: `https://localhost:7076`

---

## 🐳 Contenedor Docker

La imagen de producción utiliza compilación multi-etapa y corre como usuario no privilegiado `app`:

```bash
# Construir la imagen localmente
docker build -f src/Api/EcoRuteando.Api/Dockerfile -t ecoruteando-backend .

# Ejecutar el contenedor
docker run -d -p 5124:5124 -p 8080:8080 --name ecoruteando-api ecoruteando-backend
```

---

## 📄 Licencia

Proyecto formativo Sena ADSO — EcoRuteando.