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
    [string]$Version = "v1.0"
)

$ErrorActionPreference = "Stop"
$scriptDir = $PSScriptRoot
$repoRoot = (Get-Item $scriptDir).Parent.FullName
$releaseRoot = Join-Path $repoRoot "releases"
$outputDir = Join-Path $releaseRoot $Version
$resourcesSrc = Join-Path $repoRoot "resources"
$projectPath = Join-Path $repoRoot "src\SnakeMarsTheme\SnakeMarsTheme.csproj"

Write-Host "🚀 Iniciando proceso de release $Version..." -ForegroundColor Cyan
Write-Host "   📂 Repo: $repoRoot"

# ═══════════════════════════════════════════════════════════════
# 1. LIMPIEZA
# ═══════════════════════════════════════════════════════════════
Write-Host "`n🧹 Limpiando directorio de salida..." -ForegroundColor Yellow
if (Test-Path $outputDir) { Remove-Item $outputDir -Recurse -Force }
New-Item -ItemType Directory -Path $outputDir | Out-Null

# ═══════════════════════════════════════════════════════════════
# 2. COMPILACIÓN
# ═══════════════════════════════════════════════════════════════
Write-Host "`n📦 Compilando binarios (Framework-dependent)..." -ForegroundColor Cyan
$binDir = Join-Path $outputDir "bin_temp"
dotnet publish $projectPath -c Release -o $binDir --self-contained false
if ($LASTEXITCODE -ne 0) { throw "❌ Error de compilación" }

# Limpiar idiomas innecesarios
Get-ChildItem $binDir -Directory | Where-Object { $_.Name -notin @("es", "runtimes") } | Remove-Item -Recurse -Force

# ═══════════════════════════════════════════════════════════════
# 3. PREPARACIÓN BASE (COMMON)
# ═══════════════════════════════════════════════════════════════
Write-Host "`n🏗️  Preparando estructura base..." -ForegroundColor Cyan
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
    # Si hay zip, borrarlo de la release (ya debería estar descomprimido si se siguió el proceso, o se descomprime aquí)
    if (Test-Path "$resDest\FFmpeg\ffmpeg.zip") {
        Write-Host "     - Descomprimiendo ffmpeg.zip..."
        Expand-Archive "$resDest\FFmpeg\ffmpeg.zip" -DestinationPath "$resDest\FFmpeg" -Force
        Remove-Item "$resDest\FFmpeg\ffmpeg.zip" -Force
    }
}

Write-Host "   - Copiando Catálogos..." -ForegroundColor Gray
Copy-Item "$resourcesSrc\*.json" $resDest

Write-Host "   - Copiando Previews (Requerido para Light/Full)..." -ForegroundColor Gray
if (Test-Path "$resourcesSrc\Previews") { Copy-Item "$resourcesSrc\Previews" "$resDest\Previews" -Recurse }

# Crear carpetas vacías estructura
New-Item -ItemType Directory -Path "$resDest\extracted" | Out-Null
New-Item -ItemType Directory -Path "$resDest\themes" | Out-Null
New-Item -ItemType Directory -Path "$resDest\ThemesPhoto" | Out-Null
New-Item -ItemType Directory -Path "$resDest\Themes_SMTHEME" | Out-Null

# ═══════════════════════════════════════════════════════════════
# 4. GENERAR VERSIÓN LIGHT
# ═══════════════════════════════════════════════════════════════
Write-Host "`n💡 Generando versión LIGHT..." -ForegroundColor Green
$lightName = "SnakeMarsTheme_${Version}_Light"
$lightDir = Join-Path $outputDir $lightName
Copy-Item $commonDir $lightDir -Recurse

# ═══════════════════════════════════════════════════════════════
# 5. GENERAR VERSIÓN FULL
# ═══════════════════════════════════════════════════════════════
Write-Host "`n🔥 Generando versión FULL..." -ForegroundColor Magenta
$fullName = "SnakeMarsTheme_${Version}_Full"
$fullDir = Join-Path $outputDir $fullName
Copy-Item $commonDir $fullDir -Recurse

Write-Host "   - Copiando Multimedia Extra (Videos, GIFs)..." -ForegroundColor Gray
if (Test-Path "$resourcesSrc\Videos") { Copy-Item "$resourcesSrc\Videos" "$fullDir\resources\Videos" -Recurse }
if (Test-Path "$resourcesSrc\GIFs") { Copy-Item "$resourcesSrc\GIFs" "$fullDir\resources\GIFs" -Recurse }

# ═══════════════════════════════════════════════════════════════
# 6. COMPRESIÓN
# ═══════════════════════════════════════════════════════════════
Write-Host "`n🤐 Comprimiendo archivos (esto tomará tiempo)..." -ForegroundColor Yellow

# Light
$zipLight = Join-Path $outputDir "${lightName}.zip"
Write-Host "   - Comprimiendo Light..."
Compress-Archive -Path "$lightDir\*" -DestinationPath $zipLight -Force

# Full
$zipFull = Join-Path $outputDir "${fullName}.zip"
Write-Host "   - Comprimiendo Full..."
Compress-Archive -Path "$fullDir\*" -DestinationPath $zipFull -Force

# ═══════════════════════════════════════════════════════════════
# 7. FINALIZACIÓN
# ═══════════════════════════════════════════════════════════════
# Limpieza temporal
Remove-Item $binDir -Recurse -Force
Remove-Item $commonDir -Recurse -Force
Remove-Item $lightDir -Recurse -Force
Remove-Item $fullDir -Recurse -Force

# Reporte
$sizeLight = "{0:N2} MB" -f ((Get-Item $zipLight).Length / 1MB)
$sizeFull = "{0:N2} MB" -f ((Get-Item $zipFull).Length / 1MB)

Write-Host "`n✅ RELEASE COMPLETADA EXITOSAMENTE" -ForegroundColor Green
Write-Host "----------------------------------------"
Write-Host "📂 Ubicación: $outputDir"
Write-Host "📄 Light: $sizeLight (Sin multimedia)"
Write-Host "📦 Full:  $sizeFull (Con todo)"
Write-Host "----------------------------------------"
