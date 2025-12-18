using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Text;
using System.Text.Json;
using SnakeMarsTheme.Models;

namespace SnakeMarsTheme.Services;

/// <summary>
/// Service for creating and saving themes.
/// Generates: back.png, demo.png, Setting.txt, and JSON
/// </summary>
public class ThemeCreatorService
{
    private readonly string _programmePath;
    private readonly string _themeSchemePath;
    
    public ThemeCreatorService(string basePath)
    {
        _programmePath = Path.Combine(basePath, "resources", "Programme");
        _themeSchemePath = Path.Combine(basePath, "resources", "ThemeScheme");
        
        if (!Directory.Exists(_programmePath))
            Directory.CreateDirectory(_programmePath);
        if (!Directory.Exists(_themeSchemePath))
            Directory.CreateDirectory(_themeSchemePath);
    }
    
    /// <summary>
    /// Save a complete theme with all required files to the default resources/Programme folder.
    /// </summary>
    public ThemeSaveResult SaveTheme(ThemeSaveRequest request)
    {
        var programmeFolder = Path.Combine(_programmePath, request.ThemeName);
        var jsonPath = Path.Combine(_themeSchemePath, $"{request.ThemeName}.json");
        
        return SaveThemeToPath(request, programmeFolder, jsonPath);
    }

    /// <summary>
    /// Save theme to a specific folder path (used for installation/export).
    /// </summary>
    public ThemeSaveResult SaveThemeToPath(ThemeSaveRequest request, string targetFolder, string? targetJsonPath = null)
    {
        try
        {
            // 1. Create Target folder
            if (!Directory.Exists(targetFolder))
                Directory.CreateDirectory(targetFolder);
                
            // IMPORTANT: SOEYI/Mars Gaming expects assets inside a 'source' subfolder!
            // BUT it also often needs copies in the root for the menu system.
            // We will DUPLICATE assets to ensure maximum compatibility (Protocol Omega Robustness).
            var sourceFolder = Path.Combine(targetFolder, "source");
            if (!Directory.Exists(sourceFolder))
                Directory.CreateDirectory(sourceFolder);
            
            // 2. Generate back.png or frame sequence (1.png, 2.png, ...)
            if (request.FramePaths != null && request.FramePaths.Count > 0)
            {
                // Animated theme: copy frames
                for (int i = 0; i < request.FramePaths.Count; i++)
                {
                    var framePath = request.FramePaths[i];
                    if (File.Exists(framePath))
                    {
                        var fileName = $"{i + 1}.png";
                        File.Copy(framePath, Path.Combine(sourceFolder, fileName), overwrite: true);
                        File.Copy(framePath, Path.Combine(targetFolder, fileName), overwrite: true); // Copy to root too
                    }
                }
            }
            else if (request.ThemeType == 0 && !string.IsNullOrEmpty(request.BackgroundPath))
            {
                // Type 0 (DIY): Copy background media to Programme folder
                // Soportamos: GIF, PNG, JPG (DisplayImages) y MP4, AVI, WEBM, MOV (BackgroundVideoFile)
                var bgExt = Path.GetExtension(request.BackgroundPath).ToLowerInvariant();
                var supportedExts = new[] { ".gif", ".png", ".jpg", ".jpeg", ".mp4", ".avi", ".webm", ".mov" };
                if (supportedExts.Contains(bgExt) && File.Exists(request.BackgroundPath))
                {
                    var fileName = Path.GetFileName(request.BackgroundPath);
                    File.Copy(request.BackgroundPath, Path.Combine(sourceFolder, fileName), overwrite: true);
                    File.Copy(request.BackgroundPath, Path.Combine(targetFolder, fileName), overwrite: true);
                }
            }
            else if (request.ThemeType != 0) // Skip back.png generation for Type 0
            {
                // Type 1 (Static theme): generate back.png
                using (var backBitmap = CreateBackgroundBitmap(request))
                {
                    backBitmap.Save(Path.Combine(sourceFolder, "back.png"), ImageFormat.Png);
                    backBitmap.Save(Path.Combine(targetFolder, "back.png"), ImageFormat.Png); // Copy to root too
                }
            }
            
            // 3. Generate demo.png (background + widgets)
            using (var demoBitmap = CreateDemoBitmap(request))
            {
                demoBitmap.Save(Path.Combine(sourceFolder, "demo.png"), ImageFormat.Png);
                demoBitmap.Save(Path.Combine(targetFolder, "demo.png"), ImageFormat.Png); // Copy to root too
            }
            
            // 4. Generate Setting.txt (if type 1)
            // Docs say Setting.txt is usually in Root, but some themes have it in source. We put it in both.
            if (request.ThemeType == 1)
            {
                var settingContent = GenerateSettingTxt(request);
                File.WriteAllText(Path.Combine(sourceFolder, "Setting.txt"), settingContent, Encoding.UTF8);
                File.WriteAllText(Path.Combine(targetFolder, "Setting.txt"), settingContent, Encoding.UTF8);
            }
            
            // 5. Generate JSON 
            // JSON is strictly required in the installation root for the installer to pick it up, 
            // or for the ThemeScheme logic. 
            // InstallationService expects it at `Path.Combine(themeFolder, $"{themeName}.json")` (Root)
            
            string jsonFinalPath = targetJsonPath;
            if (string.IsNullOrEmpty(jsonFinalPath))
            {
                jsonFinalPath = Path.Combine(targetFolder, $"{request.ThemeName}.json");
            }

            var jsonContent = GenerateThemeJson(request);
            // Ensure directory for JSON exists
            var jsonDir = Path.GetDirectoryName(jsonFinalPath);
            if (jsonDir != null && !Directory.Exists(jsonDir)) Directory.CreateDirectory(jsonDir);
            
            File.WriteAllText(jsonFinalPath, jsonContent, Encoding.UTF8);
            
            return new ThemeSaveResult
            {
                Success = true,
                OutputFolder = targetFolder,
                JsonPath = jsonFinalPath,
                Message = "Theme generated successfully at " + targetFolder
            };
        }
        catch (Exception ex)
        {
            return new ThemeSaveResult
            {
                Success = false,
                Error = $"Error guardando tema: {ex.Message}"
            };
        }
    }
    
    private Bitmap CreateBackgroundBitmap(ThemeSaveRequest request)
    {
        var bitmap = new Bitmap(request.Width, request.Height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        graphics.Clear(Color.Black);
        
        if (!string.IsNullOrEmpty(request.BackgroundPath) && File.Exists(request.BackgroundPath))
        {
            try
            {
                using var sourceImage = Image.FromFile(request.BackgroundPath);
                graphics.DrawImage(sourceImage, 0, 0, request.Width, request.Height);
            }
            catch { /* Keep black background */ }
        }
        
        return bitmap;
    }
    
    private Bitmap CreateDemoBitmap(ThemeSaveRequest request)
    {
        var bitmap = new Bitmap(request.Width, request.Height);
        using var graphics = Graphics.FromImage(bitmap);
        graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
        graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
        graphics.TextRenderingHint = System.Drawing.Text.TextRenderingHint.AntiAlias;
        graphics.Clear(Color.Black);
        
        // Draw background
        if (!string.IsNullOrEmpty(request.BackgroundPath) && File.Exists(request.BackgroundPath))
        {
            try
            {
                using var sourceImage = Image.FromFile(request.BackgroundPath);
                graphics.DrawImage(sourceImage, 0, 0, request.Width, request.Height);
            }
            catch { /* Keep black background */ }
        }
        
        // Draw widgets with their colors and fonts
        foreach (var widget in request.Widgets)
        {
            var fontSize = Math.Max(10, widget.FontSize);
            var fontName = string.IsNullOrEmpty(widget.Font) ? "Impact" : widget.Font;
            var color = ParseColor(widget.Color);
            
            using var font = new Font(fontName, fontSize);
            using var brush = new SolidBrush(color);
            var text = string.IsNullOrEmpty(widget.Unit) ? widget.Name : $"00 {widget.Unit}";
            graphics.DrawString(text, font, brush, widget.X, widget.Y);
        }
        
        return bitmap;
    }
    
    private Color ParseColor(string hexColor)
    {
        try
        {
            if (string.IsNullOrEmpty(hexColor)) return Color.White;
            hexColor = hexColor.TrimStart('#');
            if (hexColor.Length == 6)
                return Color.FromArgb(255, 
                    Convert.ToInt32(hexColor.Substring(0, 2), 16),
                    Convert.ToInt32(hexColor.Substring(2, 2), 16),
                    Convert.ToInt32(hexColor.Substring(4, 2), 16));
        }
        catch { }
        return Color.White;
    }
    
    private string GenerateSettingTxt(ThemeSaveRequest request)
    {
        var sb = new StringBuilder();
        sb.AppendLine($"name:{request.ThemeName}");
        sb.AppendLine($"width:{request.Width}");
        sb.AppendLine($"height:{request.Height}");
        sb.AppendLine("back.png:x@0,y@0,z@0");
        
        int zIndex = 1;
        foreach (var widget in request.Widgets)
        {
            var color = string.IsNullOrEmpty(widget.Color) ? "FFFFFF" : widget.Color.TrimStart('#');
            var fill = string.IsNullOrEmpty(widget.Fill) ? "00FF00" : widget.Fill.TrimStart('#');
            
            switch (widget.WidgetType)
            {
                case WidgetType.Text:
                    var unit = string.IsNullOrEmpty(widget.Unit) ? "" : widget.Unit;
                    var font = string.IsNullOrEmpty(widget.Font) ? "Impact" : widget.Font;
                    sb.AppendLine($"Text:x@{widget.X},y@{widget.Y},z@{zIndex},FontSize@{widget.FontSize},FontFamily@#{font},Foreground@#{color},data@{widget.Type},unit@{unit}");
                    break;
                    
                case WidgetType.BorderLine:
                    sb.AppendLine($"BorderLine:x@{widget.X},y@{widget.Y},z@{zIndex},maxheight@{widget.BarHeight},maxwidth@{widget.BarWidth},Fill@#{fill},CornerRadius@{widget.CornerRadius},data@{widget.Type},MaxNum@{widget.MaxNum}");
                    break;
                    
                case WidgetType.DefaultLine:
                    var backColor = string.IsNullOrEmpty(widget.BackColor) ? "333333" : widget.BackColor.TrimStart('#');
                    sb.AppendLine($"DefaultLine:x@{widget.X},y@{widget.Y},z@{zIndex},maxheight@{widget.BarHeight},maxwidth@{widget.BarWidth},Fill@#{backColor}");
                    break;
                    
                case WidgetType.GridLine:
                    sb.AppendLine($"GridLine:x@{widget.X},y@{widget.Y},z@{zIndex},maxheight@{widget.BarHeight},maxwidth@{widget.BarWidth},Margin@{widget.Margin},maxcount@{widget.MaxCount},Fill@#{fill},Orientation@{widget.Orientation},data@{widget.Type}");
                    break;
            }
            zIndex++;
        }
        
        return sb.ToString().TrimEnd();
    }
    
    private string GenerateThemeJson(ThemeSaveRequest request)
    {
        // For Type 0 (DIY), we must map widgets to DisplayTexts
        // For Type 1 (Setting.txt), DisplayTexts is empty
        
        var displayTexts = new List<object>();
        
        if (request.ThemeType == 0) // DIY Style
        {
            int zIndex = 0;
            foreach (var w in request.Widgets)
            {
                if (w.WidgetType == WidgetType.Text)
                {
                    // Use official Mars Gaming TextType from widget.Type
                    // This comes directly from the 32 validated widgets
                    string textType = w.Type ?? "CPUTemp"; // Default fallback
                    
                    displayTexts.Add(new
                    {
                        Id = Guid.NewGuid().ToString(),
                        Left = (double)w.X,
                        Top = (double)w.Y,
                        ZIndex = zIndex++,
                        TextType = textType,
                        Text = string.IsNullOrEmpty(w.Unit) ? w.Name : $"00{w.Unit}",
                        Color = "255, 255, 255", // Mars Gaming requires RGB format
                        FontName = string.IsNullOrEmpty(w.Font) ? "Segoe UI" : w.Font,
                        FontSize = (double)w.FontSize,
                        Bold = true,
                        Italic = false,  // Required by Mars Gaming
                        Underline = false,  // Required by Mars Gaming
                        TitleVisibility = false  // Required by Mars Gaming
                    });
                }
                // Note: Mars Gaming JSON mostly supports Text widgets effectively in DIY mode
                // Bars might not be supported in Type 0 the same way, usually Type 0 implies simple text widgets over GIF
            }
        }

        // For Type 0 (DIY), handle background based on file type
        var displayImages = new List<object>();
        string? backgroundVideoFile = null;
        
        if (request.ThemeType == 0 && !string.IsNullOrEmpty(request.BackgroundPath))
        {
            var ext = Path.GetExtension(request.BackgroundPath).ToLowerInvariant();
            string fileName = Path.GetFileName(request.BackgroundPath);
            string placeholderPath = $"{{PROGRAMME_PATH}}\\{request.ThemeName}\\{fileName}";
            
            if (ext == ".mp4" || ext == ".avi" || ext == ".webm" || ext == ".mov")
            {
                // MP4/Video: usar BackgroundVideoFile (NO DisplayImages)
                // Mars Gaming reproduce videos directamente desde este campo
                backgroundVideoFile = placeholderPath;
            }
            else if (ext == ".gif" || ext == ".png" || ext == ".jpg" || ext == ".jpeg")
            {
                // GIF/PNG/JPG: usar DisplayImages
                displayImages.Add(new
                {
                    Left = 0.0,
                    Top = 0.0,
                    ZIndex = 0,
                    ImageFileName = (string?)null,
                    Image = placeholderPath,
                    Width = (double)request.Width,
                    Height = (double)request.Height
                });
            }
        }

        var theme = new
        {
            Type = request.ThemeType,
            Name = request.ThemeName,
            // CRITICAL: Mars Gaming Type 0 always uses Width:320 Height:240
            // regardless of actual screen resolution
            Width = request.ThemeType == 0 ? 320.0 : (double)request.Width,
            Height = request.ThemeType == 0 ? 240.0 : (double)request.Height,
            FillMode = 0,
            CropArea = "0, 0, 0, 0",
            ThumbnailImageData = (string?)null,
            DisplayDirection = 0,
            FontDirection = 0,
            BackgroundImage = (string?)null,
            BackgroundVideoFile = backgroundVideoFile,
            DisplayTexts = displayTexts,
            DisplayImages = displayImages
        };
        
        return JsonSerializer.Serialize(theme, new JsonSerializerOptions 
        { 
            WriteIndented = true,
            PropertyNamingPolicy = null
        });
    }
}

public class ThemeSaveRequest
{
    public string ThemeName { get; set; } = "";
    public int Width { get; set; } = 360;
    public int Height { get; set; } = 960;
    public int ThemeType { get; set; } = 1; // 0=GIF, 1=Setting.txt
    public string BackgroundPath { get; set; } = "";
    public List<string> FramePaths { get; set; } = new(); // For animated themes  
    public List<WidgetInfo> Widgets { get; set; } = new();
}

public class WidgetInfo
{
    public string Name { get; set; } = "";
    public string Type { get; set; } = "";
    public string Unit { get; set; } = "";
    public int X { get; set; }
    public int Y { get; set; }
    public int FontSize { get; set; } = 24;
    public string Font { get; set; } = "Impact";
    public string Color { get; set; } = "#FFFFFF";
    
    // Widget type
    public WidgetType WidgetType { get; set; } = WidgetType.Text;
    
    // Bar properties
    public int BarWidth { get; set; } = 100;
    public int BarHeight { get; set; } = 10;
    public int MaxNum { get; set; } = 100;
    public int CornerRadius { get; set; } = 5;
    public string Fill { get; set; } = "#00FF00";
    public string BackColor { get; set; } = "#333333";
    
    // GridLine specific
    public int Margin { get; set; } = 5;
    public int MaxCount { get; set; } = 10;
    public string Orientation { get; set; } = "Horizontal";
}

public enum WidgetType
{
    Text,
    BorderLine,
    DefaultLine,
    GridLine
}

public class ThemeSaveResult
{
    public bool Success { get; set; }
    public string? OutputFolder { get; set; }
    public string? JsonPath { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
}
