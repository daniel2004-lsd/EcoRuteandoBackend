# =========================
# ETAPA 1: BUILD
# =========================
FROM mcr.microsoft.com/dotnet/sdk:9.0 AS build

WORKDIR /src

# Copiar la solución
COPY EcoRuteando.sln .

# Copiar todos los proyectos
COPY src/Shared/EcoRuteando.Shared/EcoRuteando.Shared.csproj src/Shared/EcoRuteando.Shared/
COPY src/Modules/Security/EcoRuteando.Modules.Security.Domain/EcoRuteando.Modules.Security.Domain.csproj src/Modules/Security/EcoRuteando.Modules.Security.Domain/
COPY src/Modules/Security/EcoRuteando.Modules.Security.Application/EcoRuteando.Modules.Security.Application.csproj src/Modules/Security/EcoRuteando.Modules.Security.Application/
COPY src/Modules/Security/EcoRuteando.Modules.Security.Infrastructure/EcoRuteando.Modules.Security.Infrastructure.csproj src/Modules/Security/EcoRuteando.Modules.Security.Infrastructure/
COPY src/Modules/Security/EcoRuteando.Modules.Security.Presentation/EcoRuteando.Modules.Security.Presentation.csproj src/Modules/Security/EcoRuteando.Modules.Security.Presentation/
COPY src/Modules/Security/EcoRuteando.Modules.Security/EcoRuteando.Modules.Security.csproj src/Modules/Security/EcoRuteando.Modules.Security/
COPY src/Modules/Mobility/EcoRuteando.Modules.Mobility.Domain/EcoRuteando.Modules.Mobility.Domain.csproj src/Modules/Mobility/EcoRuteando.Modules.Mobility.Domain/
COPY src/Modules/Mobility/EcoRuteando.Modules.Mobility.Application/EcoRuteando.Modules.Mobility.Application.csproj src/Modules/Mobility/EcoRuteando.Modules.Mobility.Application/
COPY src/Modules/Mobility/EcoRuteando.Modules.Mobility.Infrastructure/EcoRuteando.Modules.Mobility.Infrastructure.csproj src/Modules/Mobility/EcoRuteando.Modules.Mobility.Infrastructure/
COPY src/Modules/Mobility/EcoRuteando.Modules.Mobility.Presentation/EcoRuteando.Modules.Mobility.Presentation.csproj src/Modules/Mobility/EcoRuteando.Modules.Mobility.Presentation/
COPY src/Modules/Notifications/EcoRuteando.Modules.Notifications.Domain/EcoRuteando.Modules.Notifications.Domain.csproj src/Modules/Notifications/EcoRuteando.Modules.Notifications.Domain/
COPY src/Modules/Notifications/EcoRuteando.Modules.Notifications.Application/EcoRuteando.Modules.Notifications.Application.csproj src/Modules/Notifications/EcoRuteando.Modules.Notifications.Application/
COPY src/Modules/Notifications/EcoRuteando.Modules.Notifications.Infrastructure/EcoRuteando.Modules.Notifications.Infrastructure.csproj src/Modules/Notifications/EcoRuteando.Modules.Notifications.Infrastructure/
COPY src/Modules/Notifications/EcoRuteando.Modules.Notifications.Presentation/EcoRuteando.Modules.Notifications.Presentation.csproj src/Modules/Notifications/EcoRuteando.Modules.Notifications.Presentation/
COPY src/Modules/Admin/EcoRuteando.Modules.Admin.Domain/EcoRuteando.Modules.Admin.Domain.csproj src/Modules/Admin/EcoRuteando.Modules.Admin.Domain/
COPY src/Modules/Admin/EcoRuteando.Modules.Admin.Application/EcoRuteando.Modules.Admin.Application.csproj src/Modules/Admin/EcoRuteando.Modules.Admin.Application/
COPY src/Modules/Admin/EcoRuteando.Modules.Admin.Infrastructure/EcoRuteando.Modules.Admin.Infrastructure.csproj src/Modules/Admin/EcoRuteando.Modules.Admin.Infrastructure/
COPY src/Modules/Admin/EcoRuteando.Modules.Admin.Presentation/EcoRuteando.Modules.Admin.Presentation.csproj src/Modules/Admin/EcoRuteando.Modules.Admin.Presentation/
COPY src/Modules/Integrations/EcoRuteando.Modules.Integrations.Domain/EcoRuteando.Modules.Integrations.Domain.csproj src/Modules/Integrations/EcoRuteando.Modules.Integrations.Domain/
COPY src/Modules/Integrations/EcoRuteando.Modules.Integrations.Application/EcoRuteando.Modules.Integrations.Application.csproj src/Modules/Integrations/EcoRuteando.Modules.Integrations.Application/
COPY src/Modules/Integrations/EcoRuteando.Modules.Integrations.Infrastructure/EcoRuteando.Modules.Integrations.Infrastructure.csproj src/Modules/Integrations/EcoRuteando.Modules.Integrations.Infrastructure/
COPY src/Modules/Integrations/EcoRuteando.Modules.Integrations.Presentation/EcoRuteando.Modules.Integrations.Presentation.csproj src/Modules/Integrations/EcoRuteando.Modules.Integrations.Presentation/
COPY src/Api/EcoRuteando.Api/EcoRuteando.Api.csproj src/Api/EcoRuteando.Api/

# Restaurar dependencias
RUN dotnet restore EcoRuteando.sln

# Copiar el código fuente completo
COPY src ./src

# Compilar
RUN dotnet build EcoRuteando.sln -c Release --no-restore

# Publicar la API
RUN dotnet publish src/Api/EcoRuteando.Api/EcoRuteando.Api.csproj \
    -c Release \
    -o /app/publish \
    --no-restore


# =========================
# ETAPA 2: RUNTIME SIN PRIVILEGIOS Y CON HEALTHCHECK NATIVO
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app

# Garantizar la existencia del usuario no privilegiado 'app' (UID 1654 o fallback)
USER root
RUN id -u app >/dev/null 2>&1 || (groupadd -g 1654 app && useradd -u 1654 -g app -s /bin/sh -m app)

# Copiar aplicación publicada
COPY --from=build /app/publish .

# Asignar permisos diferenciados y mínimos necesarios
RUN chown -R app:app /app \
    && find /app -type d -exec chmod 755 {} + \
    && find /app -type f -exec chmod 550 {} +

# Puerto canónico de la API
EXPOSE 5124

# Configurar ASP.NET Core en puerto 5124
ENV ASPNETCORE_URLS=http://+:5124

# Healthcheck nativo sobre GET /health (sin requerir curl/wget ni endpoints administrativos)
HEALTHCHECK --interval=30s --timeout=5s --start-period=15s --retries=3 \
    CMD /bin/bash -c 'exec 3<>/dev/tcp/127.0.0.1/5124 && echo -e "GET /health HTTP/1.1\r\nHost: localhost\r\nConnection: close\r\n\r\n" >&3 && grep -q -E "^HTTP/1\.[01] 200([[:space:]]|$)" <&3' || exit 1

# Ejecutar como usuario no privilegiado
USER app

# Ejecutar API
ENTRYPOINT ["dotnet", "EcoRuteando.Api.dll"]
