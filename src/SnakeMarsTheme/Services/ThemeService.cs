using System.IO;
using Newtonsoft.Json;
using SnakeMarsTheme.Models;

namespace SnakeMarsTheme.Services;

/// <summary>
/// Service for loading, saving, and managing themes.
/// </summary>
public class ThemeService
{
    private readonly List<string> _basePaths = new();
    private readonly SettingParser _settingParser;
    
    public string BasePath => _basePaths.FirstOrDefault() ?? "";
    
    public ThemeService(string basePath, string? userPath = null)
    {
        if (!string.IsNullOrEmpty(basePath) && Directory.Exists(basePath))
            _basePaths.Add(basePath);
            
        if (!string.IsNullOrEmpty(userPath) && Directory.Exists(userPath))
            _basePaths.Add(userPath);
            
        _settingParser = new SettingParser();
    }
    
    /// <summary>
    /// Get all themes from the ThemeScheme folder in all search paths.
    /// </summary>
    public List<ThemeInfo> GetAllThemes()
    {
        var themes = new List<ThemeInfo>();
        var processedFiles = new HashSet<string>(); // Evitar duplicados exactos de archivo

        foreach (var baseDir in _basePaths)
        {
            var themeSchemePath = Path.Combine(baseDir, "resources", "ThemeScheme");
            
            if (!Directory.Exists(themeSchemePath))
                continue;
            
            foreach (var jsonFile in Directory.GetFiles(themeSchemePath, "*.json"))
            {
                if (processedFiles.Contains(jsonFile)) continue;
                processedFiles.Add(jsonFile);
                
                try
                {
                    var json = File.ReadAllText(jsonFile);
                    var theme = JsonConvert.DeserializeObject<ThemeJson>(json);
                    
                    if (theme != null && !string.IsNullOrWhiteSpace(theme.Name))
                    {
                        themes.Add(new ThemeInfo
                        {
                            Name = theme.Name,
                            Width = (int)(theme.Width ?? 360),
                            Height = (int)(theme.Height ?? 960),
                            Type = theme.Type ?? 0,
                            FilePath = jsonFile,
                            ThumbnailData = theme.ThumbnailImageData
                        });
                    }
                }
                catch
                {
                    // Skip invalid JSON files
                }
            }
        }
        
        return themes.OrderBy(t => t.Name).ToList();
    }
    
    /// <summary>
    /// Get the thumbnail image path for a theme.
    /// </summary>
    public string? GetThumbnailPath(string themeName)
    {
        var searchFiles = new[] { "demo.png", "back.png", "1.png" };
        
        foreach (var baseDir in _basePaths)
        {
            var programmeFolder = Path.Combine(baseDir, "resources", "Programme", themeName);
            
            if (!Directory.Exists(programmeFolder)) continue;

            foreach (var file in searchFiles)
            {
                var path = Path.Combine(programmeFolder, file);
                if (File.Exists(path))
                    return path;
            }
            
            // Search in source subfolder
            var sourceFolder = Path.Combine(programmeFolder, "source");
            if (Directory.Exists(sourceFolder))
            {
                foreach (var file in searchFiles)
                {
                    var path = Path.Combine(sourceFolder, file);
                    if (File.Exists(path))
                        return path;
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Load a full theme from Setting.txt or JSON.
    /// </summary>
    public Theme? LoadTheme(string jsonPath)
    {
        if (!File.Exists(jsonPath))
            return null;
        
        try
        {
            var json = File.ReadAllText(jsonPath);
            var themeJson = JsonConvert.DeserializeObject<ThemeJson>(json);
            
            if (themeJson == null)
                return null;
            
            // Check for Setting.txt
            var themeName = Path.GetFileNameWithoutExtension(jsonPath);
            
            // Inferir ruta de Programme relativa al JSON (ThemeScheme -> resources -> Programme)
            var themeSchemeDir = Path.GetDirectoryName(jsonPath);
            var resourcesDir = Directory.GetParent(themeSchemeDir!)?.FullName;
            
            if (resourcesDir != null)
            {
                var settingPath = Path.Combine(resourcesDir, "Programme", themeName, "Setting.txt");
                if (File.Exists(settingPath))
                {
                    return _settingParser.Parse(settingPath);
                }
            }
            
            // Return basic theme from JSON
            return new Theme
            {
                Name = themeJson.Name ?? themeName,
                Width = (int)(themeJson.Width ?? 360),
                Height = (int)(themeJson.Height ?? 960),
                Type = themeJson.Type ?? 0,
                FilePath = jsonPath
            };
        }
        catch
        {
            return null;
        }
    }
}

/// <summary>
/// Basic theme info for list display.
/// </summary>
public class ThemeInfo
{
    public string Name { get; set; } = "";
    public int Width { get; set; }
    public int Height { get; set; }
    public int Type { get; set; }
    public string FilePath { get; set; } = "";
    public string? ThumbnailData { get; set; }
    
    public string Resolution => $"{Width}x{Height}";
    public string TypeName => Type == 1 ? "Setting.txt" : "GIF";
}

/// <summary>
/// JSON structure for theme files.
/// </summary>
internal class ThemeJson
{
    public string? Name { get; set; }
    public double? Width { get; set; }
    public double? Height { get; set; }
    public int? Type { get; set; }
    public string? ThumbnailImageData { get; set; }
}
