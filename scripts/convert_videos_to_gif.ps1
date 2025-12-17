# Script para convertir todos los videos a GIF con máxima calidad
# Usa FFmpeg con paleta optimizada para mejor calidad

$videoDir = "d:\REPOS_GITHUB\SnakeMarsTheme\resources\Videos"
$gifDir = "d:\REPOS_GITHUB\SnakeMarsTheme\resources\GIFs\Converted"
$ffmpeg = "$env:LOCALAPPDATA\SnakeMarsTheme\FFmpeg\ffmpeg.exe"

# Crear directorio de salida
New-Item -ItemType Directory -Force -Path $gifDir | Out-Null

# Obtener todos los videos
$videos = Get-ChildItem -Path $videoDir -Filter "*.mp4"

Write-Host "Encontrados $($videos.Count) videos para convertir" -ForegroundColor Cyan
Write-Host "FFmpeg: $ffmpeg" -ForegroundColor Gray
Write-Host ""

$converted = 0
$failed = 0

foreach ($video in $videos) {
    $gifName = [System.IO.Path]::GetFileNameWithoutExtension($video.Name) + ".gif"
    $gifPath = Join-Path $gifDir $gifName
    
    # Skip si ya existe
    if (Test-Path $gifPath) {
        Write-Host "  [SKIP] $gifName ya existe" -ForegroundColor Yellow
        continue
    }
    
    Write-Host "  Convirtiendo: $($video.Name)..." -NoNewline
    
    # Comando FFmpeg con paleta optimizada para máxima calidad
    # -vf "fps=15,scale=480:-1" = 15 fps, ancho 480px
    # split[s0][s1] + palettegen + paletteuse = paleta optimizada
    $args = "-i `"$($video.FullName)`" -t 5 -vf `"fps=15,scale=480:-1:flags=lanczos,split[s0][s1];[s0]palettegen=max_colors=256:stats_mode=full[p];[s1][p]paletteuse=dither=bayer:bayer_scale=3`" -y `"$gifPath`""
    
    try {
        $process = Start-Process -FilePath $ffmpeg -ArgumentList $args -NoNewWindow -Wait -PassThru -RedirectStandardError "$env:TEMP\ffmpeg_error.txt"
        
        if ($process.ExitCode -eq 0 -and (Test-Path $gifPath)) {
            $size = [math]::Round((Get-Item $gifPath).Length / 1MB, 2)
            Write-Host " OK ($size MB)" -ForegroundColor Green
            $converted++
        } else {
            Write-Host " FAILED" -ForegroundColor Red
            $failed++
        }
    } catch {
        Write-Host " ERROR: $_" -ForegroundColor Red
        $failed++
    }
}

Write-Host ""
Write-Host "========================================" -ForegroundColor Cyan
Write-Host "Convertidos: $converted" -ForegroundColor Green
Write-Host "Fallidos: $failed" -ForegroundColor Red
Write-Host "GIFs en: $gifDir" -ForegroundColor Cyan
