<#
.SYNOPSIS
Automatiza la creación del instalador usando Inno Setup.

.DESCRIPTION
1. Extrae el ZIP Full de la release v1.0.
2. Compila el script Setup.iss usando ISCC.exe.
3. Genera el ejecutable de instalación en releases/

.REQUIREMENTS
Inno Setup 6.x instalado en Program Files.
#>

param(
    [string]$Version = "v1.0.1"
)
$ZipName = "SnakeMarsTheme_${Version}_Full.zip"
$ScriptRoot = $PSScriptRoot
$RepoRoot = (Get-Item $ScriptRoot).Parent.FullName
$ReleaseDir = "$RepoRoot\releases\$Version"
$TempDir = "$ScriptRoot\temp_installer"
$IssFile = "$ScriptRoot\Setup.iss"

Write-Host "Iniciando construccion del instalador..." -ForegroundColor Cyan

# 1. Verificar ZIP Full
$ZipPath = "$ReleaseDir\$ZipName"
if (-not (Test-Path $ZipPath)) {
    Write-Error "No se encontro $ZipPath. Ejecuta primero Publish-Release.ps1"
    exit 1
}

# 2. Descomprimir
Write-Host "[EXTRACT] Extrayendo archivos fuente desde $ZipName..." -ForegroundColor Cyan
if (Test-Path $TempDir) { Remove-Item $TempDir -Recurse -Force }
Expand-Archive -Path $ZipPath -DestinationPath $TempDir -Force

# 2.1 Flatten if needed (Handle nested zip structure)
$nestedRoot = Join-Path $TempDir "SnakeMarsTheme"
if (Test-Path $nestedRoot) {
    Write-Host "[FLATTEN] Normalizando estructura para instalador..." -ForegroundColor Cyan
    Get-ChildItem -Path "$nestedRoot\*" -Recurse | Move-Item -Destination $TempDir -Force
    Remove-Item $nestedRoot -Recurse -Force
}

# 3. Buscar ISCC (Compilador Inno Setup)
$InnoPath = "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
if (-not (Test-Path $InnoPath)) { 
    $InnoPath = "${env:ProgramFiles}\Inno Setup 6\ISCC.exe" 
}

if (Test-Path $InnoPath) {
    Write-Host "[COMPILE] Compilando setup.iss..." -ForegroundColor Green
    & $InnoPath $IssFile
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n[SUCCESS] INSTALADOR CREADO EXITOSAMENTE" -ForegroundColor Green
        Write-Host "   Ubicacion: $RepoRoot\releases"
        
        # Limpiar temporales
        Remove-Item $TempDir -Recurse -Force
    }
    else {
        Write-Error "Error al compilar el instalador."
    }
}
else {
    Write-Warning "`nInno Setup Compiler (ISCC.exe) no encontrado."
    Write-Host "   1. Descarga e instala Inno Setup 6: https://jrsoftware.org/isdl.php"
    Write-Host "   2. Ejecuta este script nuevamente."
}
