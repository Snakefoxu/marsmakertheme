using System.Text.Json.Serialization;

namespace SnakeMarsTheme.Models;

/// <summary>
/// Represents a theme project that can be saved and loaded.
/// Contains all the information needed to restore the editor state.
/// </summary>
public class ThemeProject
{
    public string Version { get; set; } = "1.0";
    public DateTime CreatedAt { get; set; } = DateTime.Now;
    public DateTime ModifiedAt { get; set; } = DateTime.Now;
    
    // Theme metadata
    public string ThemeName { get; set; } = "NuevoTema";
    public int Width { get; set; } = 360;
    public int Height { get; set; } = 960;
    
    // Background
    public string? BackgroundPath { get; set; }
    public bool IsVideoBackground { get; set; }
    public bool BackgroundEmbedded { get; set; }
    public string? BackgroundBase64 { get; set; }
    
    // Widgets
    public List<ProjectWidget> Widgets { get; set; } = new();
}

public class ProjectWidget
{
    public string Name { get; set; } = "";
    public string DataType { get; set; } = "";
    public string Unit { get; set; } = "";
    
    [JsonConverter(typeof(JsonStringEnumConverter))]
    public WidgetKind Kind { get; set; } = WidgetKind.Text;
    
    // Position
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    
    // Text properties
    public int FontSize { get; set; } = 24;
    public string FontFamily { get; set; } = "Impact";
    public string Color { get; set; } = "#FFFFFF";
    public int Width { get; set; } = 100;
    public int Height { get; set; } = 20;
    
    // Bar properties
    public int BarWidth { get; set; } = 100;
    public int BarHeight { get; set; } = 10;
    public int MaxNum { get; set; } = 100;
    public int CornerRadius { get; set; } = 5;
    public string Fill { get; set; } = "#00FF00";
    public string BackColor { get; set; } = "#333333";
}
