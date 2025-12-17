# RestorePreviews.ps1
# Restores missing theme previews by downloading them from the official repo
# Based on resources/catalog.json

$repoRoot = "d:\REPOS_GITHUB\SnakeMarsTheme"
$catalogPath = Join-Path $repoRoot "resources\catalog.json"
$baseUrl = "https://huggingface.co/datasets/snakefoxu/soeyi-themes/resolve/main"

if (-not (Test-Path $catalogPath)) {
    Write-Error "Catalog not found at $catalogPath"
    exit 1
}

$json = Get-Content $catalogPath -Raw | ConvertFrom-Json

# Access the 'items' property properly (case insensitive check usually handled by Powershell, but explicit is better)
$items = $json.items 
if (-not $items) { 
    $items = $json.Items
}

$total = $items.Count
$current = 0

Write-Host "Found $total themes in catalog. Checking previews..." -ForegroundColor Cyan

foreach ($item in $items) {
    $current++
    $relPath = $item.preview
    if (-not $relPath) { continue }

    # Normalize path separators
    $relPath = $item.preview -replace "/", "\"
    $localPath = Join-Path $repoRoot "resources\$relPath"

    # Try lowercase first (default from json)
    $remoteUrl = "$baseUrl/" + ($item.preview -replace "\\", "/")
    
    $dir = Split-Path $localPath
    if (-not (Test-Path $dir)) { New-Item -ItemType Directory -Path $dir -Force | Out-Null }

    # Skip if exists and size > 0 (Success)
    if (Test-Path $localPath) {
        if ((Get-Item $localPath).Length -gt 0) {
            # Write-Host "Skipping existing: $($item.preview)" -ForegroundColor DarkGray
            continue
        }
    }

    Write-Host "[$current/$total] Restoring: $($item.preview)..." -ForegroundColor Yellow
    try {
        Invoke-WebRequest -Uri $remoteUrl -OutFile $localPath -ErrorAction Stop
    }
    catch {
        # Try Capitalized 'Previews' (HuggingFace)
        try {
            $capUrl = "$baseUrl/" + ($item.preview -replace "^previews/", "Previews/" -replace "\\", "/")
            Invoke-WebRequest -Uri $capUrl -OutFile $localPath -ErrorAction Stop
            Write-Host "   -> Recovered using Capitalized path!" -ForegroundColor Green
        }
        catch {
            # Try GitHub Raw (Repo Source) - snakefoxu/marsmakertheme
            try {
                $githubUrl = "https://raw.githubusercontent.com/Snakefoxu/marsmakertheme/main/resources/Previews/" + ($item.preview -replace "previews/", "" -replace "\\", "/")
                Invoke-WebRequest -Uri $githubUrl -OutFile $localPath -ErrorAction Stop
                Write-Host "   -> Recovered from GitHub!" -ForegroundColor Cyan
            }
            catch {
                Write-Host "   Failed to download (HF & GH): $_" -ForegroundColor Red
            }
        }
    }
}

Write-Host "Preview restoration complete." -ForegroundColor Green
