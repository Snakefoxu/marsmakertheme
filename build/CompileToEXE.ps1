# Script para compilar SnakeMarsTheme a EXE
# Requiere ps2exe: Install-Module -Name ps2exe -Scope CurrentUser

param(
    [switch]$Install
)

Write-Host "=== Compilador SnakeMarsTheme a EXE ===" -ForegroundColor Cyan
Write-Host ""

# Verificar si ps2exe esta instalado
$ps2exeInstalled = Get-Command Invoke-ps2exe -ErrorAction SilentlyContinue

if (-not $ps2exeInstalled) {
    Write-Host "[!] ps2exe no esta instalado" -ForegroundColor Yellow
    
    if ($Install) {
        Write-Host "[*] Instalando ps2exe..." -ForegroundColor Cyan
        try {
            Install-Module -Name ps2exe -Scope CurrentUser -Force
            Write-Host "[+] ps2exe instalado correctamente" -ForegroundColor Green
        }
        catch {
            Write-Host "[!] Error instalando ps2exe: $_" -ForegroundColor Red
            Write-Host ""
            Write-Host "Instala manualmente con:" -ForegroundColor Yellow
            Write-Host "  Install-Module -Name ps2exe -Scope CurrentUser" -ForegroundColor White
            exit 1
        }
    }
    else {
        Write-Host ""
        Write-Host "Para instalar ps2exe ejecuta:" -ForegroundColor Yellow
        Write-Host "  .\build\CompileToEXE.ps1 -Install" -ForegroundColor White
        Write-Host ""
        Write-Host "O manualmente:" -ForegroundColor Yellow
        Write-Host "  Install-Module -Name ps2exe -Scope CurrentUser" -ForegroundColor White
        exit 1
    }
}

Write-Host "[+] ps2exe encontrado" -ForegroundColor Green
Write-Host ""

# Rutas
$scriptPath = Join-Path $PSScriptRoot "..\src\SnakeMarsTheme.ps1"
$outputPath = Join-Path $PSScriptRoot "..\bin\SnakeMarsTheme.exe"
$iconPath = Join-Path $PSScriptRoot "..\resources\icon.ico"

# Crear carpeta bin si no existe
$binFolder = Join-Path $PSScriptRoot "..\bin"
if (-not (Test-Path $binFolder)) {
    New-Item -ItemType Directory -Path $binFolder -Force | Out-Null
    Write-Host "[+] Carpeta bin creada" -ForegroundColor Green
}

# Verificar que el script existe
if (-not (Test-Path $scriptPath)) {
    Write-Host "[!] No se encontro el script: $scriptPath" -ForegroundColor Red
    exit 1
}

Write-Host "[*] Script encontrado: $scriptPath" -ForegroundColor Cyan
Write-Host "[*] Compilando a: $outputPath" -ForegroundColor Cyan
Write-Host ""

# Parametros de compilacion
$compileParams = @{
    InputFile = $scriptPath
    OutputFile = $outputPath
    NoConsole = $false  # Mantener consola para ver logs
    Title = "SnakeMarsTheme - Editor de Temas"
    Description = "Editor de temas para pantallas Mars Gaming VMAX"
    Company = "SnakeFoxu"
    Product = "SnakeMarsTheme"
    Copyright = "© 2025 SnakeFoxu"
    Version = "1.0.0.0"
    RequireAdmin = $false
    SupportOS = $true
    X64 = $true
}

# Agregar icono si existe
if (Test-Path $iconPath) {
    $compileParams.IconFile = $iconPath
    Write-Host "[+] Icono encontrado: $iconPath" -ForegroundColor Green
}
else {
    Write-Host "[!] No se encontro icono en: $iconPath" -ForegroundColor Yellow
    Write-Host "    El EXE se compilara sin icono personalizado" -ForegroundColor Gray
}

Write-Host ""
Write-Host "[*] Compilando..." -ForegroundColor Cyan

try {
    Invoke-ps2exe @compileParams
    
    if (Test-Path $outputPath) {
        Write-Host ""
        Write-Host "[+] COMPILACION EXITOSA!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Ejecutable creado en:" -ForegroundColor Cyan
        Write-Host "  $outputPath" -ForegroundColor White
        Write-Host ""
        
        # Mostrar tamaño del archivo
        $fileInfo = Get-Item $outputPath
        $sizeMB = [math]::Round($fileInfo.Length / 1MB, 2)
        Write-Host "Tamaño: $sizeMB MB" -ForegroundColor Gray
        Write-Host ""
        
        # Preguntar si quiere ejecutar
        $response = Read-Host "¿Deseas ejecutar el programa ahora? (S/N)"
        if ($response -eq "S" -or $response -eq "s") {
            Start-Process $outputPath
        }
    }
    else {
        Write-Host "[!] El archivo EXE no se creo correctamente" -ForegroundColor Red
    }
}
catch {
    Write-Host ""
    Write-Host "[!] ERROR EN LA COMPILACION:" -ForegroundColor Red
    Write-Host $_.Exception.Message -ForegroundColor Red
    Write-Host ""
    Write-Host "Detalles del error:" -ForegroundColor Yellow
    Write-Host $_ -ForegroundColor Gray
    exit 1
}
