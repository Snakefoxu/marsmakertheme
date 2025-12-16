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
    /// Save a complete theme with all required files.
    /// </summary>
    public ThemeSaveResult SaveTheme(ThemeSaveRequest request)
    {
        try
        {
            // 1. Create Programme folder
            var programmeFolder = Path.Combine(_programmePath, request.ThemeName);
            if (!Directory.Exists(programmeFolder))
                Directory.CreateDirectory(programmeFolder);
            
            // 2. Generate back.png or frame sequence (1.png, 2.png, ...)
            if (request.FramePaths != null && request.FramePaths.Count > 0)
            {
                // Animated theme: copy frames
                for (int i = 0; i < request.FramePaths.Count; i++)
                {
                    var framePath = request.FramePaths[i];
                    if (File.Exists(framePath))
                    {
                        var destPath = Path.Combine(programmeFolder, $"{i + 1}.png");
                        File.Copy(framePath, destPath, overwrite: true);
                    }
                }
            }
            else
            {
                // Static theme: generate back.png
                var backPath = Path.Combine(programmeFolder, "back.png");
                using (var backBitmap = CreateBackgroundBitmap(request))
                {
                    backBitmap.Save(backPath, ImageFormat.Png);
                }
            }
            
            // 3. Generate demo.png (background + widgets)
            var demoPath = Path.Combine(programmeFolder, "demo.png");
            using (var demoBitmap = CreateDemoBitmap(request))
            {
                demoBitmap.Save(demoPath, ImageFormat.Png);
            }
            
            // 4. Generate Setting.txt (if type 1)
            if (request.ThemeType == 1)
            {
                var settingPath = Path.Combine(programmeFolder, "Setting.txt");
                var settingContent = GenerateSettingTxt(request);
                File.WriteAllText(settingPath, settingContent, Encoding.UTF8);
            }
            
            // 5. Generate JSON in ThemeScheme
            var jsonPath = Path.Combine(_themeSchemePath, $"{request.ThemeName}.json");
            var jsonContent = GenerateThemeJson(request);
            File.WriteAllText(jsonPath, jsonContent, Encoding.UTF8);
            
            int frameCount = request.FramePaths?.Count ?? 0;
            string filesGenerated = frameCount > 0
                ? $"• Programme/{request.ThemeName}/ ({frameCount} frames: 1.png ... {frameCount}.png)\n"
                : $"• Programme/{request.ThemeName}/back.png\n";
            
            return new ThemeSaveResult
            {
                Success = true,
                OutputFolder = programmeFolder,
                JsonPath = jsonPath,
                Message = $"Tema '{request.ThemeName}' creado exitosamente!\n\n" +
                          $"Archivos generados:\n" +
                          filesGenerated +
                          $"• Programme/{request.ThemeName}/demo.png\n" +
                          (request.ThemeType == 1 ? $"• Programme/{request.ThemeName}/Setting.txt\n" : "") +
                          $"• ThemeScheme/{request.ThemeName}.json"
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
                    // Map generic widget type to Mars TextType
                    // Example: "data@CPUTemp" -> "CPUTemp"
                    string textType = w.Type ?? "Static";
                    
                    displayTexts.Add(new
                    {
                        Id = Guid.NewGuid().ToString(),
                        Left = (double)w.X,
                        Top = (double)w.Y,
                        ZIndex = zIndex++,
                        TextType = textType, // Specific key for Mars app
                        Text = string.IsNullOrEmpty(w.Unit) ? w.Name : $"00{w.Unit}",
                        Color = w.Color, // #RRGGBB
                        FontName = string.IsNullOrEmpty(w.Font) ? "Microsoft YaHei" : w.Font,
                        FontSize = (double)w.FontSize,
                        Bold = true,
                        Align = 0,
                        Data = w.Type
                    });
                }
                // Note: Mars Gaming JSON mostly supports Text widgets effectively in DIY mode
                // Bars might not be supported in Type 0 the same way, usually Type 0 implies simple text widgets over GIF
            }
        }

        var theme = new
        {
            Type = request.ThemeType,
            Name = request.ThemeName,
            Width = (double)request.Width,
            Height = (double)request.Height,
            FillMode = 0,
            CropArea = "0, 0, 0, 0",
            ThumbnailImageData = (string?)null,
            DisplayDirection = 0,
            FontDirection = 0,
            BackgroundImage = (string?)null,
            BackgroundVideoFile = (string?)null,
            DisplayTexts = displayTexts,
            DisplayImages = Array.Empty<object>()
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
