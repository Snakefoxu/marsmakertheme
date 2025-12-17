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

$Version = "v1.0"
$ZipName = "SnakeMarsTheme_${Version}_Full.zip"
$ScriptRoot = $PSScriptRoot
$RepoRoot = (Get-Item $ScriptRoot).Parent.FullName
$ReleaseDir = "$RepoRoot\releases\$Version"
$TempDir = "$ScriptRoot\temp_installer"
$IssFile = "$ScriptRoot\Setup.iss"

Write-Host "🚀 Iniciando construcción del instalador..." -ForegroundColor Cyan

# 1. Verificar ZIP Full
$ZipPath = "$ReleaseDir\$ZipName"
if (-not (Test-Path $ZipPath)) {
    Write-Error "❌ No se encontró $ZipPath. Ejecuta primero Publish-Release.ps1"
    exit 1
}

# 2. Descomprimir
Write-Host "📦 Extrayendo archivos fuente desde $ZipName..." -ForegroundColor Cyan
if (Test-Path $TempDir) { Remove-Item $TempDir -Recurse -Force }
Expand-Archive -Path $ZipPath -DestinationPath $TempDir -Force

# 3. Buscar ISCC (Compilador Inno Setup)
$InnoPath = "${env:ProgramFiles(x86)}\Inno Setup 6\ISCC.exe"
if (-not (Test-Path $InnoPath)) { 
    $InnoPath = "${env:ProgramFiles}\Inno Setup 6\ISCC.exe" 
}

if (Test-Path $InnoPath) {
    Write-Host "🛠️ Compilando setup.iss..." -ForegroundColor Green
    & $InnoPath $IssFile
    
    if ($LASTEXITCODE -eq 0) {
        Write-Host "`n✅ INSTALADOR CREADO EXITOSAMENTE" -ForegroundColor Green
        Write-Host "   📂 Ubicación: $RepoRoot\releases"
        
        # Limpiar temporales
        Remove-Item $TempDir -Recurse -Force
    }
    else {
        Write-Error "❌ Error al compilar el instalador."
    }
}
else {
    Write-Warning "`n⚠️ Inno Setup Compiler (ISCC.exe) no encontrado."
    Write-Host "   1. Descarga e instala Inno Setup 6: https://jrsoftware.org/isdl.php"
    Write-Host "   2. Ejecuta este script nuevamente."
    Write-Host "   o compila manualmente 'build\Setup.iss' apuntando a 'build\temp_installer'."
}
