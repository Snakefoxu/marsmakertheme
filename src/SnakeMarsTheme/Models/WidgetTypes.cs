namespace SnakeMarsTheme.Models;

public enum WidgetKind
{
    Text,
    BorderLine,
    DefaultLine,
    GridLine
}

public class WidgetTemplate
{
    public string Icon { get; set; }
    public string Name { get; set; }
    public string DataType { get; set; }
    public string Unit { get; set; }
    public WidgetKind Kind { get; set; }
    
    public WidgetTemplate(string icon, string name, string dataType, string unit, WidgetKind kind = WidgetKind.Text)
    {
        Icon = icon;
        Name = name;
        DataType = dataType;
        Unit = unit;
        Kind = kind;
    }
}
