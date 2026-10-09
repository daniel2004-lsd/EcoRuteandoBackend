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
# ETAPA 2: RUNTIME
# =========================
FROM mcr.microsoft.com/dotnet/aspnet:9.0 AS runtime

WORKDIR /app

# Copiar aplicación publicada
COPY --from=build /app/publish .

# Puerto de la API
EXPOSE 5124

# Configurar ASP.NET Core
ENV ASPNETCORE_URLS=http://+:5124

# Ejecutar API
ENTRYPOINT ["dotnet", "EcoRuteando.Api.dll"]