using System.Net.Http;
using System.Text.Json;
using System.IO;

namespace SnakeMarsTheme.Services;

/// <summary>
/// Service for downloading themes from HuggingFace.
/// </summary>
public class DownloadService
{
    private readonly HttpClient _httpClient;
    private readonly string _downloadPath;
    private readonly string _basePath;
    private const string HUGGINGFACE_RAW = "https://huggingface.co/datasets/snakefoxu/soeyi-themes/resolve/main";
    
    public DownloadService(string basePath)
    {
        _basePath = basePath;
        _httpClient = new HttpClient();
        _httpClient.DefaultRequestHeaders.Add("User-Agent", "SnakeMarsTheme/4.0");
        _downloadPath = Path.Combine(basePath, "resources", "ThemesPhoto");
        
        // Ensure standard theme directories exist
        var smthemePath = Path.Combine(basePath, "resources", "Themes_SMTHEME");
        Directory.CreateDirectory(_downloadPath);
        Directory.CreateDirectory(smthemePath);
    }
    
    /// <summary>
    /// Get list of available themes from LOCAL catalog.json.
    /// Themes are downloaded from HuggingFace on demand.
    /// </summary>
    public async Task<List<RemoteTheme>> GetAvailableThemesAsync()
    {
        var themes = new List<RemoteTheme>();
        
        try
        {
            // Read from LOCAL catalog.json (no network needed for browsing)
            var catalogPath = Path.Combine(_basePath, "resources", "catalog.json");
            
            if (!File.Exists(catalogPath))
            {
                System.Diagnostics.Debug.WriteLine($"Catalog not found: {catalogPath}");
                return themes;
            }
            
            var json = await File.ReadAllTextAsync(catalogPath);
            var catalog = JsonSerializer.Deserialize<CatalogRoot>(json, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            
            if (catalog?.Items != null)
            {
                foreach (var item in catalog.Items)
                {
                    themes.Add(new RemoteTheme
                    {
                        Id = GetIdFromItem(item),
                        Name = item.Name ?? "Unknown",
                        FileName = Path.GetFileName(item.Download) ?? "",
                        Size = item.Size,
                        DownloadUrl = $"{HUGGINGFACE_RAW}/{item.Download}",
                        ThumbnailUrl = $"{HUGGINGFACE_RAW}/{item.Preview}",
                        Resolution = item.Resolution ?? GetResolutionFromTags(item.Tags),
                        Type = item.Type ?? "photo",
                        Source = item.Source ?? "unknown",
                        Tags = item.Tags ?? new List<string>()
                    });
                }
            }
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Error loading catalog: {ex.Message}");
        }
        
        return themes.OrderBy(t => t.Name).ToList();
    }

    private int GetIdFromItem(CatalogItem item)
    {
        // Try to parse ID as int, otherwise return hash or 0
        if (int.TryParse(item.Id, out int id)) return id;
        return Math.Abs(item.Id?.GetHashCode() ?? 0);
    }

    private string GetResolutionFromTags(List<string>? tags)
    {
        if (tags == null) return "Unknown";
        // Try to find a tag that looks like a resolution (e.g. 320x240)
        return tags.FirstOrDefault(t => t.Contains("x") && t.Any(char.IsDigit)) ?? "Unknown";
    }
    
    /// <summary>
    /// Download a theme file.
    /// </summary>
    public async Task<bool> DownloadThemeAsync(RemoteTheme theme, IProgress<int>? progress = null)
    {
        try
        {
            // Determine destination based on type
            string destFolder;
            if (theme.Type == "smtheme")
            {
                // Go to resources/Themes_SMTHEME
                destFolder = Path.Combine(Path.GetDirectoryName(_downloadPath)!, "Themes_SMTHEME");
            }
            else
            {
                // Go to resources/ThemesPhoto
                destFolder = _downloadPath;
            }

            if (!Directory.Exists(destFolder)) Directory.CreateDirectory(destFolder);

            var destPath = Path.Combine(destFolder, theme.FileName);
            
            using var response = await _httpClient.GetAsync(theme.DownloadUrl, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            
            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var downloadedBytes = 0L;
            
            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(destPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            
            var buffer = new byte[8192];
            int bytesRead;
            
            while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                downloadedBytes += bytesRead;
                
                if (totalBytes > 0 && progress != null)
                {
                    var percent = (int)((downloadedBytes * 100) / totalBytes);
                    progress.Report(percent);
                }
            }
            
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Download error: {ex.Message}");
            return false;
        }
    }
    
    /// <summary>
    /// Check if theme is already downloaded.
    /// </summary>
    public bool IsDownloaded(RemoteTheme theme)
    {
        string destFolder;
        if (theme.Type == "smtheme")
        {
             destFolder = Path.Combine(Path.GetDirectoryName(_downloadPath)!, "Themes_SMTHEME");
        }
        else
        {
             destFolder = _downloadPath;
        }
        var path = Path.Combine(destFolder, theme.FileName);
        return File.Exists(path);
    }
    
    public bool IsDownloaded(string fileName) 
    {
        // Legacy overload, assumes .photo
        return File.Exists(Path.Combine(_downloadPath, fileName));
    }

    /// <summary>
    /// Download a file from a URL to a specific path.
    /// </summary>
    public async Task<bool> DownloadFileAsync(string url, string destinationPath, IProgress<int>? progress = null)
    {
        try
        {
            // Ensure directory exists
            var dir = Path.GetDirectoryName(destinationPath);
            if (!string.IsNullOrEmpty(dir) && !Directory.Exists(dir))
                Directory.CreateDirectory(dir);
            
            using var response = await _httpClient.GetAsync(url, HttpCompletionOption.ResponseHeadersRead);
            response.EnsureSuccessStatusCode();
            
            var totalBytes = response.Content.Headers.ContentLength ?? -1L;
            var downloadedBytes = 0L;
            
            using var contentStream = await response.Content.ReadAsStreamAsync();
            using var fileStream = new FileStream(destinationPath, FileMode.Create, FileAccess.Write, FileShare.None, 8192, true);
            
            var buffer = new byte[8192];
            int bytesRead;
            
            while ((bytesRead = await contentStream.ReadAsync(buffer)) > 0)
            {
                await fileStream.WriteAsync(buffer.AsMemory(0, bytesRead));
                downloadedBytes += bytesRead;
                
                if (totalBytes > 0 && progress != null)
                {
                    var percent = (int)((downloadedBytes * 100) / totalBytes);
                    progress.Report(percent);
                }
            }
            
            return true;
        }
        catch (Exception ex)
        {
            System.Diagnostics.Debug.WriteLine($"Download error: {ex.Message}");
            return false;
        }
    }
}

public class RemoteTheme
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public string FileName { get; set; } = "";
    public long Size { get; set; }
    public string DownloadUrl { get; set; } = "";
    public string? Url { get; set; } // Alias for DownloadUrl if needed, or separate
    public string? ThumbnailUrl { get; set; }
    public string Resolution { get; set; } = "";
    
    // New fields for v4.1
    public string Type { get; set; } = "photo"; // "photo" or "smtheme"
    public string Source { get; set; } = "unknown";
    public List<string> Tags { get; set; } = new();

    public string TypeIcon => Type == "smtheme" ? "📦" : "🖼️";
    
    public string SizeFormatted => Size switch
    {
        < 1024 => $"{Size} B",
        < 1024 * 1024 => $"{Size / 1024.0:F1} KB",
        _ => $"{Size / (1024.0 * 1024.0):F1} MB"
    };
}

internal class CatalogRoot
{
    public string? Version { get; set; }
    public List<CatalogItem>? Items { get; set; }
}

internal class CatalogItem
{
    public string? Id { get; set; }
    public string? Name { get; set; }
    public string? Type { get; set; }
    public string? Source { get; set; }
    public string? Download { get; set; }
    public string? Preview { get; set; }
    public string? Resolution { get; set; }
    public long Size { get; set; }
    public List<string>? Tags { get; set; }
}
