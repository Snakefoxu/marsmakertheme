namespace SnakeMarsTheme.Models;

/// <summary>
/// Represents a predefined theme template with common widget configurations
/// </summary>
public class ThemeTemplate
{
    public string Name { get; set; } = "";
    public string Description { get; set; } = "";
    public string Category { get; set; } = "";
    public int Width { get; set; }
    public int Height { get; set; }
    public List<TemplateWidget> Widgets { get; set; } = new();
    
    public string DisplayName => $"{Name} ({Width}x{Height})";
}

public class TemplateWidget
{
    public string Name { get; set; } = "";
    public string DataType { get; set; } = "";
    public string Unit { get; set; } = "";
    public WidgetKind Kind { get; set; } = WidgetKind.Text;
    public int X { get; set; }
    public int Y { get; set; }
    public int FontSize { get; set; } = 24;
    public string FontFamily { get; set; } = "Impact";
    public string Color { get; set; } = "#00FFFF";
    public int Width { get; set; } = 100;
    public int Height { get; set; } = 20;
}

/// <summary>
/// Factory for creating predefined theme templates
/// </summary>
public static class ThemeTemplateFactory
{
    public static List<ThemeTemplate> GetAllTemplates()
    {
        return new List<ThemeTemplate>
        {
            // Vertical Templates (360x960)
            CreateMinimalVertical(),
            CreateGamerVertical(),
            CreateSystemMonitorVertical(),
            
            // Horizontal Templates (960x360)
            CreateMinimalHorizontal(),
            CreateGamerHorizontal(),
            
            // Square/AIO Templates (480x480)
            CreateMinimalSquare(),
            CreateFullSquare(),
            
            // Small Display (320x240)
            CreateCompactDisplay(),
        };
    }
    
    public static ThemeTemplate CreateMinimalVertical()
    {
        return new ThemeTemplate
        {
            Name = "Minimal",
            Description = "Diseño minimalista con información esencial",
            Category = "Vertical",
            Width = 360,
            Height = 960,
            Widgets = new List<TemplateWidget>
            {
                // CPU Section
                new() { Name = "CPU", DataType = "Static", X = 20, Y = 50, FontSize = 18, Color = "#00FFFF" },
                new() { Name = "Temperatura CPU", DataType = "CPUTemp", Unit = "°C", X = 20, Y = 80, FontSize = 32, Color = "#FFFFFF" },
                new() { Name = "Uso CPU", DataType = "CPUUsage", Unit = "%", X = 200, Y = 80, FontSize = 32, Color = "#00FF00" },
                
                // GPU Section
                new() { Name = "GPU", DataType = "Static", X = 20, Y = 150, FontSize = 18, Color = "#FF8C00" },
                new() { Name = "Temperatura GPU", DataType = "GPUTemp", Unit = "°C", X = 20, Y = 180, FontSize = 32, Color = "#FFFFFF" },
                new() { Name = "Uso GPU", DataType = "GPUUsage", Unit = "%", X = 200, Y = 180, FontSize = 32, Color = "#00FF00" },
                
                // RAM Section
                new() { Name = "RAM", DataType = "Static", X = 20, Y = 250, FontSize = 18, Color = "#9400D3" },
                new() { Name = "Uso Memoria", DataType = "MemoryUsage", Unit = "%", X = 20, Y = 280, FontSize = 32, Color = "#FFFFFF" },
                
                // Time
                new() { Name = "Hora", DataType = "CurrentTime", X = 20, Y = 400, FontSize = 48, Color = "#00FFFF" },
                new() { Name = "Fecha", DataType = "CurrentDate", X = 20, Y = 460, FontSize = 20, Color = "#CCCCCC" },
            }
        };
    }
    
    public static ThemeTemplate CreateGamerVertical()
    {
        return new ThemeTemplate
        {
            Name = "Gamer RGB",
            Description = "Diseño gaming con barras de progreso",
            Category = "Vertical",
            Width = 360,
            Height = 960,
            Widgets = new List<TemplateWidget>
            {
                // Header
                new() { Name = "SYSTEM MONITOR", DataType = "Static", X = 20, Y = 30, FontSize = 22, Color = "#00FFFF", FontFamily = "Segoe UI Black" },
                
                // CPU with Bar
                new() { Name = "CPU", DataType = "Static", X = 20, Y = 80, FontSize = 14, Color = "#00FFFF" },
                new() { Name = "CPU Bar", DataType = "CpuUsage", Kind = WidgetKind.BorderLine, X = 20, Y = 100, Width = 320, Height = 15, Color = "#00FFFF" },
                new() { Name = "Temperatura CPU", DataType = "CPUTemp", Unit = "°C", X = 20, Y = 120, FontSize = 24, Color = "#FFFFFF" },
                new() { Name = "Uso CPU", DataType = "CPUUsage", Unit = "%", X = 240, Y = 120, FontSize = 24, Color = "#00FF00" },
                
                // GPU with Bar
                new() { Name = "GPU", DataType = "Static", X = 20, Y = 170, FontSize = 14, Color = "#FF8C00" },
                new() { Name = "GPU Bar", DataType = "GpuUsage", Kind = WidgetKind.BorderLine, X = 20, Y = 190, Width = 320, Height = 15, Color = "#FF8C00" },
                new() { Name = "Temperatura GPU", DataType = "GPUTemp", Unit = "°C", X = 20, Y = 210, FontSize = 24, Color = "#FFFFFF" },
                new() { Name = "Uso GPU", DataType = "GPUUsage", Unit = "%", X = 240, Y = 210, FontSize = 24, Color = "#00FF00" },
                
                // RAM with Bar
                new() { Name = "RAM", DataType = "Static", X = 20, Y = 260, FontSize = 14, Color = "#9400D3" },
                new() { Name = "RAM Bar", DataType = "MemoryUsage", Kind = WidgetKind.BorderLine, X = 20, Y = 280, Width = 320, Height = 15, Color = "#9400D3" },
                new() { Name = "RAM Usada", DataType = "MemoryUsedGB", Unit = "GB", X = 20, Y = 300, FontSize = 24, Color = "#FFFFFF" },
                new() { Name = "Uso RAM", DataType = "MemoryUsage", Unit = "%", X = 240, Y = 300, FontSize = 24, Color = "#00FF00" },
                
                // Fans
                new() { Name = "FANS", DataType = "Static", X = 20, Y = 360, FontSize = 14, Color = "#32CD32" },
                new() { Name = "Fan CPU", DataType = "CPUFanSpeed", Unit = "RPM", X = 20, Y = 385, FontSize = 20, Color = "#FFFFFF" },
                new() { Name = "Fan GPU", DataType = "GPUFanSpeed", Unit = "RPM", X = 180, Y = 385, FontSize = 20, Color = "#FFFFFF" },
                
                // Time at bottom
                new() { Name = "Hora", DataType = "CurrentTime", X = 80, Y = 500, FontSize = 60, Color = "#00FFFF", FontFamily = "Segoe UI Black" },
                new() { Name = "Fecha", DataType = "CurrentDate", X = 100, Y = 570, FontSize = 18, Color = "#666666" },
            }
        };
    }
    
    public static ThemeTemplate CreateSystemMonitorVertical()
    {
        return new ThemeTemplate
        {
            Name = "System Monitor Pro",
            Description = "Monitor completo con todos los sensores",
            Category = "Vertical",
            Width = 360,
            Height = 960,
            Widgets = new List<TemplateWidget>
            {
                // CPU Section
                new() { Name = "═══ CPU ═══", DataType = "Static", X = 90, Y = 30, FontSize = 16, Color = "#00FFFF" },
                new() { Name = "Temp", DataType = "CPUTemp", Unit = "°C", X = 30, Y = 60, FontSize = 28, Color = "#FFFFFF" },
                new() { Name = "Uso", DataType = "CPUUsage", Unit = "%", X = 150, Y = 60, FontSize = 28, Color = "#00FF00" },
                new() { Name = "Clock", DataType = "CPUClock", Unit = "MHz", X = 250, Y = 60, FontSize = 28, Color = "#FFA500" },
                new() { Name = "Power", DataType = "CPUPower", Unit = "W", X = 30, Y = 95, FontSize = 20, Color = "#FF4444" },
                new() { Name = "Voltage", DataType = "CPUVoltage", Unit = "V", X = 150, Y = 95, FontSize = 20, Color = "#CCCCCC" },
                
                // GPU Section
                new() { Name = "═══ GPU ═══", DataType = "Static", X = 90, Y = 140, FontSize = 16, Color = "#FF8C00" },
                new() { Name = "Temp", DataType = "GPUTemp", Unit = "°C", X = 30, Y = 170, FontSize = 28, Color = "#FFFFFF" },
                new() { Name = "Uso", DataType = "GPUUsage", Unit = "%", X = 150, Y = 170, FontSize = 28, Color = "#00FF00" },
                new() { Name = "Clock", DataType = "GPUClock", Unit = "MHz", X = 250, Y = 170, FontSize = 28, Color = "#FFA500" },
                new() { Name = "Power", DataType = "GPUPower", Unit = "W", X = 30, Y = 205, FontSize = 20, Color = "#FF4444" },
                new() { Name = "VRAM", DataType = "GPUMemUsed", Unit = "MB", X = 150, Y = 205, FontSize = 20, Color = "#9400D3" },
                
                // RAM Section
                new() { Name = "═══ RAM ═══", DataType = "Static", X = 90, Y = 250, FontSize = 16, Color = "#9400D3" },
                new() { Name = "Used", DataType = "MemoryUsedGB", Unit = "GB", X = 30, Y = 280, FontSize = 28, Color = "#FFFFFF" },
                new() { Name = "Uso", DataType = "MemoryUsage", Unit = "%", X = 150, Y = 280, FontSize = 28, Color = "#00FF00" },
                new() { Name = "Clock", DataType = "MemoryClock", Unit = "MHz", X = 250, Y = 280, FontSize = 28, Color = "#FFA500" },
                
                // Network Section
                new() { Name = "═══ NET ═══", DataType = "Static", X = 90, Y = 330, FontSize = 16, Color = "#1E90FF" },
                new() { Name = "↓", DataType = "NetworkDown", Unit = "KB/s", X = 30, Y = 360, FontSize = 24, Color = "#00FF00" },
                new() { Name = "↑", DataType = "NetworkUp", Unit = "KB/s", X = 180, Y = 360, FontSize = 24, Color = "#FF4444" },
                
                // Disk Section
                new() { Name = "═══ DISK ═══", DataType = "Static", X = 85, Y = 400, FontSize = 16, Color = "#32CD32" },
                new() { Name = "Temp", DataType = "DiskTemp", Unit = "°C", X = 30, Y = 430, FontSize = 24, Color = "#FFFFFF" },
                new() { Name = "Free", DataType = "DiskFree", Unit = "GB", X = 180, Y = 430, FontSize = 24, Color = "#00FF00" },
                
                // Fans
                new() { Name = "═══ FANS ═══", DataType = "Static", X = 85, Y = 480, FontSize = 16, Color = "#00CED1" },
                new() { Name = "CPU Fan", DataType = "CPUFanSpeed", Unit = "RPM", X = 30, Y = 510, FontSize = 22, Color = "#FFFFFF" },
                new() { Name = "GPU Fan", DataType = "GPUFanSpeed", Unit = "RPM", X = 180, Y = 510, FontSize = 22, Color = "#FFFFFF" },
                
                // Time
                new() { Name = "Hora", DataType = "CurrentTime", X = 60, Y = 600, FontSize = 64, Color = "#00FFFF", FontFamily = "Segoe UI Black" },
                new() { Name = "Fecha", DataType = "CurrentDate", X = 80, Y = 680, FontSize = 22, Color = "#666666" },
                new() { Name = "Día", DataType = "CurrentWeek", X = 130, Y = 720, FontSize = 18, Color = "#444444" },
            }
        };
    }
    
    public static ThemeTemplate CreateMinimalHorizontal()
    {
        return new ThemeTemplate
        {
            Name = "Minimal Horizontal",
            Description = "Diseño horizontal minimalista",
            Category = "Horizontal",
            Width = 960,
            Height = 360,
            Widgets = new List<TemplateWidget>
            {
                // Left: Time
                new() { Name = "Hora", DataType = "CurrentTime", X = 30, Y = 60, FontSize = 72, Color = "#00FFFF", FontFamily = "Segoe UI Black" },
                new() { Name = "Fecha", DataType = "CurrentDate", X = 30, Y = 150, FontSize = 24, Color = "#666666" },
                
                // Center: CPU/GPU
                new() { Name = "CPU", DataType = "Static", X = 380, Y = 50, FontSize = 16, Color = "#00FFFF" },
                new() { Name = "CPU Temp", DataType = "CPUTemp", Unit = "°C", X = 380, Y = 80, FontSize = 36, Color = "#FFFFFF" },
                new() { Name = "CPU Usage", DataType = "CPUUsage", Unit = "%", X = 380, Y = 130, FontSize = 28, Color = "#00FF00" },
                
                new() { Name = "GPU", DataType = "Static", X = 550, Y = 50, FontSize = 16, Color = "#FF8C00" },
                new() { Name = "GPU Temp", DataType = "GPUTemp", Unit = "°C", X = 550, Y = 80, FontSize = 36, Color = "#FFFFFF" },
                new() { Name = "GPU Usage", DataType = "GPUUsage", Unit = "%", X = 550, Y = 130, FontSize = 28, Color = "#00FF00" },
                
                // Right: RAM/Fans
                new() { Name = "RAM", DataType = "Static", X = 720, Y = 50, FontSize = 16, Color = "#9400D3" },
                new() { Name = "RAM Usage", DataType = "MemoryUsage", Unit = "%", X = 720, Y = 80, FontSize = 36, Color = "#FFFFFF" },
                
                new() { Name = "FANS", DataType = "Static", X = 850, Y = 50, FontSize = 16, Color = "#32CD32" },
                new() { Name = "CPU Fan", DataType = "CPUFanSpeed", Unit = "RPM", X = 850, Y = 80, FontSize = 24, Color = "#FFFFFF" },
            }
        };
    }
    
    public static ThemeTemplate CreateGamerHorizontal()
    {
        return new ThemeTemplate
        {
            Name = "Gamer Horizontal",
            Description = "Diseño gaming horizontal con barras",
            Category = "Horizontal",
            Width = 960,
            Height = 360,
            Widgets = new List<TemplateWidget>
            {
                // Title
                new() { Name = "SYSTEM STATUS", DataType = "Static", X = 350, Y = 20, FontSize = 28, Color = "#00FFFF", FontFamily = "Segoe UI Black" },
                
                // CPU Column
                new() { Name = "CPU", DataType = "Static", X = 50, Y = 70, FontSize = 18, Color = "#00FFFF" },
                new() { Name = "CPU Bar", DataType = "CpuUsage", Kind = WidgetKind.BorderLine, X = 50, Y = 95, Width = 200, Height = 20, Color = "#00FFFF" },
                new() { Name = "CPU Temp", DataType = "CPUTemp", Unit = "°C", X = 50, Y = 125, FontSize = 32, Color = "#FFFFFF" },
                new() { Name = "CPU Usage", DataType = "CPUUsage", Unit = "%", X = 150, Y = 125, FontSize = 32, Color = "#00FF00" },
                new() { Name = "CPU Clock", DataType = "CPUClock", Unit = "MHz", X = 50, Y = 165, FontSize = 20, Color = "#FFA500" },
                
                // GPU Column
                new() { Name = "GPU", DataType = "Static", X = 300, Y = 70, FontSize = 18, Color = "#FF8C00" },
                new() { Name = "GPU Bar", DataType = "GpuUsage", Kind = WidgetKind.BorderLine, X = 300, Y = 95, Width = 200, Height = 20, Color = "#FF8C00" },
                new() { Name = "GPU Temp", DataType = "GPUTemp", Unit = "°C", X = 300, Y = 125, FontSize = 32, Color = "#FFFFFF" },
                new() { Name = "GPU Usage", DataType = "GPUUsage", Unit = "%", X = 400, Y = 125, FontSize = 32, Color = "#00FF00" },
                new() { Name = "GPU Clock", DataType = "GPUClock", Unit = "MHz", X = 300, Y = 165, FontSize = 20, Color = "#FFA500" },
                
                // RAM Column
                new() { Name = "RAM", DataType = "Static", X = 550, Y = 70, FontSize = 18, Color = "#9400D3" },
                new() { Name = "RAM Bar", DataType = "MemoryUsage", Kind = WidgetKind.BorderLine, X = 550, Y = 95, Width = 200, Height = 20, Color = "#9400D3" },
                new() { Name = "RAM Used", DataType = "MemoryUsedGB", Unit = "GB", X = 550, Y = 125, FontSize = 32, Color = "#FFFFFF" },
                new() { Name = "RAM Usage", DataType = "MemoryUsage", Unit = "%", X = 650, Y = 125, FontSize = 32, Color = "#00FF00" },
                
                // Time Column
                new() { Name = "Hora", DataType = "CurrentTime", X = 800, Y = 80, FontSize = 48, Color = "#00FFFF" },
                new() { Name = "Fecha", DataType = "CurrentDate", X = 800, Y = 140, FontSize = 18, Color = "#666666" },
            }
        };
    }
    
    public static ThemeTemplate CreateMinimalSquare()
    {
        return new ThemeTemplate
        {
            Name = "Minimal Square",
            Description = "Diseño cuadrado minimalista para AIO",
            Category = "Square/AIO",
            Width = 480,
            Height = 480,
            Widgets = new List<TemplateWidget>
            {
                // Time centered
                new() { Name = "Hora", DataType = "CurrentTime", X = 100, Y = 50, FontSize = 72, Color = "#00FFFF", FontFamily = "Segoe UI Black" },
                new() { Name = "Fecha", DataType = "CurrentDate", X = 140, Y = 140, FontSize = 24, Color = "#666666" },
                
                // CPU/GPU row
                new() { Name = "CPU", DataType = "Static", X = 50, Y = 220, FontSize = 16, Color = "#00FFFF" },
                new() { Name = "CPU Temp", DataType = "CPUTemp", Unit = "°C", X = 50, Y = 250, FontSize = 36, Color = "#FFFFFF" },
                
                new() { Name = "GPU", DataType = "Static", X = 280, Y = 220, FontSize = 16, Color = "#FF8C00" },
                new() { Name = "GPU Temp", DataType = "GPUTemp", Unit = "°C", X = 280, Y = 250, FontSize = 36, Color = "#FFFFFF" },
                
                // RAM row
                new() { Name = "RAM", DataType = "Static", X = 50, Y = 330, FontSize = 16, Color = "#9400D3" },
                new() { Name = "RAM Usage", DataType = "MemoryUsage", Unit = "%", X = 50, Y = 360, FontSize = 36, Color = "#FFFFFF" },
                
                new() { Name = "FAN", DataType = "Static", X = 280, Y = 330, FontSize = 16, Color = "#32CD32" },
                new() { Name = "Fan Speed", DataType = "CPUFanSpeed", Unit = "RPM", X = 280, Y = 360, FontSize = 28, Color = "#FFFFFF" },
            }
        };
    }
    
    public static ThemeTemplate CreateFullSquare()
    {
        return new ThemeTemplate
        {
            Name = "Full Monitor Square",
            Description = "Monitor completo para pantallas AIO",
            Category = "Square/AIO",
            Width = 480,
            Height = 480,
            Widgets = new List<TemplateWidget>
            {
                // Header
                new() { Name = "SYSTEM", DataType = "Static", X = 180, Y = 15, FontSize = 20, Color = "#00FFFF", FontFamily = "Segoe UI Black" },
                
                // CPU Row
                new() { Name = "CPU", DataType = "Static", X = 20, Y = 55, FontSize = 14, Color = "#00FFFF" },
                new() { Name = "CPU Bar", DataType = "CpuUsage", Kind = WidgetKind.BorderLine, X = 20, Y = 75, Width = 440, Height = 12, Color = "#00FFFF" },
                new() { Name = "CPU Temp", DataType = "CPUTemp", Unit = "°C", X = 20, Y = 95, FontSize = 28, Color = "#FFFFFF" },
                new() { Name = "CPU Usage", DataType = "CPUUsage", Unit = "%", X = 130, Y = 95, FontSize = 28, Color = "#00FF00" },
                new() { Name = "CPU Clock", DataType = "CPUClock", Unit = "MHz", X = 240, Y = 95, FontSize = 28, Color = "#FFA500" },
                new() { Name = "CPU Power", DataType = "CPUPower", Unit = "W", X = 370, Y = 95, FontSize = 28, Color = "#FF4444" },
                
                // GPU Row
                new() { Name = "GPU", DataType = "Static", X = 20, Y = 140, FontSize = 14, Color = "#FF8C00" },
                new() { Name = "GPU Bar", DataType = "GpuUsage", Kind = WidgetKind.BorderLine, X = 20, Y = 160, Width = 440, Height = 12, Color = "#FF8C00" },
                new() { Name = "GPU Temp", DataType = "GPUTemp", Unit = "°C", X = 20, Y = 180, FontSize = 28, Color = "#FFFFFF" },
                new() { Name = "GPU Usage", DataType = "GPUUsage", Unit = "%", X = 130, Y = 180, FontSize = 28, Color = "#00FF00" },
                new() { Name = "GPU Clock", DataType = "GPUClock", Unit = "MHz", X = 240, Y = 180, FontSize = 28, Color = "#FFA500" },
                new() { Name = "GPU Power", DataType = "GPUPower", Unit = "W", X = 370, Y = 180, FontSize = 28, Color = "#FF4444" },
                
                // RAM Row
                new() { Name = "RAM", DataType = "Static", X = 20, Y = 225, FontSize = 14, Color = "#9400D3" },
                new() { Name = "RAM Bar", DataType = "MemoryUsage", Kind = WidgetKind.BorderLine, X = 20, Y = 245, Width = 440, Height = 12, Color = "#9400D3" },
                new() { Name = "RAM Used", DataType = "MemoryUsedGB", Unit = "GB", X = 20, Y = 265, FontSize = 28, Color = "#FFFFFF" },
                new() { Name = "RAM Usage", DataType = "MemoryUsage", Unit = "%", X = 130, Y = 265, FontSize = 28, Color = "#00FF00" },
                new() { Name = "RAM Clock", DataType = "MemoryClock", Unit = "MHz", X = 240, Y = 265, FontSize = 28, Color = "#FFA500" },
                
                // Fans Row
                new() { Name = "FANS", DataType = "Static", X = 20, Y = 310, FontSize = 14, Color = "#32CD32" },
                new() { Name = "CPU Fan", DataType = "CPUFanSpeed", Unit = "RPM", X = 20, Y = 335, FontSize = 24, Color = "#FFFFFF" },
                new() { Name = "GPU Fan", DataType = "GPUFanSpeed", Unit = "RPM", X = 180, Y = 335, FontSize = 24, Color = "#FFFFFF" },
                new() { Name = "Fan 1", DataType = "Fan1", Unit = "RPM", X = 340, Y = 335, FontSize = 24, Color = "#FFFFFF" },
                
                // Time at bottom
                new() { Name = "Hora", DataType = "CurrentTime", X = 150, Y = 390, FontSize = 48, Color = "#00FFFF" },
                new() { Name = "Fecha", DataType = "CurrentDate", X = 170, Y = 445, FontSize = 16, Color = "#666666" },
            }
        };
    }
    
    public static ThemeTemplate CreateCompactDisplay()
    {
        return new ThemeTemplate
        {
            Name = "Compact Display",
            Description = "Para pantallas pequeñas (320x240)",
            Category = "Compact",
            Width = 320,
            Height = 240,
            Widgets = new List<TemplateWidget>
            {
                // Time
                new() { Name = "Hora", DataType = "CurrentTime", X = 80, Y = 20, FontSize = 40, Color = "#00FFFF" },
                
                // CPU/GPU row
                new() { Name = "CPU", DataType = "CPUTemp", Unit = "°C", X = 20, Y = 90, FontSize = 28, Color = "#FFFFFF" },
                new() { Name = "GPU", DataType = "GPUTemp", Unit = "°C", X = 170, Y = 90, FontSize = 28, Color = "#FFFFFF" },
                
                // Usage row
                new() { Name = "CPU%", DataType = "CPUUsage", Unit = "%", X = 20, Y = 140, FontSize = 28, Color = "#00FF00" },
                new() { Name = "GPU%", DataType = "GPUUsage", Unit = "%", X = 170, Y = 140, FontSize = 28, Color = "#00FF00" },
                
                // RAM
                new() { Name = "RAM", DataType = "MemoryUsage", Unit = "%", X = 100, Y = 190, FontSize = 28, Color = "#9400D3" },
            }
        };
    }
}
