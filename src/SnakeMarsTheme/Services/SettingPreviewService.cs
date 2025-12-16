using System.IO;
using System.Text;
using System.Text.RegularExpressions;
using SnakeMarsTheme.Models;

namespace SnakeMarsTheme.Services;

/// <summary>
/// Generates Setting.txt preview and validates theme projects.
/// </summary>
public class SettingPreviewService
{
    private static readonly string[] AvailableFonts = new[]
    {
        "Impact", "Consolas", "Arial", "Tahoma", "Verdana", "Segoe UI",
        "Comic Sans MS", "Courier New", "Times New Roman", "Georgia",
        "Trebuchet MS", "Lucida Console", "Palatino Linotype",
        "Microsoft YaHei", "SimHei", "KaiTi", "FangSong",
        "Orbitron", "Rajdhani", "Russo One", "Bebas Neue"
    };

    /// <summary>
    /// Generates a Setting.txt file content from a ThemeProject.
    /// </summary>
    public string GenerateSettingPreview(ThemeProject project)
    {
        var sb = new StringBuilder();
        
        // Header
        sb.AppendLine($"name:{project.ThemeName}");
        sb.AppendLine($"width:{project.Width}");
        sb.AppendLine($"height:{project.Height}");
        sb.AppendLine();
        
        // Background image (implicit at z=-100)
        if (!string.IsNullOrEmpty(project.BackgroundPath))
        {
            var bgFileName = Path.GetFileName(project.BackgroundPath);
            sb.AppendLine($"# Background");
            sb.AppendLine($"{bgFileName}:x@0,y@0,z@-100");
            sb.AppendLine();
        }
        
        // Sort widgets by Z-index (back to front)
        var sortedWidgets = project.Widgets.OrderBy(w => w.Z).ToList();
        
        // Group by widget kind
        var textWidgets = sortedWidgets.Where(w => w.Kind == WidgetKind.Text).ToList();
        var barWidgets = sortedWidgets.Where(w => w.Kind != WidgetKind.Text).ToList();
        
        // Text widgets
        if (textWidgets.Any())
        {
            sb.AppendLine("# Text Widgets");
            foreach (var widget in textWidgets)
            {
                sb.AppendLine(GenerateTextLine(widget));
            }
            sb.AppendLine();
        }
        
        // Bar widgets
        if (barWidgets.Any())
        {
            sb.AppendLine("# Bar Widgets");
            foreach (var widget in barWidgets)
            {
                sb.AppendLine(GenerateBarLine(widget));
            }
            sb.AppendLine();
        }
        
        return sb.ToString();
    }

    /// <summary>
    /// Generates a Text: line from a ProjectWidget.
    /// </summary>
    private string GenerateTextLine(ProjectWidget widget)
    {
        var parts = new List<string>
        {
            $"x@{widget.X}",
            $"y@{widget.Y}",
            $"z@{widget.Z}",
            $"FontSize@{widget.FontSize}",
            $"FontFamily@{widget.FontFamily}",
            $"Foreground@{NormalizeColor(widget.Color)}"
        };
        
        if (!string.IsNullOrEmpty(widget.DataType))
        {
            parts.Add($"data@{widget.DataType}");
        }
        
        if (!string.IsNullOrEmpty(widget.Unit))
        {
            parts.Add($"unit@{widget.Unit}");
        }
        
        if (widget.Width > 0)
        {
            parts.Add($"maxwidth@{widget.Width}");
        }
        
        if (widget.Height > 0)
        {
            parts.Add($"maxheight@{widget.Height}");
        }
        
        return $"Text:{string.Join(",", parts)}";
    }

    /// <summary>
    /// Generates a BorderLine/DefaultLine/GridLine from a ProjectWidget.
    /// </summary>
    private string GenerateBarLine(ProjectWidget widget)
    {
        string lineType = widget.Kind switch
        {
            WidgetKind.BorderLine => "BorderLine",
            WidgetKind.DefaultLine => "DefaultLine",
            WidgetKind.GridLine => "GridLine",
            _ => "BorderLine"
        };
        
        var parts = new List<string>
        {
            $"x@{widget.X}",
            $"y@{widget.Y}",
            $"z@{widget.Z}",
            $"maxwidth@{widget.BarWidth}",
            $"maxheight@{widget.BarHeight}",
            $"Fill@{NormalizeColor(widget.Fill)}"
        };
        
        if (!string.IsNullOrEmpty(widget.DataType))
        {
            parts.Add($"data@{widget.DataType}");
        }
        
        if (widget.Kind == WidgetKind.BorderLine)
        {
            parts.Add($"MaxNum@{widget.MaxNum}");
            parts.Add($"CornerRadius@{widget.CornerRadius}");
            
            if (!string.IsNullOrEmpty(widget.BackColor) && widget.BackColor != "#333333")
            {
                parts.Add($"BackColor@{NormalizeColor(widget.BackColor)}");
            }
        }
        
        return $"{lineType}:{string.Join(",", parts)}";
    }

    /// <summary>
    /// Normalizes color to #RRGGBB or #AARRGGBB format.
    /// </summary>
    private string NormalizeColor(string color)
    {
        if (string.IsNullOrEmpty(color))
            return "#FFFFFF";
        
        color = color.Trim().ToUpper();
        if (!color.StartsWith("#"))
            color = "#" + color;
        
        return color;
    }

    /// <summary>
    /// Validates a ThemeProject and returns a list of validation messages.
    /// </summary>
    public List<ValidationMessage> ValidateSetting(ThemeProject project)
    {
        var errors = new List<ValidationMessage>();
        
        // Validate dimensions
        if (project.Width <= 0 || project.Height <= 0)
        {
            errors.Add(new ValidationMessage
            {
                Severity = ValidationSeverity.Error,
                Message = $"Dimensiones inválidas: {project.Width}x{project.Height}"
            });
        }
        
        // Validate background
        if (!string.IsNullOrEmpty(project.BackgroundPath) && !File.Exists(project.BackgroundPath))
        {
            errors.Add(new ValidationMessage
            {
                Severity = ValidationSeverity.Warning,
                Message = $"Imagen de fondo no encontrada: {Path.GetFileName(project.BackgroundPath)}"
            });
        }
        
        // Validate each widget
        foreach (var widget in project.Widgets)
        {
            // Position bounds
            if (widget.X < 0 || widget.X >= project.Width)
            {
                errors.Add(new ValidationMessage
                {
                    Severity = ValidationSeverity.Warning,
                    Message = $"Widget '{widget.Name}' fuera de límites X: {widget.X} (máx: {project.Width})"
                });
            }
            
            if (widget.Y < 0 || widget.Y >= project.Height)
            {
                errors.Add(new ValidationMessage
                {
                    Severity = ValidationSeverity.Warning,
                    Message = $"Widget '{widget.Name}' fuera de límites Y: {widget.Y} (máx: {project.Height})"
                });
            }
            
            // Font validation
            if (widget.Kind == WidgetKind.Text)
            {
                if (!AvailableFonts.Contains(widget.FontFamily))
                {
                    errors.Add(new ValidationMessage
                    {
                        Severity = ValidationSeverity.Info,
                        Message = $"Fuente '{widget.FontFamily}' no está en la lista de fuentes conocidas"
                    });
                }
                
                if (widget.FontSize < 6 || widget.FontSize > 200)
                {
                    errors.Add(new ValidationMessage
                    {
                        Severity = ValidationSeverity.Warning,
                        Message = $"Tamaño de fuente inusual en '{widget.Name}': {widget.FontSize}px"
                    });
                }
            }
            
            // Color validation
            if (!IsValidColor(widget.Color))
            {
                errors.Add(new ValidationMessage
                {
                    Severity = ValidationSeverity.Error,
                    Message = $"Color inválido en '{widget.Name}': {widget.Color}"
                });
            }
            
            if (widget.Kind != WidgetKind.Text && !IsValidColor(widget.Fill))
            {
                errors.Add(new ValidationMessage
                {
                    Severity = ValidationSeverity.Error,
                    Message = $"Color Fill inválido en '{widget.Name}': {widget.Fill}"
                });
            }
            
            // Data type validation
            if (string.IsNullOrEmpty(widget.DataType) && widget.Kind != WidgetKind.DefaultLine)
            {
                errors.Add(new ValidationMessage
                {
                    Severity = ValidationSeverity.Warning,
                    Message = $"Widget '{widget.Name}' no tiene DataType asignado"
                });
            }
        }
        
        // Success message if no errors
        if (!errors.Any())
        {
            errors.Add(new ValidationMessage
            {
                Severity = ValidationSeverity.Success,
                Message = $"✅ Tema válido - {project.Widgets.Count} widgets, sin errores"
            });
        }
        
        return errors;
    }

    /// <summary>
    /// Validates if a color string is in correct format.
    /// </summary>
    private bool IsValidColor(string color)
    {
        if (string.IsNullOrEmpty(color))
            return false;
        
        color = color.Trim();
        if (!color.StartsWith("#"))
            color = "#" + color;
        
        // Valid formats: #RGB, #RRGGBB, #AARRGGBB
        return Regex.IsMatch(color, @"^#([0-9A-Fa-f]{3}|[0-9A-Fa-f]{6}|[0-9A-Fa-f]{8})$");
    }
}

/// <summary>
/// Represents a validation message.
/// </summary>
public class ValidationMessage
{
    public ValidationSeverity Severity { get; set; }
    public string Message { get; set; } = "";
}

/// <summary>
/// Validation message severity levels.
/// </summary>
public enum ValidationSeverity
{
    Success,
    Info,
    Warning,
    Error
}
