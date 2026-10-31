# syntax=docker/dockerfile:1

# ---------- Etapa 1: compilar ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

# Primero solo los .csproj: así el "restore" queda en caché si no cambian los paquetes
COPY shared/GestionFinanciera.Shared/GestionFinanciera.Shared.csproj shared/GestionFinanciera.Shared/
COPY backend/GestionFinanciera.Api/GestionFinanciera.Api.csproj backend/GestionFinanciera.Api/
RUN dotnet restore backend/GestionFinanciera.Api/GestionFinanciera.Api.csproj

# Luego el código (y el script SQL, que el proyecto enlaza desde /database)
COPY shared/ shared/
COPY backend/ backend/
COPY database/ database/
RUN dotnet publish backend/GestionFinanciera.Api/GestionFinanciera.Api.csproj \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- Etapa 2: ejecutar ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

# Hora de El Salvador (sin horario de verano, UTC-6)
ENV TZ=America/El_Salvador

COPY --from=build /app/publish .

# Carpeta de logs escribible por el usuario no-root
RUN mkdir -p /app/logs && chown -R app:app /app/logs

USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "GestionFinanciera.Api.dll"]