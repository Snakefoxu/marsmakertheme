using System.IO;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace SnakeMarsTheme.Services;

/// <summary>
/// Service for loading the local theme catalog from themes_index.json
/// </summary>
public class ThemeCatalogService
{
    private readonly string _indexPath;
    private ThemeCatalog? _catalog;
    
    public ThemeCatalogService(string basePath)
    {
        _indexPath = Path.Combine(basePath, "resources", "themes_index.json");
    }
    
    public async Task<ThemeCatalog?> LoadCatalogAsync()
    {
        if (!File.Exists(_indexPath))
            return null;
            
        try
        {
            var json = await File.ReadAllTextAsync(_indexPath);
            _catalog = JsonSerializer.Deserialize<ThemeCatalog>(json, new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
            return _catalog;
        }
        catch
        {
            return null;
        }
    }
    
    public ThemeCatalog? GetCatalog() => _catalog;
    
    public List<CatalogTheme> FilterByResolution(string resolution)
    {
        if (_catalog == null || string.IsNullOrEmpty(resolution) || resolution == "Todas")
            return _catalog?.Themes ?? new List<CatalogTheme>();
            
        // Extract just the resolution part (e.g., "320x240" from "320x240 (AIO)")
        var resOnly = resolution.Split(' ')[0];
        return _catalog.Themes.Where(t => t.Resolution == resOnly).ToList();
    }
    
    public int TotalCount => _catalog?.Themes.Count ?? 0;
    public string Password => _catalog?.Password ?? "vmax2025";
}

public class ThemeCatalog
{
    [JsonPropertyName("password")]
    public string Password { get; set; } = "vmax2025";
    
    [JsonPropertyName("lastScannedId")]
    public int LastScannedId { get; set; }
    
    [JsonPropertyName("themes")]
    public List<CatalogTheme> Themes { get; set; } = new();
}

public class CatalogTheme
{
    [JsonPropertyName("id")]
    public int Id { get; set; }
    
    [JsonPropertyName("name")]
    public string Name { get; set; } = "";
    
    [JsonPropertyName("resolution")]
    public string Resolution { get; set; } = "";
    
    [JsonPropertyName("downloadUrl")]
    public string DownloadUrl { get; set; } = "";
    
    [JsonPropertyName("thumbnail")]
    public string? Thumbnail { get; set; }
}
