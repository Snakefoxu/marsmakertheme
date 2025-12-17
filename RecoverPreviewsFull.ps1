# RecoverPreviewsFull.ps1
# Automates the full recovery of previews by:
# 1. Downloading the .photo/.smtheme file (if missing)
# 2. Extracting it using 7-Zip (password: vmax2025)
# 3. Finding the preview image (demo.*, preview.* or technician.*)
# 4. Resizing it to a small thumbnail (320px)
# 5. Saving to resources/Previews

Add-Type -AssemblyName System.Drawing

$repoRoot = "d:\REPOS_GITHUB\SnakeMarsTheme"
$catalogPath = Join-Path $repoRoot "resources\catalog.json"
$photoDir = Join-Path $repoRoot "resources\ThemesPhoto"
$smthemeDir = Join-Path $repoRoot "resources\Themes_SMTHEME"
$previewDir = Join-Path $repoRoot "resources\Previews"
$baseUrl = "https://huggingface.co/datasets/snakefoxu/soeyi-themes/resolve/main"
$7z = "${env:ProgramFiles}\7-Zip\7z.exe"
$password = "vmax2025"

if (-not (Test-Path $7z)) {
    Write-Error "7-Zip not found at $7z"
    exit 1
}

# Load Catalog
$json = Get-Content $catalogPath -Raw | ConvertFrom-Json
$items = $json.items 
if (-not $items) { $items = $json.Items }

$total = $items.Count
$current = 0

Write-Host "Starting Full Recovery for $total themes..." -ForegroundColor Cyan

foreach ($item in $items) {
    $current++
    $pct = [math]::Round(($current / $total) * 100)
    
    # Target Preview Path
    $prevRel = $item.preview -replace "/", "\"
    $targetPreview = Join-Path $repoRoot "resources\$prevRel"
    
    # Check if target exists and is valid
    # We DO NOT skip if it exists, because we want to ensure correct "demo" image is used
    # But for speed, if we just generated it in this session, maybe?
    # No, let's just do it. Safe.
    
    # 1. Determine local theme path
    $themeRel = $item.download
    $themeName = [System.IO.Path]::GetFileName($themeRel)
    $isSmtheme = $themeName.EndsWith(".smtheme")
    
    if ($isSmtheme) {
        $localTheme = Join-Path $smthemeDir $themeName
    }
    else {
        $localTheme = Join-Path $photoDir $themeName
    }
    
    # 2. Download if missing
    if (-not (Test-Path $localTheme)) {
        Write-Host "[$current/$total] Downloading $themeName..." -ForegroundColor Yellow
        $url = "$baseUrl/" + ($item.download -replace "\\", "/")
        try {
            $dir = Split-Path $localTheme
            if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }
            Invoke-WebRequest -Uri $url -OutFile $localTheme -ErrorAction Stop
        }
        catch {
            Write-Host "   Download failed: $_" -ForegroundColor Red
            continue
        }
    }
    
    # 3. Extract to Temp
    $tempDir = Join-Path $repoRoot "temp_extract_$($item.id)"
    if (Test-Path $tempDir) { Remove-Item $tempDir -Recurse -Force }
    New-Item -ItemType Directory -Path $tempDir -Force | Out-Null
    
    # Write-Host "   Extracting..." -ForegroundColor DarkGray
    $args = "e `"$localTheme`" -o`"$tempDir`" -p$password -y"
    $p = Start-Process -FilePath $7z -ArgumentList $args -NoNewWindow -PassThru -Wait
    
    if ($p.ExitCode -ne 0) {
        Write-Host "   Extraction failed (Pass?)" -ForegroundColor Red
        Remove-Item $tempDir -Recurse -Force
        continue
    }
    
    # 4. Find Image Candidate
    $images = Get-ChildItem $tempDir -Include "*.png", "*.jpg", "*.jpeg", "*.bmp" -Recurse
    
    # Priority List:
    # 1. demo.* (User specified)
    # 2. preview.*
    # 3. technician.*
    # 4. Largest image (fallback)
    
    $candidate = $images | Where-Object { $_.Name -match "^demo\." } | Select-Object -First 1
    if (-not $candidate) {
        $candidate = $images | Where-Object { $_.Name -match "^preview\." } | Select-Object -First 1
    }
    if (-not $candidate) {
        $candidate = $images | Where-Object { $_.Name -match "^technician\." } | Select-Object -First 1
    }
    if (-not $candidate) {
        # Pick largest
        $candidate = $images | Sort-Object Length -Descending | Select-Object -First 1
    }
    
    if ($candidate) {
        # 5. Resize and Save
        try {
            $img = [System.Drawing.Image]::FromFile($candidate.FullName)
            
            # Target size: 320 width (maintain aspect)
            $newWidth = 320
            $newHeight = [math]::Round(($img.Height / $img.Width) * $newWidth)
            
            $bmp = new-object System.Drawing.Bitmap $newWidth, $newHeight
            $graph = [System.Drawing.Graphics]::FromImage($bmp)
            $graph.InterpolationMode = [System.Drawing.Drawing2D.InterpolationMode]::HighQualityBicubic
            $graph.DrawImage($img, 0, 0, $newWidth, $newHeight)
            
            $destDirForce = Split-Path $targetPreview
            if (-not (Test-Path $destDirForce)) { New-Item -ItemType Directory -Path $destDirForce -Force | Out-Null }
            
            $bmp.Save($targetPreview, [System.Drawing.Imaging.ImageFormat]::Png)
            
            $img.Dispose()
            $bmp.Dispose()
            $graph.Dispose()
            
            Write-Host "   [$current/$total] Generated Preview: $($item.preview)" -ForegroundColor Green
        }
        catch {
            Write-Host "   Resize failed: $_" -ForegroundColor Red
        }
    }
    else {
        Write-Host "   No image found in theme package." -ForegroundColor Red
    }
    
    # Cleanup
    Remove-Item $tempDir -Recurse -Force
}

Write-Host "Recovery Complete!" -ForegroundColor Cyan
