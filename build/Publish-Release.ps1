<#
.SYNOPSIS
Genera las versiones FULL y LIGHT de la release del proyecto.

.DESCRIPTION
Este script compila el proyecto en modo Release y genera dos archivos ZIP:
1. FULL: Incluye todos los recursos (Videos, GIFs, Previews, FFmpeg).
2. LIGHT: Incluye solo binarios, FFmpeg y configuración (sin multimedia pesada).

.PARAMETER Version
La versión de la release (ej: v1.0). Por defecto usa "v1.0".
#>

param(
    [string]$Version = "v1.0.0",
    [switch]$Dev = $false
)

$ErrorActionPreference = "Stop"
$scriptDir = $PSScriptRoot
$repoRoot = (Get-Item $scriptDir).Parent.FullName
$releaseRoot = Join-Path $repoRoot "releases"
$outputDir = Join-Path $releaseRoot $Version
$resourcesSrc = Join-Path $repoRoot "resources"
$projectPath = Join-Path $repoRoot "src\SnakeMarsTheme\SnakeMarsTheme.csproj"

Write-Host "Iniciando proceso de release $Version..." -ForegroundColor Cyan
Write-Host "   Repo: $repoRoot"

# 1. LIMPIEZA
Write-Host "`n[CLEAN] Limpiando directorio de salida..." -ForegroundColor Yellow
if (Test-Path $outputDir) { Remove-Item $outputDir -Recurse -Force }
New-Item -ItemType Directory -Path $outputDir | Out-Null

# 2. COMPILACIÓN
Write-Host "`n[BUILD] Compilando binarios (Framework-dependent)..." -ForegroundColor Cyan
$binDir = Join-Path $outputDir "bin_temp"
dotnet publish $projectPath -c Release -o $binDir --self-contained false
if ($LASTEXITCODE -ne 0) { throw "Error de compilación" }

# Limpiar idiomas innecesarios
Get-ChildItem $binDir -Directory | Where-Object { $_.Name -notin @("es", "runtimes") } | Remove-Item -Recurse -Force

# 3. PREPARACIÓN BASE (COMMON)
Write-Host "`n[PREP] Preparando estructura base..." -ForegroundColor Cyan
$commonDir = Join-Path $outputDir "common_temp"
New-Item -ItemType Directory -Path $commonDir | Out-Null

# Copiar Binarios
Copy-Item "$binDir\*" $commonDir -Recurse

# Copiar Resources Base (FFmpeg, JSONs)
$resDest = Join-Path $commonDir "resources"
New-Item -ItemType Directory -Path $resDest | Out-Null

Write-Host "   - Copiando FFmpeg (Offline)..." -ForegroundColor Gray
if (Test-Path "$resourcesSrc\FFmpeg") {
    Copy-Item "$resourcesSrc\FFmpeg" $resDest -Recurse
    # Si hay zip, borrarlo
    if (Test-Path "$resDest\FFmpeg\ffmpeg.zip") {
        Write-Host "     - Descomprimiendo ffmpeg.zip..."
        Expand-Archive "$resDest\FFmpeg\ffmpeg.zip" -DestinationPath "$resDest\FFmpeg" -Force
        Remove-Item "$resDest\FFmpeg\ffmpeg.zip" -Force
    }
}

Write-Host "   - Copiando Catálogos..." -ForegroundColor Gray
Copy-Item "$resourcesSrc\*.json" $resDest

Write-Host "   - Copiando Previews..." -ForegroundColor Gray
if (Test-Path "$resourcesSrc\Previews") { Copy-Item "$resourcesSrc\Previews" "$resDest\Previews" -Recurse }

# Nota: Carpetas vacías (themes, extracted) NO se crean aquí. La app las crea en UserData.

# 4. GENERAR VERSIÓN LIGHT
if (!$Dev) {
    Write-Host "`n[LIGHT] Generando versión LIGHT..." -ForegroundColor Green
    $lightName = "SnakeMarsTheme_${Version}_Light"
    $lightDir = Join-Path $outputDir $lightName
    $lightRoot = Join-Path $lightDir "SnakeMarsTheme"
    New-Item -ItemType Directory -Path $lightRoot | Out-Null
    Copy-Item "$commonDir\*" $lightRoot -Recurse
}
else {
    Write-Host "`n[LIGHT] OMITIDO POR MODO DEV" -ForegroundColor DarkGray
}

# 5. GENERAR VERSIÓN FULL
Write-Host "`n[FULL] Generando versión FULL..." -ForegroundColor Magenta
$fullName = "SnakeMarsTheme_${Version}_Full"
$fullDir = Join-Path $outputDir $fullName
$fullRoot = Join-Path $fullDir "SnakeMarsTheme"
New-Item -ItemType Directory -Path $fullRoot | Out-Null
Copy-Item "$commonDir\*" $fullRoot -Recurse

Write-Host "   - Copiando Multimedia Extra..." -ForegroundColor Gray
if (Test-Path "$resourcesSrc\Videos") { Copy-Item "$resourcesSrc\Videos" "$fullRoot\resources\Videos" -Recurse }
if (Test-Path "$resourcesSrc\GIFs") { Copy-Item "$resourcesSrc\GIFs" "$fullRoot\resources\GIFs" -Recurse }

# 6. COMPRESIÓN
Write-Host "`n[ZIP] Comprimiendo archivos..." -ForegroundColor Yellow

# Light
if (!$Dev) {
    $zipLight = Join-Path $outputDir "${lightName}.zip"
    Write-Host "   - Comprimiendo Light..."
    Compress-Archive -Path "$lightDir\*" -DestinationPath $zipLight -Force
}

# Full
$zipFull = Join-Path $outputDir "${fullName}.zip"
Write-Host "   - Comprimiendo Full..."
Compress-Archive -Path "$fullDir\*" -DestinationPath $zipFull -Force

# 7. FINALIZACIÓN
# Limpieza temporal
Remove-Item $binDir -Recurse -Force
Remove-Item $commonDir -Recurse -Force
# Reporte
if (!$Dev) {
    $sizeLight = "{0:N2} MB" -f ((Get-Item $zipLight).Length / 1MB)
    Remove-Item $lightDir -Recurse -Force
}
else {
    $sizeLight = "N/A"
}
$sizeFull = "{0:N2} MB" -f ((Get-Item $zipFull).Length / 1MB)

Write-Host "`n[SUCCESS] RELEASE COMPLETADA EXITOSAMENTE" -ForegroundColor Green
Write-Host "----------------------------------------"
Write-Host "Ubicación: $outputDir"
Write-Host "Light: $sizeLight (Sin multimedia)"
Write-Host "Full:  $sizeFull (Con todo)"
Write-Host "----------------------------------------"
