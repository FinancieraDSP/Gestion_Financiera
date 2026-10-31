# syntax=docker/dockerfile:1

# ---------- Etapa 1: compilar ----------
FROM mcr.microsoft.com/dotnet/sdk:10.0 AS build
WORKDIR /src

COPY shared/GestionFinanciera.Shared/GestionFinanciera.Shared.csproj shared/GestionFinanciera.Shared/
COPY frontend/GestionFinanciera.Web/GestionFinanciera.Web.csproj frontend/GestionFinanciera.Web/
RUN dotnet restore frontend/GestionFinanciera.Web/GestionFinanciera.Web.csproj

COPY shared/ shared/
COPY frontend/ frontend/
RUN dotnet publish frontend/GestionFinanciera.Web/GestionFinanciera.Web.csproj \
    -c Release -o /app/publish --no-restore /p:UseAppHost=false

# ---------- Etapa 2: ejecutar ----------
FROM mcr.microsoft.com/dotnet/aspnet:10.0 AS final
WORKDIR /app

ENV TZ=America/El_Salvador

COPY --from=build /app/publish .

# Logs y llaves de Data Protection (cifran la cookie de sesión y los tokens antifalsificación):
# si se pierden al reiniciar, todos los usuarios tendrían que volver a iniciar sesión.
RUN mkdir -p /app/logs /home/app/.aspnet/DataProtection-Keys \
    && chown -R app:app /app/logs /home/app/.aspnet

USER app
EXPOSE 8080
ENTRYPOINT ["dotnet", "GestionFinanciera.Web.dll"]