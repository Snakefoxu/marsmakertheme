using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SnakeMarsTheme.Models;
using SnakeMarsTheme.Services;

namespace SnakeMarsTheme.ViewModels;

// Snapshot for Undo/Redo system
public class FileItem
{
    public string Name { get; set; } = "";
    public string FullPath { get; set; } = "";
    public string Icon { get; set; } = "📄"; // 🎥 o 🖼️
    public string? PreviewPath { get; set; }
}

public class EditorSnapshot
{
    public List<WidgetSnapshot> Widgets { get; set; } = new();
    public string ThemeName { get; set; } = "";
    public int ThemeWidth { get; set; }
    public int ThemeHeight { get; set; }
    public string BackgroundPath { get; set; } = "";
}

public class WidgetSnapshot
{
    public string Name { get; set; } = "";
    public string DataType { get; set; } = "";
    public string Unit { get; set; } = "";
    public WidgetKind Kind { get; set; }
    public int X { get; set; }
    public int Y { get; set; }
    public int Z { get; set; }
    public int FontSize { get; set; }
    public string FontFamily { get; set; } = "";
    public string Color { get; set; } = "";
    public int Width { get; set; }
    public int Height { get; set; }
    public int BarWidth { get; set; }
    public int BarHeight { get; set; }
    public int MaxNum { get; set; }
    public int CornerRadius { get; set; }
    public string Fill { get; set; } = "";
    public string BackColor { get; set; } = "";
}

public partial class ThemeEditorViewModel : ObservableObject
{
    private readonly ThemeCreatorService _themeCreatorService;
    private readonly ExtractionService _extractionService;
    private readonly ProjectService _projectService;
    private readonly SettingPreviewService _settingPreviewService;
    private readonly SmthemePackagerService _smthemePackagerService;
    private readonly string _basePath;
    private PlacedWidgetItem? _clipboardWidget;
    private string? _currentProjectPath;
    
    // Undo/Redo stacks
    private readonly Stack<EditorSnapshot> _undoStack = new();
    private readonly Stack<EditorSnapshot> _redoStack = new();
    private const int MaxUndoLevels = 50;
    private bool _isApplyingSnapshot = false;
    
    // Grid settings
    [ObservableProperty]
    private bool _snapToGrid = true;
    
    [ObservableProperty]
    private int _gridSize = 10;
    
    public bool Is7ZipAvailable => _extractionService.Is7ZipAvailable();
    public bool HasProject => !string.IsNullOrEmpty(_currentProjectPath);
    public string ProjectStatus => HasProject 
        ? $"📁 {Path.GetFileName(_currentProjectPath)}" 
        : "Sin guardar";
    
    [ObservableProperty]
    private string _themeName = "NuevoTema";
    
    [ObservableProperty]
    private int _themeWidth = 360;
    
    [ObservableProperty]
    private int _themeHeight = 960;
    
    [ObservableProperty]
    private int _zoomLevel = 50;
    
    [ObservableProperty]
    private BitmapImage? _backgroundImage;
    
    [ObservableProperty]
    private string _backgroundPath = "";
    
    [ObservableProperty]
    private bool _isVideoBackground;
    
    [ObservableProperty]
    private Uri? _videoSource;
    
    [ObservableProperty]
    private bool _isGifBackground;
    
    [ObservableProperty]
    private string? _gifSourcePath;
    
    /// <summary>
    /// Rotation angle for the background (0, 90, 180, 270)
    /// </summary>
    [ObservableProperty]
    private int _backgroundRotation = 0;
    
    [ObservableProperty]
    private ObservableCollection<PlacedWidgetItem> _placedWidgets = new();
    
    [ObservableProperty]
    private PlacedWidgetItem? _selectedWidget;
    
    // Multiple selection support
    public ObservableCollection<PlacedWidgetItem> SelectedWidgets { get; } = new();
    public bool HasMultipleSelection => SelectedWidgets.Count > 1;
    public string SelectionInfo => SelectedWidgets.Count > 0 
        ? $"{SelectedWidgets.Count} widget(s) seleccionado(s)" 
        : "Sin selección";
    
    [ObservableProperty]
    private ObservableCollection<WidgetTemplate> _availableWidgets = new();
    
    [ObservableProperty]
    private WidgetTemplate? _selectedAvailableWidget;
    
    // --- MEDIA_LIBRARY ---
    [ObservableProperty]
    private ObservableCollection<FileItem> _availableGifs = new();
    
    [ObservableProperty]
    private FileItem? _selectedMediaItem;
    
    [ObservableProperty]
    private bool _showMediaLibrary; // Toggle between Widgets and Media
    // ---------------------

    [ObservableProperty]
    private string _selectedCategory = "CPU";
    
    [ObservableProperty]
    private ThemeTemplate? _selectedTemplate;
    
    // Preview and Validation properties
    [ObservableProperty]
    private string _settingPreviewText = "// Preview de Setting.txt aparecerá aquí";
    
    [ObservableProperty]
    private ObservableCollection<ValidationMessage> _validationErrors = new();
    
    [ObservableProperty]
    private bool _isPreviewExpanded = true;

    public double ZoomScale => ZoomLevel / 100.0;
    public double CanvasWidth => ThemeWidth * ZoomScale;
    public double CanvasHeight => ThemeHeight * ZoomScale;
    public string CanvasResolution => $"{ThemeWidth}x{ThemeHeight}";
    
    // Undo/Redo status
    public bool CanUndo => _undoStack.Count > 0;
    public bool CanRedo => _redoStack.Count > 0;
    public string UndoStatus => $"Undo: {_undoStack.Count} | Redo: {_redoStack.Count}";
    
    public ObservableCollection<ThemeTemplate> AvailableTemplates { get; } = new();
    
    public ObservableCollection<string> Categories { get; } = new()
    {
        "CPU", "GPU", "Memoria", "Sistema", "Ventiladores", "Disco", "Red", "Clima", "Etiquetas", "Barras"
    };
    
    public ObservableCollection<string> AvailableFonts { get; } = new()
    {
        // Popular fonts for themes
        "Impact", "Arial", "Arial Black", "Segoe UI", "Segoe UI Black",
        "Verdana", "Tahoma", "Trebuchet MS", "Century Gothic",
        // Monospace
        "Consolas", "Courier New", "Lucida Console",
        // Decorative
        "Georgia", "Times New Roman", "Palatino Linotype",
        // Display
        "Bauhaus 93", "Broadway", "Stencil", "Agency FB", "Berlin Sans FB",
        // Fun
        "Comic Sans MS", "Papyrus"
    };
    
    public ObservableCollection<string> AvailableColors { get; } = new()
    {
        // Basic
        "#FFFFFF", "#000000", "#CCCCCC", "#666666", "#333333",
        // Cyan/Blue
        "#00FFFF", "#00D4FF", "#00CED1", "#1E90FF", "#0000FF",
        // Green
        "#00FF00", "#32CD32", "#7FFF00", "#00FA9A",
        // Red/Pink
        "#FF0000", "#FF4444", "#FF69B4", "#FF1493",
        // Yellow/Orange
        "#FFFF00", "#FFD700", "#FFA500", "#FF8C00",
        // Purple
        "#FF00FF", "#9400D3", "#8A2BE2", "#9370DB"
    };
    
    public ThemeEditorViewModel()
    {
        var exePath = AppDomain.CurrentDomain.BaseDirectory;
        _basePath = Path.GetFullPath(Path.Combine(exePath, "..", "..", "..", "..", ".."));
        _themeCreatorService = new ThemeCreatorService(_basePath);
        _extractionService = new ExtractionService(_basePath);
        _projectService = new ProjectService(_basePath);
        _settingPreviewService = new SettingPreviewService();
        _smthemePackagerService = new SmthemePackagerService();
        
        LoadWidgetsForCategory();
        LoadTemplates();
        LoadMediaResources();
        UpdatePreview(); // Initial preview generation
    }
    
    private void LoadMediaResources()
    {
        AvailableGifs.Clear();

        // 1. Scan Resources in Repo Root (Dev Env)
        var repoResources = Path.Combine(_basePath, "resources");
        ScanMediaInFolder(repoResources);
        
        // 2. Scan Resources in App Directory (Prod/Release Env)
        var appResources = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources");
        if (Directory.Exists(appResources) && appResources != repoResources)
        {
             ScanMediaInFolder(appResources);
        }
    }

    private void ScanMediaInFolder(string rootPath)
    {
        if (!Directory.Exists(rootPath)) return;

        // GIFs (includes converted videos)
        var gifPath = Path.Combine(rootPath, "GIFs");
        if (Directory.Exists(gifPath))
        {
            var files = Directory.GetFiles(gifPath, "*.gif", SearchOption.AllDirectories);
            var previewsPath = Path.Combine(rootPath, "Previews");
            
            foreach(var f in files)
            {
                if(!AvailableGifs.Any(g => g.FullPath == f))
                {
                    var item = new FileItem 
                    { 
                        Name = Path.GetFileName(f), 
                        FullPath = f, 
                        Icon = "🎞️" 
                    };

                    // Try detecting preview
                    if (Directory.Exists(previewsPath))
                    {
                        var nameNoExt = Path.GetFileNameWithoutExtension(f);
                        var pngPreview = Path.Combine(previewsPath, nameNoExt + ".png");
                        if (File.Exists(pngPreview))
                        {
                            item.PreviewPath = pngPreview;
                        }
                    }

                    AvailableGifs.Add(item);
                }
            }
        }
    }

    [RelayCommand]
    private void ToggleLibraryView()
    {
        ShowMediaLibrary = !ShowMediaLibrary;
    }

    [RelayCommand]
    private void ApplyMediaAsBackground(FileItem item)
    {
        if(item == null) return;
        
        SaveStateForUndo();
        
        // Clear previous background state
        BackgroundImage = null;
        VideoSource = null;
        GifSourcePath = null;
        IsVideoBackground = false;
        IsGifBackground = false;
        
        // Verify file exists
        if (!File.Exists(item.FullPath))
        {
            MessageBox.Show($"Archivo no encontrado:\n{item.FullPath}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            return;
        }
        
        System.Diagnostics.Debug.WriteLine($"[MEDIA] Applying: {item.FullPath}");
        
        var ext = Path.GetExtension(item.FullPath).ToLowerInvariant();
        
        if (ext == ".gif")
        {
            // GIFs use XamlAnimatedGif directly
            IsGifBackground = true;
            GifSourcePath = item.FullPath;
            System.Diagnostics.Debug.WriteLine($"[GIF] Set GifSourcePath: {GifSourcePath}");
        }
        else if (ext == ".png" || ext == ".jpg" || ext == ".jpeg")
        {
            // Static images
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(item.FullPath);
                bitmap.EndInit();
                BackgroundImage = bitmap;
            }
            catch { }
        }
        else
        {
            // Videos no soportados - usa los GIFs de la biblioteca
            MessageBox.Show(
                "Los videos han sido convertidos a GIFs.\n\n" +
                "Busca el GIF correspondiente en la biblioteca de GIFs.",
                "Info",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
            return;
        }
        
        BackgroundPath = item.FullPath;
    }
    
    private void LoadTemplates()
    {
        var templates = ThemeTemplateFactory.GetAllTemplates();
        AvailableTemplates.Clear();
        foreach (var t in templates)
            AvailableTemplates.Add(t);
    }
    
    // ============== Undo/Redo System ==============
    
    private EditorSnapshot CreateSnapshot()
    {
        var snapshot = new EditorSnapshot
        {
            ThemeName = ThemeName,
            ThemeWidth = ThemeWidth,
            ThemeHeight = ThemeHeight,
            BackgroundPath = BackgroundPath,
            Widgets = PlacedWidgets.Select(w => new WidgetSnapshot
            {
                Name = w.Name,
                DataType = w.DataType,
                Unit = w.Unit,
                Kind = w.Kind,
                X = w.X,
                Y = w.Y,
                Z = w.Z,
                FontSize = w.FontSize,
                FontFamily = w.FontFamily,
                Color = w.Color,
                Width = w.Width,
                Height = w.Height,
                BarWidth = w.BarWidth,
                BarHeight = w.BarHeight,
                MaxNum = w.MaxNum,
                CornerRadius = w.CornerRadius,
                Fill = w.Fill,
                BackColor = w.BackColor
            }).ToList()
        };
        return snapshot;
    }
    
    private void ApplySnapshot(EditorSnapshot snapshot)
    {
        _isApplyingSnapshot = true;
        
        ThemeName = snapshot.ThemeName;
        ThemeWidth = snapshot.ThemeWidth;
        ThemeHeight = snapshot.ThemeHeight;
        BackgroundPath = snapshot.BackgroundPath;
        
        PlacedWidgets.Clear();
        foreach (var ws in snapshot.Widgets)
        {
            PlacedWidgets.Add(new PlacedWidgetItem
            {
                Name = ws.Name,
                DataType = ws.DataType,
                Unit = ws.Unit,
                Kind = ws.Kind,
                X = ws.X,
                Y = ws.Y,
                Z = ws.Z,
                FontSize = ws.FontSize,
                FontFamily = ws.FontFamily,
                Color = ws.Color,
                Width = ws.Width,
                Height = ws.Height,
                BarWidth = ws.BarWidth,
                BarHeight = ws.BarHeight,
                MaxNum = ws.MaxNum,
                CornerRadius = ws.CornerRadius,
                Fill = ws.Fill,
                BackColor = ws.BackColor,
                IsSelected = false
            });
        }
        
        SelectedWidget = null;
        
        _isApplyingSnapshot = false;
        
        OnPropertyChanged(nameof(CanvasWidth));
        OnPropertyChanged(nameof(CanvasHeight));
        OnPropertyChanged(nameof(CanvasResolution));
    }
    
    public void SaveStateForUndo()
    {
        if (_isApplyingSnapshot) return;
        
        _undoStack.Push(CreateSnapshot());
        _redoStack.Clear();
        
        // Limit undo levels
        while (_undoStack.Count > MaxUndoLevels)
        {
            var temp = new Stack<EditorSnapshot>();
            for (int i = 0; i < MaxUndoLevels; i++)
                temp.Push(_undoStack.Pop());
            _undoStack.Clear();
            while (temp.Count > 0)
                _undoStack.Push(temp.Pop());
        }
        
        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
        OnPropertyChanged(nameof(UndoStatus));
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
    }
    
    [RelayCommand(CanExecute = nameof(CanUndo))]
    private void Undo()
    {
        if (_undoStack.Count == 0) return;
        
        // Save current state to redo stack
        _redoStack.Push(CreateSnapshot());
        
        // Apply previous state
        var snapshot = _undoStack.Pop();
        ApplySnapshot(snapshot);
        
        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
        OnPropertyChanged(nameof(UndoStatus));
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
    }
    
    [RelayCommand(CanExecute = nameof(CanRedo))]
    private void Redo()
    {
        if (_redoStack.Count == 0) return;
        
        // Save current state to undo stack
        _undoStack.Push(CreateSnapshot());
        
        // Apply redo state
        var snapshot = _redoStack.Pop();
        ApplySnapshot(snapshot);
        
        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
        OnPropertyChanged(nameof(UndoStatus));
        UndoCommand.NotifyCanExecuteChanged();
        RedoCommand.NotifyCanExecuteChanged();
    }
    
    [RelayCommand]
    private void SetRotation(string angleStr)
    {
        if (int.TryParse(angleStr, out int angle))
        {
            if (BackgroundRotation != angle)
            {
                SaveStateForUndo();
                BackgroundRotation = angle;
            }
        }
    }
    
    // ============== End Undo/Redo ==============

    [RelayCommand]
    private void ApplyTemplate()
    {
        if (SelectedTemplate == null) return;
        
        // Ask confirmation if there are existing widgets
        if (PlacedWidgets.Count > 0)
        {
            var result = MessageBox.Show(
                $"¿Deseas reemplazar los {PlacedWidgets.Count} widgets actuales con la plantilla '{SelectedTemplate.Name}'?",
                "Aplicar Plantilla",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            
            if (result != MessageBoxResult.Yes) return;
        }
        
        SaveStateForUndo();
        
        // Clear existing widgets
        PlacedWidgets.Clear();
        
        // Set theme dimensions
        ThemeWidth = SelectedTemplate.Width;
        ThemeHeight = SelectedTemplate.Height;
        ThemeName = SelectedTemplate.Name;
        
        // Add widgets from template
        int zIndex = 1;
        foreach (var tw in SelectedTemplate.Widgets)
        {
            var widget = new PlacedWidgetItem
            {
                Name = tw.Name,
                DataType = tw.DataType,
                Unit = tw.Unit,
                Kind = tw.Kind,
                X = tw.X,
                Y = tw.Y,
                Z = zIndex++,
                FontSize = tw.FontSize,
                FontFamily = tw.FontFamily,
                Color = tw.Color,
                Width = tw.Width,
                Height = tw.Height,
                IsSelected = false
            };
            PlacedWidgets.Add(widget);
        }
        
        OnPropertyChanged(nameof(CanvasWidth));
        OnPropertyChanged(nameof(CanvasHeight));
        OnPropertyChanged(nameof(CanvasResolution));
        
        MessageBox.Show(
            $"Plantilla '{SelectedTemplate.Name}' aplicada.\n{PlacedWidgets.Count} widgets agregados.",
            "Plantilla Aplicada",
            MessageBoxButton.OK,
            MessageBoxImage.Information);
    }
    
    partial void OnZoomLevelChanged(int value)
    {
        OnPropertyChanged(nameof(ZoomScale));
        OnPropertyChanged(nameof(CanvasWidth));
        OnPropertyChanged(nameof(CanvasHeight));
    }
    
    partial void OnSelectedCategoryChanged(string value)
    {
        LoadWidgetsForCategory();
    }
    
    private void LoadWidgetsForCategory()
    {
        var widgets = GetWidgetsForCategory(SelectedCategory);
        AvailableWidgets.Clear();
        foreach (var w in widgets)
            AvailableWidgets.Add(w);
    }
    
    private List<WidgetTemplate> GetWidgetsForCategory(string category)
    {
        return category switch
        {
            "CPU" => new()
            {
                // Temperatura
                new("🌡️", "Temperatura CPU", "CPUTemp", "°C"),
                new("🌡️", "Temp CPU (SOEYI)", "CPUT", "°C"),
                // Uso
                new("📊", "Uso CPU", "CPUUsage", "%"),
                new("📊", "Uso CPU (alias)", "CpuUsage", "%"),
                // Frecuencia
                new("⚡", "Frecuencia CPU", "CPUClock", "MHz"),
                new("⚡", "Frecuencia CPU (alias)", "CpuFrequency", "MHz"),
                // Voltaje
                new("🔌", "Voltaje CPU", "CPUVoltage", "V"),
                new("🔌", "Voltaje CPU (alias)", "CpuVoltage", "V"),
                // Potencia
                new("⚡", "Potencia CPU", "CPUPower", "W"),
                new("⚡", "TDP CPU", "CpuTEC", "W"),
                // Fan
                new("🌀", "Ventilador CPU", "CPUFanSpeed", "RPM"),
                // Etiqueta
                new("🏷️", "Etiqueta CPU", "CPU", ""),
            },
            "GPU" => new()
            {
                // Temperatura
                new("🌡️", "Temperatura GPU", "GPUTemp", "°C"),
                new("🌡️", "Temp GPU (SOEYI)", "GPUT", "°C"),
                // Uso
                new("📊", "Uso GPU", "GPUUsage", "%"),
                new("📊", "Uso GPU (alias)", "GpuUsage", "%"),
                // Frecuencia
                new("⚡", "Frecuencia GPU", "GPUClock", "MHz"),
                new("⚡", "Frecuencia GPU (alias)", "GpuFrequency", "MHz"),
                // Memoria
                new("💾", "Memoria GPU usada", "GPUMemUsed", "MB"),
                new("💾", "Uso VRAM (%)", "GPUMemoryLoad", "%"),
                // Potencia
                new("⚡", "Potencia GPU", "GPUPower", "W"),
                new("⚡", "TDP GPU", "GpuTEC", "W"),
                // Fan
                new("🌀", "Ventilador GPU", "GPUFanSpeed", "RPM"),
                // Etiqueta
                new("🏷️", "Etiqueta GPU", "GPU", ""),
            },
            "Memoria" => new()
            {
                // Uso
                new("📊", "Uso de memoria", "MemoryUsed", "%"),
                new("📊", "Uso memoria (alias)", "MemoryUsage", "%"),
                // GB
                new("💾", "Memoria usada (GB)", "MemoryUsedGB", "GB"),
                new("💾", "Memoria usada (alias)", "MemoryUse", "GB"),
                new("💾", "Memoria total", "MemoryTotal", "GB"),
                // Frecuencia
                new("⚡", "Frecuencia RAM", "MemoryClock", "MHz"),
                new("⚡", "Frecuencia RAM (alias)", "MemoryFrequency", "MHz"),
                // Entero
                new("🔢", "Uso RAM entero", "MemoryUseInt", ""),
                // Etiquetas
                new("🏷️", "Etiqueta RAM", "RAM", ""),
                new("🏷️", "Etiqueta ROM", "ROM", ""),
            },
            "Sistema" => new()
            {
                // Hora
                new("🕐", "Hora actual", "CurrentTime", ""),
                new("🕐", "Hora (TIME)", "TIME", ""),
                new("🕐", "Hora alternativa", "CurrentTimeShut", ""),
                new("🕐", "Solo hora", "CurrentH", ""),
                new("🕐", "Solo minutos", "CurrentM", ""),
                // Fecha
                new("📅", "Fecha actual", "CurrentDate", ""),
                new("📅", "Fecha completa", "CurrentDates", ""),
                new("📅", "Solo año", "CurrentDatesY", ""),
                new("📅", "Solo mes", "CurrentDatesM", ""),
                new("📅", "Solo día", "CurrentDatesD", ""),
                new("📅", "Mes y día", "CurrentDatesMD", ""),
                // Semana
                new("📆", "Día de semana", "CurrentWeek", ""),
                new("📆", "Día semana (alias)", "WeekDays", ""),
                new("📆", "Hoy", "Today", ""),
                // Otros
                new("🌙", "Fecha lunar", "LunarDate", ""),
                new("🖥️", "Tasa refresco", "RefreshRate", "Hz"),
            },
            "Ventiladores" => new()
            {
                new("🌀", "Ventilador CPU", "CPUFanSpeed", "RPM"),
                new("🌀", "Ventilador GPU", "GPUFanSpeed", "RPM"),
                new("🌀", "Ventilador 1", "Fan1", "RPM"),
                new("🌀", "Ventilador 2", "Fan2", "RPM"),
                new("🌀", "Ventilador 3", "Fan3", "RPM"),
                new("🌀", "Ventilador 4", "Fan4", "RPM"),
                new("🌀", "Ventiladores (general)", "Fans", "RPM"),
            },
            "Disco" => new()
            {
                new("🌡️", "Temperatura disco", "DiskTemp", "°C"),
                new("📊", "Uso de disco", "DiskUsage", "%"),
                new("📊", "Uso disco (alias)", "DiskUtilizations", "%"),
                new("💾", "Espacio libre", "DiskFree", "GB"),
            },
            "Red" => new()
            {
                new("⬇️", "Velocidad descarga", "NetworkDown", "KB/s"),
                new("⬆️", "Velocidad subida", "NetworkUp", "KB/s"),
                new("📶", "Estado WiFi", "WifiStatus", ""),
                new("📶", "Nombre WiFi", "WifiName", ""),
            },
            "Clima" => new()
            {
                new("🌡️", "Temperatura clima", "WeatherInfo", "°C"),
                new("🌙", "Clima nocturno", "Nightweather", ""),
                new("☀️", "Clima máxima", "Heightweather", ""),
                new("🌤️", "Clima mínima", "Lowweather", ""),
                new("🌦️", "Condición clima", "WeatherCondition", ""),
            },
            "Etiquetas" => new()
            {
                new("🏷️", "Etiqueta CPU", "CPU", ""),
                new("🏷️", "Etiqueta GPU", "GPU", ""),
                new("🏷️", "Etiqueta RAM", "RAM", ""),
                new("🏷️", "Etiqueta ROM", "ROM", ""),
                new("🏷️", "Etiqueta USAGE", "USAGE", ""),
                new("🏷️", "Etiqueta DAMM", "DAMM", ""),
                new("🐱", "Kitten (decorativo)", "Kitten", ""),
                new("📝", "Texto estático", "Static", ""),
            },
            "Barras" => new()
            {
                new("▰", "[BAR] CPU", "CpuUsage", "%", WidgetKind.BorderLine),
                new("▰", "[BAR] GPU", "GpuUsage", "%", WidgetKind.BorderLine),
                new("▰", "[BAR] RAM", "MemoryUsage", "%", WidgetKind.BorderLine),
                new("▱", "[BACK] Fondo", "Static", "", WidgetKind.DefaultLine),
                new("▤", "[GRID] CPU", "CpuUsage", "%", WidgetKind.GridLine),
            },
            _ => new()
        };
    }
    
    [RelayCommand]
    private void AddWidget()
    {
        if (SelectedAvailableWidget == null) return;
        
        SaveStateForUndo();
        
        var widget = new PlacedWidgetItem
        {
            Name = SelectedAvailableWidget.Name,
            DataType = SelectedAvailableWidget.DataType,
            Unit = SelectedAvailableWidget.Unit,
            Kind = SelectedAvailableWidget.Kind,
            X = 20 + (PlacedWidgets.Count * 10),
            Y = 50 + (PlacedWidgets.Count * 40),
            Z = PlacedWidgets.Count + 1,
            FontSize = 24,
            FontFamily = "Impact",
            Color = "#FFFFFF",
            BarWidth = 100,
            BarHeight = 10,
            MaxNum = 100
        };
        
        PlacedWidgets.Add(widget);
        SelectedWidget = widget;
        UpdatePreview(); // Update preview when widget is added
    }
    
    [RelayCommand]
    private void DeleteWidget()
    {
        if (SelectedWidget == null) return;
        
        SaveStateForUndo();
        PlacedWidgets.Remove(SelectedWidget);
        SelectedWidget = null;
        UpdatePreview(); // Update preview when widget is deleted
    }
    
    [RelayCommand]
    private void CopyWidget()
    {
        if (SelectedWidget == null) return;
        _clipboardWidget = SelectedWidget;
    }
    
    [RelayCommand]
    private void PasteWidget()
    {
        if (_clipboardWidget == null) return;
        
        SaveStateForUndo();
        
        var newWidget = new PlacedWidgetItem
        {
            Name = _clipboardWidget.Name,
            DataType = _clipboardWidget.DataType,
            Unit = _clipboardWidget.Unit,
            Kind = _clipboardWidget.Kind,
            X = _clipboardWidget.X + 20,
            Y = _clipboardWidget.Y + 20,
            Z = PlacedWidgets.Count + 1,
            FontSize = _clipboardWidget.FontSize,
            FontFamily = _clipboardWidget.FontFamily,
            Color = _clipboardWidget.Color,
            Width = _clipboardWidget.Width,
            Height = _clipboardWidget.Height,
            BarWidth = _clipboardWidget.BarWidth,
            BarHeight = _clipboardWidget.BarHeight,
            Fill = _clipboardWidget.Fill,
            BackColor = _clipboardWidget.BackColor,
            CornerRadius = _clipboardWidget.CornerRadius,
            MaxNum = _clipboardWidget.MaxNum,
            IsSelected = false
        };
        
        PlacedWidgets.Add(newWidget);
        SelectWidget(newWidget);
        UpdatePreview(); // Update preview when widget is pasted
    }
    
    [RelayCommand]
    private void DuplicateWidget()
    {
        if (SelectedWidget == null) return;
        _clipboardWidget = SelectedWidget;
        PasteWidget();
    }
    
    [RelayCommand]
    private void BringToFront()
    {
        if (SelectedWidget == null) return;
        SaveStateForUndo();
        
        // Find max Z
        int maxZ = PlacedWidgets.Count > 0 ? PlacedWidgets.Max(w => w.Z) : 0;
        SelectedWidget.Z = maxZ + 1;
        
        // Renumber all Z values
        NormalizeZOrder();
    }
    
    [RelayCommand]
    private void SendToBack()
    {
        if (SelectedWidget == null) return;
        SaveStateForUndo();
        
        // Set to 0, then normalize
        SelectedWidget.Z = 0;
        NormalizeZOrder();
    }
    
    [RelayCommand]
    private void BringForward()
    {
        if (SelectedWidget == null) return;
        SaveStateForUndo();
        
        // Find widget directly above
        var currentZ = SelectedWidget.Z;
        var widgetAbove = PlacedWidgets
            .Where(w => w.Z > currentZ)
            .OrderBy(w => w.Z)
            .FirstOrDefault();
        
        if (widgetAbove != null)
        {
            // Swap Z values
            var tempZ = widgetAbove.Z;
            widgetAbove.Z = currentZ;
            SelectedWidget.Z = tempZ;
        }
    }
    
    [RelayCommand]
    private void SendBackward()
    {
        if (SelectedWidget == null) return;
        SaveStateForUndo();
        
        // Find widget directly below
        var currentZ = SelectedWidget.Z;
        var widgetBelow = PlacedWidgets
            .Where(w => w.Z < currentZ)
            .OrderByDescending(w => w.Z)
            .FirstOrDefault();
        
        if (widgetBelow != null)
        {
            // Swap Z values
            var tempZ = widgetBelow.Z;
            widgetBelow.Z = currentZ;
            SelectedWidget.Z = tempZ;
        }
    }
    
    private void NormalizeZOrder()
    {
        var orderedWidgets = PlacedWidgets.OrderBy(w => w.Z).ToList();
        for (int i = 0; i < orderedWidgets.Count; i++)
        {
            orderedWidgets[i].Z = i + 1;
        }
    }
    
    /// <summary>
    /// Snap a coordinate to the grid if enabled
    /// </summary>
    public int SnapToGridValue(int value)
    {
        if (!SnapToGrid || GridSize <= 0) return value;
        return (int)(Math.Round((double)value / GridSize) * GridSize);
    }
    
    [RelayCommand]
    private async Task LoadBackground()
    {
        var dialog = new OpenFileDialog
        {
            Filter = "Todos los fondos|*.png;*.jpg;*.jpeg;*.gif;*.mp4;*.avi;*.wmv|Imagenes|*.png;*.jpg;*.jpeg|GIFs|*.gif|Videos|*.mp4;*.avi;*.wmv",
            Title = "Seleccionar fondo (imagen, GIF o video)"
        };
        
        if (dialog.ShowDialog() == true)
        {
            SaveStateForUndo();
            
            // Clear all background states first
            BackgroundImage = null;
            VideoSource = null;
            GifSourcePath = null;
            IsVideoBackground = false;
            IsGifBackground = false;
            
            BackgroundPath = dialog.FileName;
            var ext = Path.GetExtension(dialog.FileName).ToLowerInvariant();
            
            if (ext == ".mp4" || ext == ".avi" || ext == ".wmv")
            {
                // Convert video to GIF using FFmpeg
                try
                {
                    System.Windows.Input.Mouse.OverrideCursor = System.Windows.Input.Cursors.Wait;
                    
                    var animService = new Services.AnimationService();
                    System.Diagnostics.Debug.WriteLine($"[VIDEO] Converting to GIF: {dialog.FileName}");
                    
                    var gifPath = await Task.Run(() => animService.ConvertVideoToGif(dialog.FileName, fps: 15, width: ThemeWidth));
                    
                    System.Windows.Input.Mouse.OverrideCursor = null;
                    
                    if (System.IO.File.Exists(gifPath))
                    {
                        IsGifBackground = true;
                        GifSourcePath = gifPath;
                        System.Diagnostics.Debug.WriteLine($"[VIDEO→GIF] Converted: {gifPath}");
                    }
                    else
                    {
                        throw new Exception("El archivo GIF no se generó correctamente");
                    }
                }
                catch (Exception ex)
                {
                    System.Windows.Input.Mouse.OverrideCursor = null;
                    MessageBox.Show(
                        $"Error al convertir video a GIF:\n{ex.Message}\n\n" +
                        "Alternativa: Convierte manualmente usando ezgif.com",
                        "Error de Conversión",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    BackgroundPath = "";
                }
            }
            else if (ext == ".gif")
            {
                // GIFs use XamlAnimatedGif
                IsGifBackground = true;
                GifSourcePath = dialog.FileName;
            }
            else
            {
                // Static images (PNG, JPG)
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.UriSource = new Uri(dialog.FileName);
                    bitmap.EndInit();
                    BackgroundImage = bitmap;
                }
                catch { }
            }
        }
    }
    
    [RelayCommand]
    private void ClearCanvas()
    {
        SaveStateForUndo();
        
        // Clear widgets
        PlacedWidgets.Clear();
        SelectedWidget = null;
        
        // Clear backgrounds
        BackgroundImage = null;
        BackgroundPath = "";
        VideoSource = null;
        GifSourcePath = null;
        IsVideoBackground = false;
        IsGifBackground = false;
        
        // Reset theme name
        ThemeName = "NuevoTema";
        
        UpdatePreview();
    }
    
    [RelayCommand]
    private void ZoomIn()
    {
        if (ZoomLevel < 200)
            ZoomLevel += 10;
    }
    
    [RelayCommand]
    private void ZoomOut()
    {
        if (ZoomLevel > 20)
            ZoomLevel -= 10;
    }
    
    [RelayCommand]
    private void ZoomReset()
    {
        ZoomLevel = 50;
    }
    
    public void SelectWidget(PlacedWidgetItem widget, bool addToSelection = false)
    {
        if (addToSelection)
        {
            // Toggle selection for Ctrl+Click
            if (widget.IsSelected)
            {
                widget.IsSelected = false;
                SelectedWidgets.Remove(widget);
                SelectedWidget = SelectedWidgets.LastOrDefault();
            }
            else
            {
                widget.IsSelected = true;
                SelectedWidgets.Add(widget);
                SelectedWidget = widget;
            }
        }
        else
        {
            // Single selection - clear others
            foreach (var w in PlacedWidgets)
                w.IsSelected = false;
            SelectedWidgets.Clear();
            
            widget.IsSelected = true;
            SelectedWidgets.Add(widget);
            SelectedWidget = widget;
        }
        
        OnPropertyChanged(nameof(HasMultipleSelection));
        OnPropertyChanged(nameof(SelectionInfo));
    }
    
    public void DeselectAll()
    {
        foreach (var w in PlacedWidgets)
            w.IsSelected = false;
        SelectedWidgets.Clear();
        SelectedWidget = null;
        
        OnPropertyChanged(nameof(HasMultipleSelection));
        OnPropertyChanged(nameof(SelectionInfo));
    }
    
    [RelayCommand]
    private void SelectAll()
    {
        SelectedWidgets.Clear();
        foreach (var w in PlacedWidgets)
        {
            w.IsSelected = true;
            SelectedWidgets.Add(w);
        }
        SelectedWidget = SelectedWidgets.LastOrDefault();
        
        OnPropertyChanged(nameof(HasMultipleSelection));
        OnPropertyChanged(nameof(SelectionInfo));
    }
    
    [RelayCommand]
    private void DeleteSelected()
    {
        if (SelectedWidgets.Count == 0) return;
        
        SaveStateForUndo();
        
        var toRemove = SelectedWidgets.ToList();
        foreach (var w in toRemove)
        {
            PlacedWidgets.Remove(w);
        }
        SelectedWidgets.Clear();
        SelectedWidget = null;
        
        OnPropertyChanged(nameof(HasMultipleSelection));
        OnPropertyChanged(nameof(SelectionInfo));
    }
    
    [RelayCommand]
    private void AlignSelectedLeft()
    {
        if (SelectedWidgets.Count < 2) return;
        
        SaveStateForUndo();
        var minX = SelectedWidgets.Min(w => w.X);
        foreach (var w in SelectedWidgets)
            w.X = minX;
    }
    
    [RelayCommand]
    private void AlignSelectedTop()
    {
        if (SelectedWidgets.Count < 2) return;
        
        SaveStateForUndo();
        var minY = SelectedWidgets.Min(w => w.Y);
        foreach (var w in SelectedWidgets)
            w.Y = minY;
    }
    
    [RelayCommand]
    private void AlignSelectedRight()
    {
        if (SelectedWidgets.Count < 2) return;
        
        SaveStateForUndo();
        var maxX = SelectedWidgets.Max(w => w.X);
        foreach (var w in SelectedWidgets)
            w.X = maxX;
    }
    
    [RelayCommand]
    private void AlignSelectedBottom()
    {
        if (SelectedWidgets.Count < 2) return;
        
        SaveStateForUndo();
        var maxY = SelectedWidgets.Max(w => w.Y);
        foreach (var w in SelectedWidgets)
            w.Y = maxY;
    }
    
    [RelayCommand]
    private void DistributeHorizontally()
    {
        if (SelectedWidgets.Count < 3) return;
        
        SaveStateForUndo();
        var ordered = SelectedWidgets.OrderBy(w => w.X).ToList();
        var minX = ordered.First().X;
        var maxX = ordered.Last().X;
        var step = (maxX - minX) / (ordered.Count - 1);
        
        for (int i = 0; i < ordered.Count; i++)
            ordered[i].X = minX + (i * step);
    }
    
    [RelayCommand]
    private void DistributeVertically()
    {
        if (SelectedWidgets.Count < 3) return;
        
        SaveStateForUndo();
        var ordered = SelectedWidgets.OrderBy(w => w.Y).ToList();
        var minY = ordered.First().Y;
        var maxY = ordered.Last().Y;
        var step = (maxY - minY) / (ordered.Count - 1);
        
        for (int i = 0; i < ordered.Count; i++)
            ordered[i].Y = minY + (i * step);
    }
    
    [RelayCommand]
    private void SaveTheme()
    {
        var request = new ThemeSaveRequest
        {
            ThemeName = ThemeName,
            Width = ThemeWidth,
            Height = ThemeHeight,
            ThemeType = 1,
            BackgroundPath = BackgroundPath,
            Widgets = PlacedWidgets.Select(w => new WidgetInfo
            {
                Name = w.Name,
                Type = w.DataType,
                Unit = w.Unit,
                X = w.X,
                Y = w.Y,
                FontSize = w.FontSize,
                Font = w.FontFamily,
                Color = w.Color,
                WidgetType = (Services.WidgetType)(int)w.Kind,
                BarWidth = w.BarWidth,
                BarHeight = w.BarHeight,
                MaxNum = w.MaxNum
            }).ToList()
        };
        
        var result = _themeCreatorService.SaveTheme(request);
        
        if (result.Success)
        {
            System.Windows.MessageBox.Show(result.Message, "Tema Guardado",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        else
        {
            System.Windows.MessageBox.Show(result.Error, "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }
    
    [RelayCommand]
    private async Task ExportToPhoto()
    {
        if (!Is7ZipAvailable)
        {
            System.Windows.MessageBox.Show(
                "7-Zip no está instalado.\n\nDescárgalo de: https://7-zip.org",
                "7-Zip Requerido",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return;
        }
        
        // First save the theme
        var request = new ThemeSaveRequest
        {
            ThemeName = ThemeName,
            Width = ThemeWidth,
            Height = ThemeHeight,
            ThemeType = 1,
            BackgroundPath = BackgroundPath,
            Widgets = PlacedWidgets.Select(w => new WidgetInfo
            {
                Name = w.Name,
                Type = w.DataType,
                Unit = w.Unit,
                X = w.X,
                Y = w.Y,
                FontSize = w.FontSize,
                Font = w.FontFamily,
                Color = w.Color,
                WidgetType = (Services.WidgetType)(int)w.Kind,
                BarWidth = w.BarWidth,
                BarHeight = w.BarHeight,
                MaxNum = w.MaxNum
            }).ToList()
        };
        
        var saveResult = _themeCreatorService.SaveTheme(request);
        
        if (!saveResult.Success)
        {
            System.Windows.MessageBox.Show(saveResult.Error, "Error al guardar",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            return;
        }
        
        // Ask where to save the .photo file
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Exportar tema .photo",
            Filter = "Archivo Photo|*.photo",
            FileName = $"{ThemeName}.photo",
            DefaultExt = ".photo",
            InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "ThemesPhoto")
        };
        
        if (dialog.ShowDialog() != true) return;
        
        // Create the .photo file
        var photoResult = await _extractionService.CreatePhotoAsync(saveResult.OutputFolder!, dialog.FileName);
        
        if (photoResult.Success)
        {
            System.Windows.MessageBox.Show(
                $"Tema exportado exitosamente!\n\n" +
                $"📦 Archivo: {Path.GetFileName(photoResult.OutputPath)}\n" +
                $"📏 Tamaño: {photoResult.FileSizeFormatted}\n" +
                $"📁 Ubicación: {Path.GetDirectoryName(photoResult.OutputPath)}",
                "Exportación Completada",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
        else
        {
            System.Windows.MessageBox.Show(
                $"Error al exportar: {photoResult.Error}",
                "Error de Exportación",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }
    
    [RelayCommand]
    private async Task ExportToSmtheme()
    {
        // First save the theme to a temp folder
        var tempFolder = Path.Combine(Path.GetTempPath(), $"smtheme_{Guid.NewGuid():N}");
        Directory.CreateDirectory(tempFolder);
        
        try
        {
            // Copy background
            if (!string.IsNullOrEmpty(BackgroundPath) && File.Exists(BackgroundPath))
            {
                File.Copy(BackgroundPath, Path.Combine(tempFolder, "background.png"), true);
            }
            
            // Generate preview (use background resized as preview)
            var previewPath = Path.Combine(tempFolder, "preview.png");
            if (!string.IsNullOrEmpty(BackgroundPath) && File.Exists(BackgroundPath))
            {
                // Simple copy as preview (SmthemePackagerService will handle it during pack)
                File.Copy(BackgroundPath, previewPath, true);
            }
            
            // Create manifest
            var manifest = new SmthemePackagerService.ThemeManifest
            {
                Name = ThemeName,
                Version = "1.0",
                Author = Environment.UserName,
                Resolution = $"{ThemeWidth}x{ThemeHeight}",
                Animated = false,
                FrameCount = 0,
                Source = "editor",
                Created = DateTime.Now.ToString("yyyy-MM-dd"),
                Description = $"Tema creado con SnakeMarsTheme Editor"
            };
            
            // Ask where to save
            var dialog = new SaveFileDialog
            {
                Title = "Exportar tema .smtheme",
                Filter = "Archivo SnakeMars Theme|*.smtheme",
                FileName = $"{ThemeName}.smtheme",
                DefaultExt = ".smtheme",
                InitialDirectory = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "Themes_SMTHEME")
            };
            
            if (dialog.ShowDialog() != true) return;
            
            // Pack the theme
            await Task.Run(() => _smthemePackagerService.PackTheme(tempFolder, dialog.FileName, manifest));
            
            var fileInfo = new FileInfo(dialog.FileName);
            MessageBox.Show(
                $"Tema exportado exitosamente!\n\n" +
                $"📦 Archivo: {Path.GetFileName(dialog.FileName)}\n" +
                $"📏 Tamaño: {fileInfo.Length / 1024} KB\n" +
                $"📁 Ubicación: {Path.GetDirectoryName(dialog.FileName)}",
                "Exportación Completada",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error al exportar: {ex.Message}",
                "Error de Exportación",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
        finally
        {
            // Cleanup temp folder
            try { Directory.Delete(tempFolder, true); } catch { }
        }
    }
    
    [RelayCommand]
    private async Task ImportSmtheme()
    {
        var initialDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "Themes_SMTHEME");
        if (!Directory.Exists(initialDir))
        {
             // Fallback to generic resources or base dir
             initialDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources");
        }

        var dialog = new OpenFileDialog
        {
            Title = "Importar tema .smtheme",
            Filter = "Archivos SnakeMars Theme|*.smtheme|Todos los archivos|*.*",
            DefaultExt = ".smtheme",
            InitialDirectory = Directory.Exists(initialDir) ? initialDir : AppDomain.CurrentDomain.BaseDirectory
        };
        
        if (dialog.ShowDialog() != true) return;
        
        try
        {
            // Validate structure first
            if (!_smthemePackagerService.ValidateSmtheme(dialog.FileName, out var errors))
            {
                var criticalErrors = errors.Where(e => !e.Contains("se generará")).ToList();
                if (criticalErrors.Any())
                {
                    MessageBox.Show(
                        $"El archivo .smtheme tiene problemas:\n\n• " + 
                        string.Join("\n• ", criticalErrors),
                        "Error de Validación",
                        MessageBoxButton.OK,
                        MessageBoxImage.Warning);
                    return;
                }
            }
            
            // Read manifest to show info
            var manifest = _smthemePackagerService.ReadManifest(dialog.FileName);
            
            // Confirm import
            var confirm = MessageBox.Show(
                $"¿Importar este tema?\n\n" +
                $"📁 Nombre: {manifest.Name}\n" +
                $"📐 Resolución: {manifest.Resolution}\n" +
                $"👤 Autor: {manifest.Author}\n" +
                $"📅 Creado: {manifest.Created}",
                "Importar Tema",
                MessageBoxButton.YesNo,
                MessageBoxImage.Question);
            
            if (confirm != MessageBoxResult.Yes) return;
            
            // Extract to temp folder
            var tempFolder = Path.Combine(Path.GetTempPath(), $"smtheme_import_{Guid.NewGuid():N}");
            await Task.Run(() => _smthemePackagerService.UnpackTheme(dialog.FileName, tempFolder));
            
            // Try to load settings.txt if exists (for full widget import)
            var settingPath = FindSettingFile(tempFolder);
            if (settingPath != null)
            {
                try
                {
                    var parser = new SettingParser();
                    var theme = parser.Parse(settingPath);
                    LoadThemeIntoEditor(theme, Path.GetDirectoryName(settingPath)!);
                    
                    MessageBox.Show(
                        $"Tema importado con widgets!\n\n" +
                        $"📁 {theme.Name}\n" +
                        $"📐 {ThemeWidth}x{ThemeHeight}\n" +
                        $"📝 {theme.Texts.Count} textos\n" +
                        $"📊 {theme.Bars.Count} barras",
                        "Importación Completada",
                        MessageBoxButton.OK,
                        MessageBoxImage.Information);
                    return;
                }
                catch
                {
                    // Settings.txt parsing failed, fall back to background only
                }
            }
            
            // Fallback: Load background only (no settings.txt or parse failed)
            var bgPath = Path.Combine(tempFolder, "background.png");
            if (File.Exists(bgPath))
            {
                BackgroundPath = bgPath;
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(bgPath);
                bitmap.EndInit();
                BackgroundImage = bitmap;
            }
            
            // Update theme properties from manifest
            ThemeName = manifest.Name ?? "ImportedTheme";
            if (!string.IsNullOrEmpty(manifest.Resolution) && manifest.Resolution.Contains("x"))
            {
                var parts = manifest.Resolution.Split('x');
                if (int.TryParse(parts[0], out int w) && int.TryParse(parts[1], out int h))
                {
                    ThemeWidth = w;
                    ThemeHeight = h;
                }
            }
            
            MessageBox.Show(
                $"Tema importado (solo fondo)\n\n" +
                $"📁 {manifest.Name}\n" +
                $"📐 {ThemeWidth}x{ThemeHeight}\n\n" +
                $"No se encontró settings.txt - puedes añadir widgets manualmente.",
                "Importación Completada",
                MessageBoxButton.OK,
                MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            MessageBox.Show(
                $"Error al importar: {ex.Message}",
                "Error de Importación",
                MessageBoxButton.OK,
                MessageBoxImage.Error);
        }
    }
    
    [RelayCommand]
    private async Task ImportPhotoTheme()
    {
        if (!Is7ZipAvailable)
        {
            System.Windows.MessageBox.Show(
                "7-Zip no está instalado.\n\nDescárgalo de: https://7-zip.org",
                "7-Zip Requerido",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return;
        }
        
        var initialDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources", "ThemesPhoto");
        if (!Directory.Exists(initialDir))
        {
             initialDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "resources");
        }

        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Importar tema .photo",
            Filter = "Archivos Photo|*.photo|Todos los archivos|*.*",
            DefaultExt = ".photo",
            InitialDirectory = Directory.Exists(initialDir) ? initialDir : AppDomain.CurrentDomain.BaseDirectory
        };
        
        if (dialog.ShowDialog() != true) return;
        
        // Extract the .photo file
        var extractResult = await _extractionService.ExtractPhotoAsync(dialog.FileName);
        
        if (!extractResult.Success)
        {
            System.Windows.MessageBox.Show(
                $"Error al extraer: {extractResult.Error}",
                "Error de Extracción",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
            return;
        }
        
        // Find Setting.txt in extracted folder
        var settingPath = FindSettingFile(extractResult.ExtractedPath!);
        if (settingPath == null)
        {
            System.Windows.MessageBox.Show(
                "No se encontró Setting.txt en el tema.",
                "Archivo no encontrado",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Warning);
            return;
        }
        
        try
        {
            // Parse the Setting.txt
            var parser = new SettingParser();
            var theme = parser.Parse(settingPath);
            
            // Load into editor
            LoadThemeIntoEditor(theme, Path.GetDirectoryName(settingPath)!);
            
            System.Windows.MessageBox.Show(
                $"Tema importado exitosamente!\n\n" +
                $"📁 {theme.Name}\n" +
                $"📐 {theme.Width}x{theme.Height}\n" +
                $"📝 {theme.Texts.Count} textos\n" +
                $"📊 {theme.Bars.Count} barras",
                "Importación Completada",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Error al parsear tema: {ex.Message}",
                "Error de Parseo",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }
    
    private string? FindSettingFile(string extractedPath)
    {
        // Direct Setting.txt
        var direct = Path.Combine(extractedPath, "Setting.txt");
        if (File.Exists(direct)) return direct;
        
        // In subfolder
        foreach (var dir in Directory.GetDirectories(extractedPath))
        {
            var subSetting = Path.Combine(dir, "Setting.txt");
            if (File.Exists(subSetting)) return subSetting;
        }
        
        return null;
    }
    
    private void LoadThemeIntoEditor(Theme theme, string themeFolderPath)
    {
        // Clear current state
        PlacedWidgets.Clear();
        SelectedWidgets.Clear();
        SelectedWidget = null;
        _undoStack.Clear();
        _redoStack.Clear();
        
        // Set basic properties
        ThemeName = theme.Name ?? Path.GetFileName(themeFolderPath);
        ThemeWidth = theme.Width;
        ThemeHeight = theme.Height;
        
        // Try to load background
        var backPath = Path.Combine(themeFolderPath, "back.png");
        if (File.Exists(backPath))
        {
            BackgroundPath = backPath;
            IsVideoBackground = false;
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(backPath);
                bitmap.EndInit();
                BackgroundImage = bitmap;
            }
            catch { BackgroundImage = null; }
        }
        
        // Load text widgets
        int zIndex = 1;
        foreach (var text in theme.Texts)
        {
            PlacedWidgets.Add(new PlacedWidgetItem
            {
                Name = text.Title ?? text.Data ?? "Texto",
                DataType = text.Data ?? "Static",
                Unit = text.Unit ?? "",
                Kind = WidgetKind.Text,
                X = (int)text.X,
                Y = (int)text.Y,
                Z = text.Z > 0 ? text.Z : zIndex++,
                FontSize = text.FontSize,
                FontFamily = text.FontFamily ?? "Impact",
                Color = text.Foreground ?? "#FFFFFF",
                IsSelected = false
            });
        }
        
        // Load bar widgets
        foreach (var bar in theme.Bars)
        {
            var kind = bar.Type switch
            {
                "BorderLine" => WidgetKind.BorderLine,
                "GridLine" => WidgetKind.GridLine,
                _ => WidgetKind.DefaultLine
            };
            
            PlacedWidgets.Add(new PlacedWidgetItem
            {
                Name = $"[{bar.Type}] {bar.Data ?? "Bar"}",
                DataType = bar.Data ?? "Static",
                Unit = "",
                Kind = kind,
                X = (int)bar.X,
                Y = (int)bar.Y,
                Z = bar.Z > 0 ? bar.Z : zIndex++,
                BarWidth = bar.MaxWidth,
                BarHeight = bar.MaxHeight,
                Fill = bar.Fill ?? "#00FF00",
                BackColor = bar.BackColor ?? "#333333",
                CornerRadius = ParseCornerRadius(bar.CornerRadius),
                MaxNum = bar.MaxNum,
                IsSelected = false
            });
        }
        
        // Update UI
        OnPropertyChanged(nameof(CanvasWidth));
        OnPropertyChanged(nameof(CanvasHeight));
        OnPropertyChanged(nameof(CanvasResolution));
        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
        OnPropertyChanged(nameof(UndoStatus));
        OnPropertyChanged(nameof(HasMultipleSelection));
        OnPropertyChanged(nameof(SelectionInfo));
    }
    
    private int ParseCornerRadius(string? cornerRadius)
    {
        if (string.IsNullOrEmpty(cornerRadius)) return 0;
        var parts = cornerRadius.Split(',');
        if (parts.Length > 0 && int.TryParse(parts[0].Trim(), out int r))
            return r;
        return 0;
    }
    
    // ============== Project Save/Load ==============
    
    private ThemeProject CreateProjectFromCurrentState()
    {
        return new ThemeProject
        {
            ThemeName = ThemeName,
            Width = ThemeWidth,
            Height = ThemeHeight,
            BackgroundPath = BackgroundPath,
            IsVideoBackground = IsVideoBackground,
            Widgets = PlacedWidgets.Select(w => new ProjectWidget
            {
                Name = w.Name,
                DataType = w.DataType,
                Unit = w.Unit,
                Kind = w.Kind,
                X = w.X,
                Y = w.Y,
                Z = w.Z,
                FontSize = w.FontSize,
                FontFamily = w.FontFamily,
                Color = w.Color,
                Width = w.Width,
                Height = w.Height,
                BarWidth = w.BarWidth,
                BarHeight = w.BarHeight,
                MaxNum = w.MaxNum,
                CornerRadius = w.CornerRadius,
                Fill = w.Fill,
                BackColor = w.BackColor
            }).ToList()
        };
    }
    
    private void LoadProjectIntoEditor(ThemeProject project)
    {
        ThemeName = project.ThemeName;
        ThemeWidth = project.Width;
        ThemeHeight = project.Height;
        BackgroundPath = project.BackgroundPath ?? "";
        IsVideoBackground = project.IsVideoBackground;
        
        // Load background based on type
        if (!string.IsNullOrEmpty(BackgroundPath) && File.Exists(BackgroundPath))
        {
            if (IsVideoBackground)
            {
                VideoSource = new Uri(BackgroundPath);
                BackgroundImage = null;
            }
            else
            {
                VideoSource = null;
                try
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(BackgroundPath);
                    bitmap.CacheOption = BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    BackgroundImage = bitmap;
                }
                catch { BackgroundImage = null; }
            }
        }
        else
        {
            VideoSource = null;
            BackgroundImage = null;
        }
        
        // Load widgets
        PlacedWidgets.Clear();
        foreach (var pw in project.Widgets)
        {
            PlacedWidgets.Add(new PlacedWidgetItem
            {
                Name = pw.Name,
                DataType = pw.DataType,
                Unit = pw.Unit,
                Kind = pw.Kind,
                X = pw.X,
                Y = pw.Y,
                Z = pw.Z,
                FontSize = pw.FontSize,
                FontFamily = pw.FontFamily,
                Color = pw.Color,
                Width = pw.Width,
                Height = pw.Height,
                BarWidth = pw.BarWidth,
                BarHeight = pw.BarHeight,
                MaxNum = pw.MaxNum,
                CornerRadius = pw.CornerRadius,
                Fill = pw.Fill,
                BackColor = pw.BackColor,
                IsSelected = false
            });
        }
        
        SelectedWidget = null;
        _undoStack.Clear();
        _redoStack.Clear();
        
        OnPropertyChanged(nameof(CanvasWidth));
        OnPropertyChanged(nameof(CanvasHeight));
        OnPropertyChanged(nameof(CanvasResolution));
        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
        OnPropertyChanged(nameof(UndoStatus));
        OnPropertyChanged(nameof(ProjectStatus));
    }
    
    [RelayCommand]
    private void SaveProject()
    {
        var dialog = new Microsoft.Win32.SaveFileDialog
        {
            Title = "Guardar Proyecto",
            Filter = ProjectService.ProjectFilter,
            FileName = $"{ThemeName}{ProjectService.ProjectExtension}",
            DefaultExt = ProjectService.ProjectExtension,
            InitialDirectory = _projectService.GetProjectsFolder()
        };
        
        if (!string.IsNullOrEmpty(_currentProjectPath))
        {
            dialog.InitialDirectory = Path.GetDirectoryName(_currentProjectPath);
            dialog.FileName = Path.GetFileName(_currentProjectPath);
        }
        
        if (dialog.ShowDialog() != true) return;
        
        var project = CreateProjectFromCurrentState();
        var result = _projectService.SaveProject(project, dialog.FileName);
        
        if (result.Success)
        {
            _currentProjectPath = result.FilePath;
            OnPropertyChanged(nameof(HasProject));
            OnPropertyChanged(nameof(ProjectStatus));
            
            System.Windows.MessageBox.Show(
                $"Proyecto guardado!\n\n📁 {Path.GetFileName(result.FilePath)}",
                "Proyecto Guardado",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
        else
        {
            System.Windows.MessageBox.Show(result.Error, "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }
    
    [RelayCommand]
    private void QuickSaveProject()
    {
        if (string.IsNullOrEmpty(_currentProjectPath))
        {
            SaveProject();
            return;
        }
        
        var project = CreateProjectFromCurrentState();
        var result = _projectService.SaveProject(project, _currentProjectPath);
        
        if (!result.Success)
        {
            System.Windows.MessageBox.Show(result.Error, "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }
    
    [RelayCommand]
    private void LoadProject()
    {
        // Ask to save current work if there are widgets
        if (PlacedWidgets.Count > 0)
        {
            var confirm = System.Windows.MessageBox.Show(
                "¿Deseas guardar el proyecto actual antes de cargar otro?",
                "Guardar Cambios",
                System.Windows.MessageBoxButton.YesNoCancel,
                System.Windows.MessageBoxImage.Question);
            
            if (confirm == System.Windows.MessageBoxResult.Cancel) return;
            if (confirm == System.Windows.MessageBoxResult.Yes) SaveProject();
        }
        
        var dialog = new Microsoft.Win32.OpenFileDialog
        {
            Title = "Cargar Proyecto",
            Filter = ProjectService.ProjectFilter,
            InitialDirectory = _projectService.GetProjectsFolder()
        };
        
        if (dialog.ShowDialog() != true) return;
        
        var result = _projectService.LoadProject(dialog.FileName);
        
        if (result.Success && result.Project != null)
        {
            _currentProjectPath = result.FilePath;
            LoadProjectIntoEditor(result.Project);
            
            System.Windows.MessageBox.Show(
                $"Proyecto cargado!\n\n" +
                $"📁 {result.Project.ThemeName}\n" +
                $"📐 {result.Project.Width}x{result.Project.Height}\n" +
                $"📦 {result.Project.Widgets.Count} widgets",
                "Proyecto Cargado",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
        else
        {
            System.Windows.MessageBox.Show(result.Error, "Error",
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }
    
    [RelayCommand]
    private void NewProject()
    {
        // Ask to save current work if there are widgets
        if (PlacedWidgets.Count > 0)
        {
            var confirm = System.Windows.MessageBox.Show(
                "¿Deseas guardar el proyecto actual antes de crear uno nuevo?",
                "Guardar Cambios",
                System.Windows.MessageBoxButton.YesNoCancel,
                System.Windows.MessageBoxImage.Question);
            
            if (confirm == System.Windows.MessageBoxResult.Cancel) return;
            if (confirm == System.Windows.MessageBoxResult.Yes) SaveProject();
        }
        
        // Reset to defaults
        ThemeName = "NuevoTema";
        ThemeWidth = 360;
        ThemeHeight = 960;
        BackgroundPath = "";
        BackgroundImage = null;
        IsVideoBackground = false;
        VideoSource = null;
        PlacedWidgets.Clear();
        SelectedWidget = null;
        _currentProjectPath = null;
        _undoStack.Clear();
        _redoStack.Clear();
        
        OnPropertyChanged(nameof(CanvasWidth));
        OnPropertyChanged(nameof(CanvasHeight));
        OnPropertyChanged(nameof(CanvasResolution));
        OnPropertyChanged(nameof(CanUndo));
        OnPropertyChanged(nameof(CanRedo));
        OnPropertyChanged(nameof(UndoStatus));
        OnPropertyChanged(nameof(HasProject));
        OnPropertyChanged(nameof(ProjectStatus));
        
        UpdatePreview(); // Update preview when creating new project
    }
    
    // ============== Setting Preview and Validation ==============
    
    /// <summary>
    /// Updates the Setting.txt preview and runs validation.
    /// Called automatically when widgets change.
    /// </summary>
    public void UpdatePreview()
    {
        try
        {
            // Create ThemeProject from current state
            var project = new ThemeProject
            {
                ThemeName = ThemeName,
                Width = ThemeWidth,
                Height = ThemeHeight,
                BackgroundPath = BackgroundPath,
                Widgets = PlacedWidgets.Select(w => new ProjectWidget
                {
                    Name = w.Name,
                    DataType = w.DataType,
                    Unit = w.Unit,
                    Kind = w.Kind,
                    X = w.X,
                    Y = w.Y,
                    Z = w.Z,
                    FontSize = w.FontSize,
                    FontFamily = w.FontFamily,
                    Color = w.Color,
                    Width = w.Width,
                    Height = w.Height,
                    BarWidth = w.BarWidth,
                    BarHeight = w.BarHeight,
                    MaxNum = w.MaxNum,
                    CornerRadius = w.CornerRadius,
                    Fill = w.Fill,
                    BackColor = w.BackColor
                }).ToList()
            };
            
            // Generate preview text
            SettingPreviewText = _settingPreviewService.GenerateSettingPreview(project);
            
            // Run validation
            var errors = _settingPreviewService.ValidateSetting(project);
            ValidationErrors.Clear();
            foreach (var error in errors)
            {
                ValidationErrors.Add(error);
            }
        }
        catch (Exception ex)
        {
            SettingPreviewText = $"// Error al generar preview:\n// {ex.Message}";
            ValidationErrors.Clear();
            ValidationErrors.Add(new ValidationMessage
            {
                Severity = ValidationSeverity.Error,
                Message = $"Error al generar preview: {ex.Message}"
            });
        }
    }
    
    /// <summary>
    /// Copies the Setting.txt preview to clipboard.
    /// </summary>
    [RelayCommand]
    private void CopySettingToClipboard()
    {
        try
        {
            System.Windows.Clipboard.SetText(SettingPreviewText);
            System.Windows.MessageBox.Show(
                "Setting.txt copiado al portapapeles",
                "Copiado",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            System.Windows.MessageBox.Show(
                $"Error al copiar: {ex.Message}",
                "Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
    }
}

public partial class PlacedWidgetItem : ObservableObject
{
    [ObservableProperty] private string _name = "";
    [ObservableProperty] private string _dataType = "";
    [ObservableProperty] private string _unit = "";
    [ObservableProperty] private WidgetKind _kind = WidgetKind.Text;
    [ObservableProperty] private int _x;
    [ObservableProperty] private int _y;
    [ObservableProperty] private int _z;
    [ObservableProperty] private int _fontSize = 24;
    [ObservableProperty] private string _fontFamily = "Impact";
    [ObservableProperty] private string _color = "#FFFFFF";
    [ObservableProperty] private bool _isSelected;
    [ObservableProperty] private int _width = 100;
    [ObservableProperty] private int _height = 20;
    
    // Bar properties
    [ObservableProperty] private int _barWidth = 100;
    [ObservableProperty] private int _barHeight = 10;
    [ObservableProperty] private int _maxNum = 100;
    [ObservableProperty] private int _cornerRadius = 5;
    [ObservableProperty] private string _fill = "#00FF00";
    [ObservableProperty] private string _backColor = "#333333";
    
    // Computed properties for display
    public string DisplayText => string.IsNullOrEmpty(Unit) ? Name : $"00 {Unit}";
    public bool IsTextWidget => Kind == WidgetKind.Text;
    public bool IsBarWidget => Kind != WidgetKind.Text;
    public string BorderColor => IsSelected ? "#00FFFF" : "Transparent";
    
    public SolidColorBrush ForegroundBrush
    {
        get
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(Color);
                return new SolidColorBrush(color);
            }
            catch { return Brushes.White; }
        }
    }
    
    public SolidColorBrush BarFillBrush
    {
        get
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(Fill);
                return new SolidColorBrush(color);
            }
            catch { return Brushes.Green; }
        }
    }
    
    public SolidColorBrush BarBackground
    {
        get
        {
            try
            {
                var color = (Color)ColorConverter.ConvertFromString(BackColor);
                return new SolidColorBrush(color);
            }
            catch { return Brushes.DarkGray; }
        }
    }
    
    public double BarFillWidth => BarWidth * 0.65; // Demo fill at 65%
    public CornerRadius BarCornerRadius => new(CornerRadius);
    
    // Notify computed properties when underlying values change
    partial void OnColorChanged(string value) => OnPropertyChanged(nameof(ForegroundBrush));
    partial void OnFillChanged(string value) => OnPropertyChanged(nameof(BarFillBrush));
    partial void OnBackColorChanged(string value) => OnPropertyChanged(nameof(BarBackground));
    partial void OnIsSelectedChanged(bool value) => OnPropertyChanged(nameof(BorderColor));
    partial void OnBarWidthChanged(int value) => OnPropertyChanged(nameof(BarFillWidth));
    partial void OnCornerRadiusChanged(int value) => OnPropertyChanged(nameof(BarCornerRadius));
}
