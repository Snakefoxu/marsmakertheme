# GeneratePreviews.ps1
# Generates PNG previews for all GIFs in resources/GIFs folder
# utilizing WPF's System.Windows.Media.Imaging

Add-Type -AssemblyName PresentationCore
Add-Type -AssemblyName PresentationFramework
Add-Type -AssemblyName WindowsBase
Add-Type -AssemblyName System.Drawing

$repoRoot = "d:\REPOS_GITHUB\SnakeMarsTheme"
$gifDir = Join-Path $repoRoot "resources\GIFs"
$previewDir = Join-Path $repoRoot "resources\Previews"

# 1. Clean existing previews
Write-Host "Cleaning Previews folder..." -ForegroundColor Yellow
if (Test-Path $previewDir) {
    Remove-Item "$previewDir\*.png" -Force -ErrorAction SilentlyContinue
}
else {
    New-Item -ItemType Directory -Path $previewDir -Force | Out-Null
}

$gifs = Get-ChildItem $gifDir -Filter "*.gif"
$total = $gifs.Count
$current = 0

foreach ($gif in $gifs) {
    $current++
    $percent = [math]::Round(($current / $total) * 100)
    Write-Progress -Activity "Generating Previews" -Status "Processing $($gif.Name)" -PercentComplete $percent

    try {
        $previewName = [System.IO.Path]::GetFileNameWithoutExtension($gif.Name) + ".png"
        $previewPath = Join-Path $previewDir $previewName

        # Open GIF stream
        $stream = [System.IO.File]::OpenRead($gif.FullName)
        $decoder = [System.Windows.Media.Imaging.GifBitmapDecoder]::new($stream, 
            [System.Windows.Media.Imaging.BitmapCreateOptions]::PreservePixelFormat, 
            [System.Windows.Media.Imaging.BitmapCacheOption]::OnLoad)

        # Get first frame
        $frame = $decoder.Frames[0]

        # Save as PNG
        $encoder = [System.Windows.Media.Imaging.PngBitmapEncoder]::new()
        $encoder.Frames.Add($frame)

        $outStream = [System.IO.File]::Create($previewPath)
        $encoder.Save($outStream)
        
        $outStream.Dispose()
        $stream.Dispose()
        
        Write-Host "[$current/$total] Generated: $previewName" -ForegroundColor DarkGray
    }
    catch {
        Write-Host "Error processing $($gif.Name): $($_.Exception.Message)" -ForegroundColor Red
    }
}

Write-Host "Done! Generated $current previews." -ForegroundColor Green
