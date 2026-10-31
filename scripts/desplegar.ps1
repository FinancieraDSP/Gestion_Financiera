<#
 Publica una nueva versión en Azure.
 Requisitos: Docker Desktop abierto, "az login" hecho.
 Uso:  powershell -ExecutionPolicy Bypass -File scripts/desplegar.ps1 -Version 3
       powershell -ExecutionPolicy Bypass -File scripts/desplegar.ps1 -Version 4 -Servicio api
 Cada despliegue usa un número de versión NUEVO (3, 4, 5...). Para ver los usados:
       az acr repository show-tags -n <ACR> --repository finanzapp-api -o table
#>
param(
    [ValidateSet("api", "web", "ambos")][string]$Servicio = "ambos",
    [Parameter(Mandatory)][string]$Version
)

# ===== Ajusta estos valores una sola vez =====
$RG      = "db-finanzas"
$ACRNAME = "finanzappalvarado"
$APPAPI  = "finanzapersonal-dsp-api"
$APPWEB  = "finanzapersonal-dsp"
# =============================================

$ACR = "$ACRNAME.azurecr.io"
Set-Location (Split-Path $PSScriptRoot -Parent)   # raíz del repo

az acr login -n $ACRNAME
if ($LASTEXITCODE -ne 0) { throw "az acr login falló (¿Docker Desktop abierto y az login hecho?)" }

function Publicar($nombre, $dockerfile, $app) {
    $imagen = "$ACR/finanzapp-${nombre}:$Version"
    docker build -f $dockerfile -t $imagen .
    if ($LASTEXITCODE -ne 0) { throw "docker build falló ($nombre)" }
    docker push $imagen
    if ($LASTEXITCODE -ne 0) { throw "docker push falló ($nombre)" }
    az webapp config container set -g $RG -n $app --container-image-name $imagen --container-registry-url "https://$ACR" --output none
    if ($LASTEXITCODE -ne 0) { throw "No se pudo actualizar el sitio $app" }
    Write-Host "OK: $app ahora corre $imagen" -ForegroundColor Green
}

if ($Servicio -in "api", "ambos") { Publicar "api" "backend/GestionFinanciera.Api/Dockerfile" $APPAPI }
if ($Servicio -in "web", "ambos") { Publicar "web" "frontend/GestionFinanciera.Web/Dockerfile" $APPWEB }