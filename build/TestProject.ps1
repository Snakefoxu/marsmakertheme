# Script de Prueba - SnakeMarsTheme v1.0.1

Write-Host "========================================" -ForegroundColor Cyan
Write-Host " Test de SnakeMarsTheme v1.0.1" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$repoPath = "D:\REPOS_GITHUB\SnakeMarsTheme"
$tests = @()

# Test 1: Verificar estructura de carpetas
Write-Host "[TEST 1] Verificando estructura de carpetas..." -ForegroundColor Yellow
$requiredPaths = @{
    "bin/" = Join-Path $repoPath "bin"
    "src/" = Join-Path $repoPath "src"
    "resources/" = Join-Path $repoPath "resources"
    "resources/ThemeScheme/" = Join-Path $repoPath "resources\ThemeScheme"
    "resources/Programme/" = Join-Path $repoPath "resources\Programme"
    "resources/Gif/" = Join-Path $repoPath "resources\Gif"
}

$allPathsExist = $true
foreach ($path in $requiredPaths.GetEnumerator()) {
    if (Test-Path $path.Value) {
        Write-Host "  [OK] $($path.Key)" -ForegroundColor Green
    } else {
        Write-Host "  [FAIL] $($path.Key) no existe" -ForegroundColor Red
        $allPathsExist = $false
    }
}
$tests += @{Name = "Estructura de carpetas"; Result = $allPathsExist}
Write-Host ""

# Test 2: Verificar archivos clave
Write-Host "[TEST 2] Verificando archivos clave..." -ForegroundColor Yellow
$requiredFiles = @{
    "SnakeMarsTheme.exe" = Join-Path $repoPath "bin\SnakeMarsTheme.exe"
    "SnakeMarsTheme.ps1" = Join-Path $repoPath "src\SnakeMarsTheme.ps1"
    "PreviewAnimatedTheme.ps1" = Join-Path $repoPath "src\PreviewAnimatedTheme.ps1"
}

$allFilesExist = $true
foreach ($file in $requiredFiles.GetEnumerator()) {
    if (Test-Path $file.Value) {
        $fileInfo = Get-Item $file.Value
        $sizeMB = [math]::Round($fileInfo.Length / 1MB, 2)
        Write-Host "  [OK] $($file.Key) ($sizeMB MB)" -ForegroundColor Green
    } else {
        Write-Host "  [FAIL] $($file.Key) no existe" -ForegroundColor Red
        $allFilesExist = $false
    }
}
$tests += @{Name = "Archivos clave"; Result = $allFilesExist}
Write-Host ""

# Test 3: Contar temas JSON
Write-Host "[TEST 3] Contando temas JSON..." -ForegroundColor Yellow
$themesPath = Join-Path $repoPath "resources\ThemeScheme"
$themeFiles = Get-ChildItem $themesPath -Filter "*.json" -File -ErrorAction SilentlyContinue
$themeCount = $themeFiles.Count

if ($themeCount -gt 0) {
    Write-Host "  [OK] $themeCount temas encontrados" -ForegroundColor Green
    $tests += @{Name = "Temas JSON"; Result = $true}
} else {
    Write-Host "  [FAIL] No se encontraron temas" -ForegroundColor Red
    $tests += @{Name = "Temas JSON"; Result = $false}
}
Write-Host ""

# Test 4: Verificar tema WPF animado
Write-Host "[TEST 4] Verificando tema WPF animado..." -ForegroundColor Yellow
$animatedTheme = Join-Path $themesPath "AnimatedTheme_Complete.json"
if (Test-Path $animatedTheme) {
    try {
        $json = Get-Content $animatedTheme -Raw -Encoding UTF8 | ConvertFrom-Json
        $hasComponents = $null -ne $json.Components
        $hasAnimTypes = $null -ne $json.AnimationTypes
        
        if ($hasComponents -and $hasAnimTypes) {
            Write-Host "  [OK] Tema WPF con $($json.Components.Count) componentes" -ForegroundColor Green
            $tests += @{Name = "Tema WPF animado"; Result = $true}
        } else {
            Write-Host "  [FAIL] Tema WPF invalido" -ForegroundColor Red
            $tests += @{Name = "Tema WPF animado"; Result = $false}
        }
    }
    catch {
        Write-Host "  [FAIL] Error leyendo tema: $_" -ForegroundColor Red
        $tests += @{Name = "Tema WPF animado"; Result = $false}
    }
} else {
    Write-Host "  [FAIL] AnimatedTheme_Complete.json no existe" -ForegroundColor Red
    $tests += @{Name = "Tema WPF animado"; Result = $false}
}
Write-Host ""

# Test 5: Verificar que el EXE es la version correcta
Write-Host "[TEST 5] Verificando version del EXE..." -ForegroundColor Yellow
$exePath = Join-Path $repoPath "bin\SnakeMarsTheme.exe"
if (Test-Path $exePath) {
    $fileInfo = Get-Item $exePath
    $lastWrite = $fileInfo.LastWriteTime
    $now = Get-Date
    $minutesAgo = ($now - $lastWrite).TotalMinutes
    
    if ($minutesAgo -lt 60) {
        Write-Host "  [OK] EXE compilado hace $([math]::Round($minutesAgo, 1)) minutos" -ForegroundColor Green
        $tests += @{Name = "Version del EXE"; Result = $true}
    } else {
        Write-Host "  [WARN] EXE compilado hace $([math]::Round($minutesAgo / 60, 1)) horas" -ForegroundColor Yellow
        Write-Host "         Considera recompilar con: .\build\CompileQuick.ps1" -ForegroundColor Gray
        $tests += @{Name = "Version del EXE"; Result = $true}
    }
} else {
    Write-Host "  [FAIL] EXE no existe" -ForegroundColor Red
    $tests += @{Name = "Version del EXE"; Result = $false}
}
Write-Host ""

# Resumen
Write-Host "========================================" -ForegroundColor Cyan
Write-Host " RESUMEN DE TESTS" -ForegroundColor Cyan
Write-Host "========================================" -ForegroundColor Cyan
Write-Host ""

$passed = ($tests | Where-Object { $_.Result -eq $true }).Count
$total = $tests.Count

foreach ($test in $tests) {
    $icon = if ($test.Result) { "[OK]" } else { "[FAIL]" }
    $color = if ($test.Result) { "Green" } else { "Red" }
    Write-Host "  $icon $($test.Name)" -ForegroundColor $color
}

Write-Host ""
Write-Host "Resultado: $passed/$total tests pasados" -ForegroundColor $(if ($passed -eq $total) { "Green" } else { "Yellow" })
Write-Host ""

if ($passed -eq $total) {
    Write-Host "[OK] Todos los tests pasaron! El proyecto esta listo para usar." -ForegroundColor Green
    Write-Host ""
    Write-Host "Ejecuta el programa con:" -ForegroundColor Cyan
    Write-Host "  .\bin\SnakeMarsTheme.exe" -ForegroundColor White
} else {
    Write-Host "[!] Algunos tests fallaron. Revisa los errores arriba." -ForegroundColor Yellow
    Write-Host ""
    Write-Host "Para recompilar:" -ForegroundColor Cyan
    Write-Host "  .\build\CompileQuick.ps1" -ForegroundColor White
}

Write-Host ""
