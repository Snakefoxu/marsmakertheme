# Compilacion rapida sin interaccion
Write-Host "=== Compilando SnakeMarsTheme v1.0.2 ===" -ForegroundColor Cyan
Write-Host ""

# Rutas
$repoPath = "D:\REPOS_GITHUB\SnakeMarsTheme"
$scriptPath = Join-Path $repoPath "src\SnakeMarsTheme.ps1"
$outputPath = Join-Path $repoPath "bin\SnakeMarsTheme.exe"
$binPath = Join-Path $repoPath "bin"

# Verificar que el script fuente existe
if (-not (Test-Path $scriptPath)) {
    Write-Host "[ERROR] No se encontro el script fuente:" -ForegroundColor Red
    Write-Host "  $scriptPath" -ForegroundColor Gray
    exit 1
}

Write-Host "[+] Script fuente encontrado" -ForegroundColor Green
Write-Host "  $scriptPath" -ForegroundColor Gray
Write-Host ""

# Verificar que ps2exe esta disponible
$ps2exe = Get-Command Invoke-ps2exe -ErrorAction SilentlyContinue
if (-not $ps2exe) {
    Write-Host "[!] ps2exe no esta instalado" -ForegroundColor Yellow
    Write-Host "    Instalando..." -ForegroundColor Cyan
    try {
        Install-Module -Name ps2exe -Scope CurrentUser -Force
        Import-Module ps2exe
        Write-Host "[+] ps2exe instalado correctamente" -ForegroundColor Green
    }
    catch {
        Write-Host "[ERROR] No se pudo instalar ps2exe: $_" -ForegroundColor Red
        exit 1
    }
}

Write-Host "[+] ps2exe disponible" -ForegroundColor Green
Write-Host ""

# Cerrar instancias existentes del programa
Write-Host "[*] Cerrando instancias existentes..." -ForegroundColor Cyan
$processes = Get-Process -Name "SnakeMarsTheme" -ErrorAction SilentlyContinue
if ($processes) {
    $processes | ForEach-Object {
        Stop-Process -Id $_.Id -Force -ErrorAction SilentlyContinue
    }
    Start-Sleep -Seconds 2
    Write-Host "[+] Instancias cerradas" -ForegroundColor Green
} else {
    Write-Host "[+] No hay instancias en ejecucion" -ForegroundColor Green
}
Write-Host ""

# Crear carpeta bin si no existe
if (-not (Test-Path $binPath)) {
    New-Item -ItemType Directory -Path $binPath -Force | Out-Null
    Write-Host "[+] Carpeta bin creada" -ForegroundColor Green
} else {
    Write-Host "[+] Carpeta bin existe" -ForegroundColor Green
}

Write-Host ""
Write-Host "[*] Compilando..." -ForegroundColor Cyan
Write-Host "    Entrada:  $scriptPath" -ForegroundColor Gray
Write-Host "    Salida:   $outputPath" -ForegroundColor Gray
Write-Host ""

try {
    # Compilar con parametros completos
    Invoke-ps2exe `
        -InputFile $scriptPath `
        -OutputFile $outputPath `
        -NoConsole:$false `
        -Title "SnakeMarsTheme - Editor de Temas v1.0.2" `
        -Description "Editor de temas para pantallas Mars Gaming VMAX" `
        -Company "SnakeFoxu" `
        -Product "SnakeMarsTheme" `
        -Copyright "© 2025 SnakeFoxu" `
        -Version "1.0.2.0" `
        -X64 `
        -SupportOS
    
    Write-Host ""
    
    # Verificar que el EXE se creo
    if (Test-Path $outputPath) {
        $fileInfo = Get-Item $outputPath
        $sizeMB = [math]::Round($fileInfo.Length / 1MB, 2)
        
        Write-Host "[OK] COMPILACION EXITOSA!" -ForegroundColor Green
        Write-Host ""
        Write-Host "Ejecutable creado:" -ForegroundColor Cyan
        Write-Host "  Ruta:    $outputPath" -ForegroundColor White
        Write-Host "  Tamaño:  $sizeMB MB" -ForegroundColor White
        Write-Host "  Version: 1.0.2.0" -ForegroundColor White
        Write-Host ""
        Write-Host "CAMBIOS EN v1.0.2:" -ForegroundColor Yellow
        Write-Host "  - Deteccion ultra-robusta de rutas (5 metodos)" -ForegroundColor Gray
        Write-Host "  - Fallback a ruta hardcoded si falla todo" -ForegroundColor Gray
        Write-Host "  - Busqueda automatica de resources en multiples ubicaciones" -ForegroundColor Gray
        Write-Host "  - Validaciones nulas en todas las operaciones Path" -ForegroundColor Gray
        Write-Host ""
        
        # Abrir carpeta
        Start-Process explorer.exe -ArgumentList "/select,`"$outputPath`""
        
    } else {
        Write-Host "[ERROR] El archivo EXE no se creo" -ForegroundColor Red
        exit 1
    }
}
catch {
    Write-Host ""
    Write-Host "[ERROR] Fallo en la compilacion:" -ForegroundColor Red
    Write-Host "  $_" -ForegroundColor Gray
    exit 1
}
