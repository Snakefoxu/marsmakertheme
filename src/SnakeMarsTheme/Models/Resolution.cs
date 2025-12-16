namespace SnakeMarsTheme.Models;

/// <summary>
/// Predefined resolutions for Mars Gaming/SOEYI screens.
/// </summary>
public class Resolution
{
    public string Name { get; set; } = "";
    public int Width { get; set; }
    public int Height { get; set; }
    
    /// <summary>
    /// Determines the orientation based on aspect ratio.
    /// </summary>
    public string Orientation
    {
        get
        {
            double aspectRatio = (double)Width / Height;
            if (aspectRatio > 1.2) return "Horizontal";
            if (aspectRatio < 0.8) return "Vertical";
            return "Square";
        }
    }
    
    public override string ToString() => $"{Name} ({Width}x{Height})";
    
    public static readonly Resolution[] Presets = new[]
    {
        // Vertical screens
        new Resolution { Name = "Chasis Vertical", Width = 360, Height = 960 },
        new Resolution { Name = "Chasis Vertical 320", Width = 320, Height = 960 },
        new Resolution { Name = "Chasis Vertical 379", Width = 379, Height = 960 },
        new Resolution { Name = "Ultra Vertical", Width = 462, Height = 1920 },
        
        // Horizontal screens  
        new Resolution { Name = "Chasis Horizontal", Width = 960, Height = 360 },
        new Resolution { Name = "Chasis Horizontal 320", Width = 960, Height = 320 },
        new Resolution { Name = "Chasis Horizontal 376", Width = 960, Height = 376 },
        new Resolution { Name = "Chasis Horizontal 480", Width = 960, Height = 480 },
        new Resolution { Name = "Ultra Horizontal", Width = 1920, Height = 462 },
        new Resolution { Name = "Ultra Horizontal 480", Width = 1920, Height = 480 },
        new Resolution { Name = "Wide Horizontal", Width = 1600, Height = 600 },
        new Resolution { Name = "Wide 1024", Width = 1024, Height = 600 },
        
        // Square & AIO
        new Resolution { Name = "Display Cuadrado", Width = 480, Height = 480 },
        new Resolution { Name = "AIO Pequeño", Width = 320, Height = 240 },
        new Resolution { Name = "AIO Vertical", Width = 240, Height = 320 },
        new Resolution { Name = "Chasis Compacto", Width = 480, Height = 272 },
        
        // Custom option
        new Resolution { Name = "Personalizado", Width = 360, Height = 960 },
    };
}
