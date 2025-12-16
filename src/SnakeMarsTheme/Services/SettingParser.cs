using System.IO;
using System.Text.RegularExpressions;
using SnakeMarsTheme.Models;

namespace SnakeMarsTheme.Services;

/// <summary>
/// Parses Setting.txt files from SOEYI/Mars Gaming themes.
/// Ported from PowerShell SettingParser.ps1
/// </summary>
public class SettingParser
{
    /// <summary>
    /// Parse a Setting.txt file and return a Theme object.
    /// </summary>
    public Theme Parse(string path)
    {
        if (!File.Exists(path))
            throw new FileNotFoundException($"Setting.txt not found: {path}");
        
        var lines = File.ReadAllLines(path)
            .Where(l => !string.IsNullOrWhiteSpace(l))
            .Select(l => l.Trim())
            .ToList();
        
        var theme = new Theme
        {
            FilePath = path
        };
        
        foreach (var line in lines)
        {
            // Header: name:
            var nameMatch = Regex.Match(line, @"^name:(.+)$");
            if (nameMatch.Success)
            {
                theme.Name = nameMatch.Groups[1].Value.Trim();
                continue;
            }
            
            // Header: width:
            var widthMatch = Regex.Match(line, @"^width:(\d+)");
            if (widthMatch.Success)
            {
                theme.Width = int.Parse(widthMatch.Groups[1].Value);
                continue;
            }
            
            // Header: height:
            var heightMatch = Regex.Match(line, @"^height:(\d+)");
            if (heightMatch.Success)
            {
                theme.Height = int.Parse(heightMatch.Groups[1].Value);
                continue;
            }
            
            // PNG Image
            var pngMatch = Regex.Match(line, @"^(.+\.png):(.+)$", RegexOptions.IgnoreCase);
            if (pngMatch.Success)
            {
                var parms = ParseParameters(pngMatch.Groups[2].Value);
                theme.Images.Add(new ThemeImage
                {
                    FileName = pngMatch.Groups[1].Value,
                    X = GetDouble(parms, "x"),
                    Y = GetDouble(parms, "y"),
                    Z = GetInt(parms, "z"),
                    Width = GetNullableInt(parms, "width"),
                    Height = GetNullableInt(parms, "height")
                });
                continue;
            }
            
            // GIF Image
            var gifMatch = Regex.Match(line, @"^(.+\.gif):(.+)$", RegexOptions.IgnoreCase);
            if (gifMatch.Success)
            {
                var parms = ParseParameters(gifMatch.Groups[2].Value);
                theme.GIFs.Add(new ThemeGif
                {
                    FileName = gifMatch.Groups[1].Value,
                    X = GetDouble(parms, "x"),
                    Y = GetDouble(parms, "y"),
                    Z = GetInt(parms, "z"),
                    Width = GetNullableInt(parms, "width"),
                    Height = GetNullableInt(parms, "height"),
                    IsBack = GetBool(parms, "IsBack")
                });
                continue;
            }
            
            // Text widget
            var textMatch = Regex.Match(line, @"^Text:(.+)$");
            if (textMatch.Success)
            {
                var parms = ParseParameters(textMatch.Groups[1].Value);
                theme.Texts.Add(new TextWidget
                {
                    X = GetDouble(parms, "x"),
                    Y = GetDouble(parms, "y"),
                    Z = GetInt(parms, "z"),
                    FontSize = GetInt(parms, "FontSize", 12),
                    FontFamily = GetString(parms, "FontFamily", "Segoe UI").TrimStart('#'),
                    Foreground = ParseColor(GetString(parms, "Foreground", "#FFFFFF")),
                    Data = GetString(parms, "data"),
                    Unit = GetString(parms, "unit"),
                    Opacity = GetDouble(parms, "Opacity", 1.0),
                    IsStatic = GetBool(parms, "IsDefaultText"),
                    Title = GetString(parms, "Title"),
                    MaxWidth = GetNullableInt(parms, "maxwidth"),
                    MaxHeight = GetNullableInt(parms, "maxheight")
                });
                continue;
            }
            
            // BorderLine (dynamic bar)
            var borderMatch = Regex.Match(line, @"^BorderLine:(.+)$");
            if (borderMatch.Success)
            {
                var parms = ParseParameters(borderMatch.Groups[1].Value);
                theme.Bars.Add(new BarWidget
                {
                    Type = "BorderLine",
                    X = GetDouble(parms, "x"),
                    Y = GetDouble(parms, "y"),
                    Z = GetInt(parms, "z"),
                    MaxWidth = GetInt(parms, "maxwidth", 100),
                    MaxHeight = GetInt(parms, "maxheight", 10),
                    Fill = ParseColor(GetString(parms, "Fill", "#FFFFFF")),
                    Data = GetString(parms, "data"),
                    CornerRadius = GetString(parms, "CornerRadius"),
                    MaxNum = GetInt(parms, "MaxNum", 100),
                    Opacity = GetDouble(parms, "Opacity", 1.0),
                    BorderThickness = GetInt(parms, "BorderThicknes"),
                    BorderFill = parms.ContainsKey("BorderFill") ? ParseColor(parms["BorderFill"]) : null,
                    BackColor = parms.ContainsKey("BackColor") ? ParseColor(parms["BackColor"]) : null
                });
                continue;
            }
            
            // DefaultLine (static bar)
            var defaultMatch = Regex.Match(line, @"^DefaultLine:(.+)$");
            if (defaultMatch.Success)
            {
                var parms = ParseParameters(defaultMatch.Groups[1].Value);
                theme.Bars.Add(new BarWidget
                {
                    Type = "DefaultLine",
                    X = GetDouble(parms, "x"),
                    Y = GetDouble(parms, "y"),
                    Z = GetInt(parms, "z"),
                    MaxWidth = GetInt(parms, "maxwidth", 100),
                    MaxHeight = GetInt(parms, "maxheight", 10),
                    Fill = ParseColor(GetString(parms, "Fill", "#FFFFFF")),
                    Data = GetString(parms, "data")
                });
                continue;
            }
            
            // GridLine (segmented bar)
            var gridMatch = Regex.Match(line, @"^GridLine:(.+)$");
            if (gridMatch.Success)
            {
                var parms = ParseParameters(gridMatch.Groups[1].Value);
                theme.Bars.Add(new BarWidget
                {
                    Type = "GridLine",
                    X = GetDouble(parms, "x"),
                    Y = GetDouble(parms, "y"),
                    Z = GetInt(parms, "z"),
                    MaxWidth = GetInt(parms, "maxwidth", 20),
                    MaxHeight = GetInt(parms, "maxheight", 10),
                    Fill = ParseColor(GetString(parms, "Fill", "#FFFFFF")),
                    Data = GetString(parms, "data"),
                    Margin = GetInt(parms, "Margin", 5),
                    MaxCount = GetInt(parms, "maxcount", 10),
                    Orientation = GetString(parms, "Orientation", "Horizontal")
                });
                continue;
            }
        }
        
        return theme;
    }
    
    /// <summary>
    /// Parse key@value parameters from a Setting.txt line.
    /// </summary>
    private Dictionary<string, string> ParseParameters(string paramString)
    {
        var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        var matches = Regex.Matches(paramString, @"(\w+)@([^,]+)");
        
        foreach (Match match in matches)
        {
            var key = match.Groups[1].Value;
            var value = match.Groups[2].Value.Trim();
            result[key] = value;
        }
        
        return result;
    }
    
    /// <summary>
    /// Normalize color strings to #RRGGBB or #AARRGGBB format.
    /// </summary>
    private string ParseColor(string colorString)
    {
        colorString = colorString.Trim();
        if (!colorString.StartsWith("#"))
            colorString = "#" + colorString;
        return colorString.ToUpper();
    }
    
    // Helper methods
    private double GetDouble(Dictionary<string, string> parms, string key, double defaultValue = 0)
        => parms.TryGetValue(key, out var val) && double.TryParse(val, out var d) ? d : defaultValue;
    
    private int GetInt(Dictionary<string, string> parms, string key, int defaultValue = 0)
        => parms.TryGetValue(key, out var val) && int.TryParse(val, out var i) ? i : defaultValue;
    
    private int? GetNullableInt(Dictionary<string, string> parms, string key)
        => parms.TryGetValue(key, out var val) && int.TryParse(val, out var i) ? i : null;
    
    private string GetString(Dictionary<string, string> parms, string key, string defaultValue = "")
        => parms.TryGetValue(key, out var val) ? val : defaultValue;
    
    private bool GetBool(Dictionary<string, string> parms, string key)
        => parms.TryGetValue(key, out var val) && val.Equals("true", StringComparison.OrdinalIgnoreCase);
}
