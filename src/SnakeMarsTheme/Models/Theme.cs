namespace SnakeMarsTheme.Models;

/// <summary>
/// Represents a theme configuration parsed from Setting.txt or JSON.
/// </summary>
public class Theme
{
    public string Name { get; set; } = "";
    public int Width { get; set; } = 360;
    public int Height { get; set; } = 960;
    public int Type { get; set; } = 1; // 0=GIF, 1=Setting.txt
    
    public List<ThemeImage> Images { get; set; } = new();
    public List<ThemeGif> GIFs { get; set; } = new();
    public List<TextWidget> Texts { get; set; } = new();
    public List<BarWidget> Bars { get; set; } = new();
    
    public string? BackgroundImagePath { get; set; }
    public string? ThumbnailImageData { get; set; }
    public string FilePath { get; set; } = "";
}

/// <summary>
/// Represents a PNG image in a theme.
/// </summary>
public class ThemeImage
{
    public string FileName { get; set; } = "";
    public double X { get; set; }
    public double Y { get; set; }
    public int Z { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
}

/// <summary>
/// Represents an animated GIF in a theme.
/// </summary>
public class ThemeGif
{
    public string FileName { get; set; } = "";
    public double X { get; set; }
    public double Y { get; set; }
    public int Z { get; set; }
    public int? Width { get; set; }
    public int? Height { get; set; }
    public bool IsBack { get; set; }
}

/// <summary>
/// Represents a text widget (sensor data display).
/// </summary>
public class TextWidget
{
    public double X { get; set; }
    public double Y { get; set; }
    public int Z { get; set; }
    public int FontSize { get; set; } = 12;
    public string FontFamily { get; set; } = "Segoe UI";
    public string Foreground { get; set; } = "#FFFFFF";
    public string Data { get; set; } = ""; // e.g., "CPUTemp", "CpuUsage"
    public string Unit { get; set; } = ""; // e.g., "°C", "%"
    public double Opacity { get; set; } = 1.0;
    public bool IsStatic { get; set; }
    public string? Title { get; set; }
    public int? MaxWidth { get; set; }
    public int? MaxHeight { get; set; }
}

/// <summary>
/// Represents a progress bar widget (BorderLine, DefaultLine, GridLine).
/// </summary>
public class BarWidget
{
    public string Type { get; set; } = "BorderLine"; // BorderLine, DefaultLine, GridLine
    public double X { get; set; }
    public double Y { get; set; }
    public int Z { get; set; }
    public int MaxWidth { get; set; } = 100;
    public int MaxHeight { get; set; } = 10;
    public string Fill { get; set; } = "#FFFFFF";
    public string Data { get; set; } = "";
    public string? CornerRadius { get; set; }
    public int MaxNum { get; set; } = 100;
    public double Opacity { get; set; } = 1.0;
    public int BorderThickness { get; set; }
    public string? BorderFill { get; set; }
    public string? BackColor { get; set; }
    public int Margin { get; set; } = 5;
    public int MaxCount { get; set; } = 10;
    public string Orientation { get; set; } = "Horizontal";
}
