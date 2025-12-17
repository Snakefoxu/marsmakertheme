using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using Microsoft.Win32;
using SnakeMarsTheme.Models;
using SnakeMarsTheme.Services;

namespace SnakeMarsTheme.ViewModels;

public partial class WizardViewModel : ObservableObject
{
    private readonly ThemeCreatorService _themeCreatorService;
    private readonly AnimationService _animationService;
    
    // Step tracking
    [ObservableProperty]
    private int _currentStep = 1;
    
    [ObservableProperty]
    private bool _canGoBack;
    
    [ObservableProperty]
    private bool _canGoNext = true;
    
    [ObservableProperty]
    private string _nextButtonText = "Siguiente ->";
    
    // Step 1: Configuration
    [ObservableProperty]
    private string _themeName = "";
    
    [ObservableProperty]
    private Resolution _selectedResolution;
    
    [ObservableProperty]
    private int _customWidth = 360;
    
    [ObservableProperty]
    private int _customHeight = 960;
    
    [ObservableProperty]
    private bool _isSettingTxtType = true;
    
    [ObservableProperty]
    private bool _isCustomResolution;
    
    // Step 2: Background
    [ObservableProperty]
    private string _backgroundPath = "";
    
    [ObservableProperty]
    private BitmapImage? _backgroundPreview;
    
    [ObservableProperty]
    private bool _isStaticBackground = true;
    
    // Animation properties
    [ObservableProperty]
    private string _backgroundType = "Imagen Estática";
    
    [ObservableProperty]
    private int _animationFPS = 10;
    
    [ObservableProperty]
    private int _extractedFramesCount = 0;
    
    [ObservableProperty]
    private List<string> _extractedFramePaths = new();
    
    [ObservableProperty]
    private bool _isExtracting = false;
    
    [ObservableProperty]
    private string _extractionStatus = "";
    
    // Video preview support
    private bool _isVideoBackground;
    public bool IsVideoBackground
    {
        get => _isVideoBackground;
        set => SetProperty(ref _isVideoBackground, value);
    }
    
    private Uri? _videoSource;
    public Uri? VideoSource
    {
        get => _videoSource;
        set => SetProperty(ref _videoSource, value);
    }
    
    // Step 3: Widgets
    [ObservableProperty]
    private ObservableCollection<WidgetItem> _availableWidgets = new();
    
    [ObservableProperty]
    private ObservableCollection<WidgetItem> _selectedWidgets = new();
    
    [ObservableProperty]
    private WidgetItem? _selectedAvailableWidget;
    
    // Selected widget for property panel editing - with auto canvas update
    private WidgetItem? _selectedThemeWidget;
    public WidgetItem? SelectedThemeWidget
    {
        get => _selectedThemeWidget;
        set
        {
            // Unsubscribe from old widget
            if (_selectedThemeWidget != null)
            {
                _selectedThemeWidget.PropertyChanged -= OnSelectedWidgetPropertyChanged;
            }
            
            _selectedThemeWidget = value;
            OnPropertyChanged(nameof(SelectedThemeWidget));
            
            // Subscribe to new widget for auto canvas updates
            if (_selectedThemeWidget != null)
            {
                _selectedThemeWidget.PropertyChanged += OnSelectedWidgetPropertyChanged;
            }
        }
    }
    
    // Auto update canvas when widget properties change
    private void OnSelectedWidgetPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        // Only update preview for relevant properties
        if (e.PropertyName == nameof(WidgetItem.X) || 
            e.PropertyName == nameof(WidgetItem.Y) || 
            e.PropertyName == nameof(WidgetItem.FontSize) || 
            e.PropertyName == nameof(WidgetItem.Font) || 
            e.PropertyName == nameof(WidgetItem.Color))
        {
            UpdateCanvasPreview();
        }
    }
    
    [RelayCommand]
    private void RefreshPreview()
    {
        UpdateCanvasPreview();
    }
    
    [ObservableProperty]
    private string _selectedCategory = "CPU";
    
    // Available options for property panel dropdowns
    public ObservableCollection<string> AvailableFonts { get; } = new()
    {
        "Impact", "Arial", "Segoe UI", "Verdana", "Tahoma", "Consolas", 
        "Courier New", "Georgia", "Times New Roman", "Comic Sans MS"
    };
    
    public ObservableCollection<string> AvailableColors { get; } = new()
    {
        "#FFFFFF", "#00FFFF", "#FF00FF", "#FFFF00", "#00FF00", "#FF0000",
        "#0000FF", "#FFA500", "#FF69B4", "#00CED1", "#CCCCCC", "#666666"
    };
    
    // Step 4: Summary
    [ObservableProperty]
    private string _summary = "";
    
    // Canvas Preview
    [ObservableProperty]
    private BitmapImage? _canvasPreview;
    
    /// <summary>
    /// Determines if horizontal layout should be used (rows) vs vertical/square (columns)
    /// </summary>
    public bool IsHorizontalLayout
    {
        get
        {
            var orientation = SelectedResolution?.Orientation ?? "Vertical";
            return orientation == "Horizontal";
        }
    }
    
    public double PreviewContainerWidth
    {
        get
        {
            var orientation = SelectedResolution?.Orientation ?? "Vertical";
            return orientation switch
            {
                "Horizontal" => 980, // Full width for row layout
                "Vertical" => 500,   // Left column
                _ => 450             // Square/AIO
            };
        }
    }
    
    private string _canvasInfo = "";
    public string CanvasInfo 
    {
        get => _canvasInfo;
        private set
        {
            if (_canvasInfo != value)
            {
                _canvasInfo = value;
                OnPropertyChanged(nameof(CanvasInfo));
            }
        }
    }
    
    private void UpdateCanvasInfo()
    {
        var width = IsCustomResolution ? CustomWidth : SelectedResolution?.Width ?? 360;
        var height = IsCustomResolution ? CustomHeight : SelectedResolution?.Height ?? 960;
        CanvasInfo = $"Resolucion: {width}x{height}\nWidgets: {SelectedWidgets.Count}\nZoom: Scroll Mouse";
    }
    
    // Canvas dimensions for interactive preview
    public int CanvasWidth => IsCustomResolution ? CustomWidth : SelectedResolution?.Width ?? 360;
    public int CanvasHeight => IsCustomResolution ? CustomHeight : SelectedResolution?.Height ?? 960;
    

    // Properties
    public ObservableCollection<Resolution> Resolutions { get; } = new(Resolution.Presets);
    public ObservableCollection<string> Categories { get; } = new() 
    { 
        "CPU", "GPU", "Memoria", "Sistema", "Ventiladores", "Disco", "Red", "Clima", "Etiquetas", "Barras" 
    };
    
    private string _basePath;
    
    public ObservableCollection<string> BackgroundTypes { get; } = new()
    {
        "Imagen Estática",
        "GIF Animado",
        "Video (MP4/AVI)",
        "Secuencia de Frames"
    };

    // Template Support
    [ObservableProperty]
    private ObservableCollection<string> _availableTemplates = new();

    [ObservableProperty]
    private string _selectedTemplate;

    [ObservableProperty]
    private bool _hasTemplates;

    partial void OnSelectedTemplateChanged(string value)
    {
        if (!string.IsNullOrEmpty(value) && _basePath != null)
        {
            // Find full path
            string folder = GetTemplateFolderForCurrentType();
            if (!string.IsNullOrEmpty(folder))
            {
                string fullPath = Path.Combine(folder, value);
                if (File.Exists(fullPath))
                {
                    BackgroundPath = fullPath;
                }
            }
        }
    }

    partial void OnBackgroundTypeChanged(string value)
    {
        LoadTemplates();
    }

    private string GetTemplateFolderForCurrentType()
    {
        if (string.IsNullOrEmpty(_basePath)) return "";

        string resourceFolder = Path.Combine(_basePath, "resources");
        if (!Directory.Exists(resourceFolder)) return "";

        return BackgroundType switch
        {
            "GIF Animado" => Path.Combine(resourceFolder, "GIFs"),
            "Video (MP4/AVI)" => Path.Combine(resourceFolder, "Videos"),
            _ => ""
        };
    }

    private void LoadTemplates()
    {
        AvailableTemplates.Clear();
        SelectedTemplate = null;
        HasTemplates = false;

        string folder = GetTemplateFolderForCurrentType();
        if (string.IsNullOrEmpty(folder) || !Directory.Exists(folder)) return;

        try
        {
            string[] extensions = BackgroundType switch
            {
                "GIF Animado" => new[] { "*.gif" },
                "Video (MP4/AVI)" => new[] { "*.mp4", "*.avi", "*.mov" },
                _ => Array.Empty<string>()
            };

            var files = new List<string>();
            foreach (var ext in extensions)
            {
                files.AddRange(Directory.GetFiles(folder, ext).Select(Path.GetFileName));
            }

            foreach (var file in files.OrderBy(f => f))
            {
                AvailableTemplates.Add(file);
            }

            HasTemplates = AvailableTemplates.Count > 0;
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error loading templates: {ex.Message}");
        }
    }
    
    public WizardViewModel()
    {
        // Get base path - prioritize LOCAL resources folder first (for publish mode)
        var exePath = AppDomain.CurrentDomain.BaseDirectory;
        string basePath;
        
        // PRIORITY 1: Check for resources/ in SAME directory as .exe (publish mode)
        var localResourcesPath = Path.Combine(exePath, "resources");
        if (Directory.Exists(localResourcesPath))
        {
            basePath = exePath;
        }
        else
        {
            // PRIORITY 2: Search UP the directory tree (dev mode)
            var currentDir = new DirectoryInfo(exePath);
            while (currentDir != null)
            {
                var resourcesPath = Path.Combine(currentDir.FullName, "resources");
                if (Directory.Exists(resourcesPath))
                {
                    basePath = currentDir.FullName;
                    break;
                }
                currentDir = currentDir.Parent;
            }
            
            // PRIORITY 3: Fallback to old method
            if (currentDir == null)
            {
                basePath = Path.GetFullPath(Path.Combine(exePath, "..", "..", "..", "..", ".."));
            }
            else
            {
                basePath = currentDir.FullName;
            }
        }
        
        _basePath = basePath;
        
        // FIX: Usar "Mis Documentos/SnakeMarsTheme" para escritura (evitar error de permisos en Program Files)
        var userPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SnakeMarsTheme");
        if (!Directory.Exists(userPath))
        {
            try { Directory.CreateDirectory(userPath); } catch { /* Ignore if fails, service handles dir creation too */ }
        }

        _themeCreatorService = new ThemeCreatorService(userPath);
        _animationService = new AnimationService();
        _selectedResolution = Resolution.Presets[0];
        LoadWidgetCategories();
    }
    
    // Preview Canvas Methods
    public void UpdateCanvasPreview()
    {
        try
        {
            var width = IsCustomResolution ? CustomWidth : SelectedResolution.Width;
            var height = IsCustomResolution ? CustomHeight : SelectedResolution.Height;
            
            // Create bitmap
            using (var bitmap = new Bitmap(width, height))
            {
                using (var graphics = Graphics.FromImage(bitmap))
                {
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                    graphics.Clear(System.Drawing.Color.Black);
                    
                    // Draw background
                    if (!string.IsNullOrEmpty(BackgroundPath) && File.Exists(BackgroundPath))
                    {
                        try 
                        {
                            using (var sourceImage = Image.FromFile(BackgroundPath))
                            {
                                graphics.DrawImage(sourceImage, 0, 0, width, height);
                            }
                        }
                        catch { /* Ignore image load error, keep black back */ }
                    }
                    
                    // Draw widgets - this is the REAL widget as it appears on LCD screen
                    foreach (var widget in SelectedWidgets)
                    {
                        DrawWidget(graphics, widget);
                    }
                }
                
                // Convert to BitmapImage for WPF
                using (var memory = new MemoryStream())
                {
                    bitmap.Save(memory, ImageFormat.Png);
                    memory.Position = 0;
                    
                    var bitmapImage = new BitmapImage();
                    bitmapImage.BeginInit();
                    bitmapImage.StreamSource = memory;
                    bitmapImage.CacheOption = BitmapCacheOption.OnLoad;
                    bitmapImage.EndInit();
                    bitmapImage.Freeze();
                    
                    CanvasPreview = bitmapImage;
                }
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error updating canvas: {ex.Message}");
        }
    }
    
    private void DrawWidget(Graphics graphics, WidgetItem widget)
    {
        try
        {
            var fontSize = Math.Max(10, widget.FontSize);
            var fontName = string.IsNullOrEmpty(widget.Font) ? "Impact" : widget.Font;
            var color = ParseColor(widget.Color);
            
            using (var font = new Font(fontName, fontSize, FontStyle.Bold, GraphicsUnit.Pixel))
            using (var brush = new SolidBrush(color))
            {
                var text = string.IsNullOrEmpty(widget.Unit) 
                    ? widget.Name 
                    : $"00 {widget.Unit}";
                graphics.DrawString(text, font, brush, widget.X, widget.Y);
            }
        }
        catch (Exception ex)
        {
            Debug.WriteLine($"Error drawing widget {widget.Name}: {ex.Message}");
        }
    }
    
    private System.Drawing.Color ParseColor(string hexColor)
    {
        try
        {
            if (string.IsNullOrEmpty(hexColor)) return System.Drawing.Color.White;
            hexColor = hexColor.TrimStart('#');
            if (hexColor.Length == 6)
            {
                return System.Drawing.Color.FromArgb(255,
                    Convert.ToInt32(hexColor.Substring(0, 2), 16),
                    Convert.ToInt32(hexColor.Substring(2, 2), 16),
                    Convert.ToInt32(hexColor.Substring(4, 2), 16));
            }
        }
        catch { }
        return System.Drawing.Color.White;
    }
    
    private void LoadWidgetCategories()
    {
        var allWidgets = new Dictionary<string, List<WidgetItem>>
        {
            ["CPU"] = new()
            {
                new() { Name = "Temperatura CPU", Unit = "°C" },
                new() { Name = "Temp CPU (SOEYI)", Unit = "°C" },
                new() { Name = "Uso CPU", Unit = "%" },
                new() { Name = "Uso CPU (alias)", Unit = "%" },
                new() { Name = "Frecuencia CPU", Unit = "MHz" },
                new() { Name = "Frecuencia CPU (alias)", Unit = "MHz" },
                new() { Name = "Voltaje CPU", Unit = "V" },
                new() { Name = "Voltaje CPU (alias)", Unit = "V" },
                new() { Name = "Potencia CPU", Unit = "W" },
                new() { Name = "TDP CPU", Unit = "W" },
                new() { Name = "Ventilador CPU", Unit = "RPM" },
                new() { Name = "Etiqueta CPU", Unit = "" }
            },
            ["GPU"] = new()
            {
                new() { Name = "Temperatura GPU", Unit = "°C" },
                new() { Name = "Temp GPU (SOEYI)", Unit = "°C" },
                new() { Name = "Uso GPU", Unit = "%" },
                new() { Name = "Uso GPU (alias)", Unit = "%" },
                new() { Name = "Frecuencia GPU", Unit = "MHz" },
                new() { Name = "Frecuencia GPU (alias)", Unit = "MHz" },
                new() { Name = "Memoria GPU", Unit = "MB" },
                new() { Name = "Uso VRAM", Unit = "%" },
                new() { Name = "Potencia GPU", Unit = "W" },
                new() { Name = "TDP GPU", Unit = "W" },
                new() { Name = "Ventilador GPU", Unit = "RPM" },
                new() { Name = "Etiqueta GPU", Unit = "" }
            },
            ["Memoria"] = new()
            {
                new() { Name = "Uso Memoria", Unit = "%" },
                new() { Name = "Uso Memoria (alias)", Unit = "%" },
                new() { Name = "Memoria Usada (GB)", Unit = "GB" },
                new() { Name = "Memoria Usada (alias)", Unit = "GB" },
                new() { Name = "Memoria Total", Unit = "GB" },
                new() { Name = "Frecuencia RAM", Unit = "MHz" },
                new() { Name = "Frecuencia RAM (alias)", Unit = "MHz" },
                new() { Name = "Uso RAM entero", Unit = "" },
                new() { Name = "Etiqueta RAM", Unit = "" },
                new() { Name = "Etiqueta ROM", Unit = "" }
            },
            ["Sistema"] = new()
            {
                new() { Name = "Hora actual", Unit = "" },
                new() { Name = "Hora (TIME)", Unit = "" },
                new() { Name = "Hora completa", Unit = "" },
                new() { Name = "Solo hora", Unit = "" },
                new() { Name = "Solo minutos", Unit = "" },
                new() { Name = "Fecha actual", Unit = "" },
                new() { Name = "Fecha completa", Unit = "" },
                new() { Name = "Solo año", Unit = "" },
                new() { Name = "Solo mes", Unit = "" },
                new() { Name = "Solo dia", Unit = "" },
                new() { Name = "Mes y dia", Unit = "" },
                new() { Name = "Dia de semana", Unit = "" },
                new() { Name = "Dia semana (alias)", Unit = "" },
                new() { Name = "Hoy", Unit = "" },
                new() { Name = "Fecha lunar", Unit = "" },
                new() { Name = "Tasa refresco", Unit = "Hz" }
            },
            ["Ventiladores"] = new()
            {
                new() { Name = "Fan 1", Unit = "RPM" },
                new() { Name = "Fan 2", Unit = "RPM" },
                new() { Name = "Fan 3", Unit = "RPM" },
                new() { Name = "Fan 4", Unit = "RPM" },
                new() { Name = "Fans (general)", Unit = "RPM" }
            },
            ["Disco"] = new()
            {
                new() { Name = "Temperatura Disco", Unit = "°C" },
                new() { Name = "Uso Disco", Unit = "%" },
                new() { Name = "Uso Disco (alias)", Unit = "%" },
                new() { Name = "Espacio Libre", Unit = "GB" }
            },
            ["Red"] = new()
            {
                new() { Name = "Descarga", Unit = "KB/s" },
                new() { Name = "Subida", Unit = "KB/s" },
                new() { Name = "Estado WiFi", Unit = "" },
                new() { Name = "Nombre WiFi", Unit = "" }
            },
            ["Clima"] = new()
            {
                new() { Name = "Temperatura clima", Unit = "°C" },
                new() { Name = "Clima nocturno", Unit = "" },
                new() { Name = "Clima maxima", Unit = "" }
            },
            ["Etiquetas"] = new()
            {
                new() { Name = "Etiqueta USAGE", Unit = "" },
                new() { Name = "Etiqueta DAMM", Unit = "" },
                new() { Name = "Kitten", Unit = "" },
                new() { Name = "Edgy", Unit = "" },
                new() { Name = "Texto libre", Unit = "" }
            },
            ["Barras"] = new()
            {
                // BorderLine - Dynamic progress bars
                new() { Name = "[BAR] CPU Usage", Unit = "%", WidgetType = WidgetType.BorderLine },
                new() { Name = "[BAR] GPU Usage", Unit = "%", WidgetType = WidgetType.BorderLine },
                new() { Name = "[BAR] Memory", Unit = "%", WidgetType = WidgetType.BorderLine },
                new() { Name = "[BAR] CPU Temp", Unit = "°C", WidgetType = WidgetType.BorderLine },
                new() { Name = "[BAR] GPU Temp", Unit = "°C", WidgetType = WidgetType.BorderLine },
                new() { Name = "[BAR] Fan1", Unit = "RPM", WidgetType = WidgetType.BorderLine },
                // DefaultLine - Static background bars
                new() { Name = "[BACK] Bar Background", Unit = "", WidgetType = WidgetType.DefaultLine },
                // GridLine - Segmented bars
                new() { Name = "[GRID] CPU Segments", Unit = "%", WidgetType = WidgetType.GridLine },
                new() { Name = "[GRID] GPU Segments", Unit = "%", WidgetType = WidgetType.GridLine }
            }
        };
        
        if (allWidgets.TryGetValue(SelectedCategory, out var widgets))
        {
            AvailableWidgets.Clear();
            foreach (var w in widgets)
                AvailableWidgets.Add(w);
        }
    }
    
    partial void OnSelectedCategoryChanged(string value)
    {
        LoadWidgetCategories();
    }
    
    partial void OnSelectedResolutionChanged(Resolution value)
    {
        IsCustomResolution = value.Name == "Personalizado";
        OnPropertyChanged(nameof(IsHorizontalLayout)); // Notify layout change
        OnPropertyChanged(nameof(PreviewContainerWidth)); // Notify container width change
        UpdateCanvasPreview(); // Update preview
    }
    
    [RelayCommand]
    private void BrowseBackground()
    {
        var dialog = new OpenFileDialog();
        
        // Configurar filtro según el tipo seleccionado
        switch (BackgroundType)
        {
            case "Imagen Estática":
                dialog.Filter = "Imágenes|*.png;*.jpg;*.jpeg";
                break;
            case "GIF Animado":
                dialog.Filter = "GIF Animado|*.gif";
                break;
            case "Video (MP4/AVI)":
                dialog.Filter = "Videos|*.mp4;*.avi;*.wmv;*.mov";
                break;
            case "Secuencia de Frames":
                dialog.Filter = "Imágenes PNG|*.png";
                dialog.Multiselect = true;
                break;
        }
        
        dialog.Title = "Seleccionar fondo";
        
        if (dialog.ShowDialog() == true)
        {
            if (BackgroundType == "Secuencia de Frames")
            {
                // Múltiples archivos
                ExtractedFramePaths = dialog.FileNames.ToList();
                ExtractedFramesCount = ExtractedFramePaths.Count;
                BackgroundPath = ExtractedFramePaths.FirstOrDefault() ?? "";
            }
            else
            {
                BackgroundPath = dialog.FileName;
            }
            
            LoadBackgroundPreview();
        }
    }
    
    [RelayCommand]
    private async Task ExtractFrames()
    {
        if (string.IsNullOrEmpty(BackgroundPath) || !File.Exists(BackgroundPath))
        {
            System.Windows.MessageBox.Show("Primero selecciona un archivo GIF o Video", "Error");
            return;
        }
        
        IsExtracting = true;
        ExtractionStatus = "Extrayendo frames...";
        ExtractedFramePaths.Clear();
        ExtractedFramesCount = 0;
        
        try
        {
            var outputFolder = _animationService.GetTempFramesFolder();
            
            // Limpiar frames anteriores
            _animationService.CleanupTempFrames();
            
            List<string> frames;
            
            if (BackgroundType == "GIF Animado")
            {
                ExtractionStatus = "Extrayendo frames del GIF...";
                frames = await _animationService.ExtractGifFrames(BackgroundPath, outputFolder);
            }
            else if (BackgroundType == "Video (MP4/AVI)")
            {
                ExtractionStatus = $"Extrayendo frames del video ({AnimationFPS} FPS)...";
                frames = await _animationService.ExtractVideoFrames(BackgroundPath, outputFolder, AnimationFPS);
            }
            else
            {
                ExtractionStatus = "Tipo de fondo no soportado para extracción";
                return;
            }
            
            ExtractedFramePaths = frames;
            ExtractedFramesCount = frames.Count;
            ExtractionStatus = $"✓ Extraídos {frames.Count} frames correctamente";
            
            System.Windows.MessageBox.Show(
                $"Se extrajeron {frames.Count} frames en:\n{outputFolder}",
                "Extracción Exitosa",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Information);
        }
        catch (Exception ex)
        {
            ExtractionStatus = $"❌ Error: {ex.Message}";
            System.Windows.MessageBox.Show(
                $"Error al extraer frames:\n\n{ex.Message}",
                "Error",
                System.Windows.MessageBoxButton.OK,
                System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsExtracting = false;
        }
    }
    
    private void LoadBackgroundPreview()
    {
        if (string.IsNullOrEmpty(BackgroundPath) || !File.Exists(BackgroundPath))
        {
            BackgroundPreview = null;
            VideoSource = null;
            IsVideoBackground = false;
            return;
        }
        
        var ext = Path.GetExtension(BackgroundPath).ToLowerInvariant();
        
        if (ext is ".mp4" or ".avi" or ".wmv" or ".mov")
        {
            // Video - show in MediaElement
            IsVideoBackground = true;
            VideoSource = new Uri(BackgroundPath);
            BackgroundPreview = null;
        }
        else
        {
            // Static image
            IsVideoBackground = false;
            VideoSource = null;
            
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(BackgroundPath);
                bitmap.EndInit();
                BackgroundPreview = bitmap;
            }
            catch
            {
                BackgroundPreview = null;
            }
        }
        
        UpdateCanvasPreview();
    }
    
    [RelayCommand]
    private void AddWidget()
    {
        // If no widget is selected, use first available
        var widgetToAdd = SelectedAvailableWidget ?? AvailableWidgets.FirstOrDefault();
        if (widgetToAdd == null) return;
        
        var newWidget = new WidgetItem
        {
            Name = widgetToAdd.Name,
            WidgetType = WidgetType.Text, // Default to text
            Unit = widgetToAdd.Unit,
            X = 20,
            Y = 50 + (SelectedWidgets.Count * 40),
            FontSize = 24,
            Font = "Impact",
            Color = "#FFFFFF"
        };
        
        SelectedWidgets.Add(newWidget);
        UpdateCanvasPreview(); // Update preview
    }
    
    [RelayCommand]
    private void RemoveWidget()
    {
        if (SelectedThemeWidget == null) return;
        SelectedWidgets.Remove(SelectedThemeWidget);
        UpdateCanvasPreview(); // Update preview
    }
    
    [RelayCommand]
    private void ApplyWidgetChanges()
    {
        if (SelectedThemeWidget == null) return;
        
        // Properties are now auto-synced via direct binding
        // This command just refreshes the list display and canvas
        
        // Refresh the list to update Display
        var index = SelectedWidgets.IndexOf(SelectedThemeWidget);
        if (index >= 0)
        {
            var widget = SelectedThemeWidget;
            SelectedWidgets.RemoveAt(index);
            SelectedWidgets.Insert(index, widget);
            SelectedThemeWidget = widget;
        }
        
        UpdateCanvasPreview(); // Update preview
    }
    
    [RelayCommand]
    private void GoBack()
    {
        if (CurrentStep > 1)
        {
            CurrentStep--;
            UpdateNavigation();
        }
    }
    
    [RelayCommand]
    private void GoNext()
    {
        if (CurrentStep < 4)
        {
            if (!ValidateCurrentStep()) return;
            CurrentStep++;
            UpdateNavigation();
            
            if (CurrentStep == 4)
                UpdateSummary();
        }
        else
        {
            SaveTheme();
        }
    }
    
    private bool ValidateCurrentStep()
    {
        if (CurrentStep == 1)
        {
            if (string.IsNullOrWhiteSpace(ThemeName))
            {
                System.Windows.MessageBox.Show("Ingresa un nombre para el tema.", "Validacion");
                return false;
            }
        }
        return true;
    }
    
    private void UpdateNavigation()
    {
        CanGoBack = CurrentStep > 1;
        NextButtonText = CurrentStep == 4 ? "Crear Tema" : "Siguiente ->";
    }
    
    private void UpdateSummary()
    {
        var width = IsCustomResolution ? CustomWidth : SelectedResolution.Width;
        var height = IsCustomResolution ? CustomHeight : SelectedResolution.Height;
        
        Summary = $"Nombre: {ThemeName}\n" +
                  $"Resolucion: {width}x{height}\n" +
                  $"Tipo: {(IsSettingTxtType ? "Setting.txt" : "GIF Simple")}\n" +
                  $"Fondo: {(string.IsNullOrEmpty(BackgroundPath) ? "Negro" : Path.GetFileName(BackgroundPath))}\n" +
                  $"Widgets: {SelectedWidgets.Count}";
    }
    
    private void SaveTheme()
    {
        var width = IsCustomResolution ? CustomWidth : SelectedResolution.Width;
        var height = IsCustomResolution ? CustomHeight : SelectedResolution.Height;
        
        var request = new ThemeSaveRequest
        {
            ThemeName = ThemeName.Trim(),
            Width = width,
            Height = height,
            ThemeType = IsSettingTxtType ? 1 : 0,
            BackgroundPath = BackgroundPath,
            FramePaths = ExtractedFramePaths, // Pass extracted frames for animation
            Widgets = SelectedWidgets.Select(w => new WidgetInfo
            {
                Name = w.Name,
                Unit = w.Unit,
                X = w.X,
                Y = w.Y,
                FontSize = w.FontSize,
                Font = w.Font,
                Color = w.Color,
                WidgetType = (SnakeMarsTheme.Services.WidgetType)(int)w.WidgetType,
                BarWidth = w.BarWidth,
                BarHeight = w.BarHeight,
                MaxNum = w.MaxNum,
                CornerRadius = w.CornerRadius,
                Fill = w.Fill,
                BackColor = w.BackColor,
                Margin = w.Margin,
                MaxCount = w.MaxCount,
                Orientation = w.Orientation
            }).ToList()
        };
        
        var result = _themeCreatorService.SaveTheme(request);
        
        if (result.Success)
        {
            System.Windows.MessageBox.Show(result.Message, "Exito", 
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);
        }
        else
        {
            System.Windows.MessageBox.Show(result.Error, "Error", 
                System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
    }
}

public enum WidgetType
{
    Text,
    BorderLine,   // Dynamic progress bar
    DefaultLine,  // Static background bar
    GridLine      // Segmented progress bar
}

// Widget item class
public class WidgetItem : System.ComponentModel.INotifyPropertyChanged
{
    public event System.ComponentModel.PropertyChangedEventHandler? PropertyChanged;
    
    private void NotifyPropertyChanged([System.Runtime.CompilerServices.CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new System.ComponentModel.PropertyChangedEventArgs(propertyName));
    }
    
    public string Name { get; set; } = "";
    public string? Icon { get; set; }
    public string Category { get; set; } = "";
    // public string Type { get; set; } = ""; // Removed
    public string? Unit { get; set; }
    
    private int _x;
    public int X
    {
        get => _x;
        set
        {
            if (_x != value)
            {
                _x = value;
                NotifyPropertyChanged();
            }
        }
    }
    
    private int _y;
    public int Y
    {
        get => _y;
        set
        {
            if (_y != value)
            {
                _y = value;
                NotifyPropertyChanged();
            }
        }
    }
    
    private int _fontSize = 24;
    public int FontSize
    {
        get => _fontSize;
        set
        {
            if (_fontSize != value)
            {
                _fontSize = value;
                NotifyPropertyChanged();
            }
        }
    }
    
    private string _font = "Impact";
    public string Font
    {
        get => _font;
        set
        {
            if (_font != value)
            {
                _font = value;
                NotifyPropertyChanged();
            }
        }
    }
    
    private string _color = "#FFFFFF";
    public string Color
    {
        get => _color;
        set
        {
            if (_color != value)
            {
                _color = value;
                NotifyPropertyChanged();
                NotifyPropertyChanged(nameof(ColorMedia));
                NotifyPropertyChanged(nameof(ColorBrush));
            }
        }
    }
    
    // For ColorPicker binding (System.Windows.Media.Color)
    public System.Windows.Media.Color ColorMedia
    {
        get
        {
            try
            {
                return (System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(Color);
            }
            catch
            {
                return System.Windows.Media.Colors.White;
            }
        }
        set
        {
            // Convert to hex string (remove alpha for 6-char format)
            var hex = $"#{value.R:X2}{value.G:X2}{value.B:X2}";
            if (Color != hex)
            {
                Color = hex; // This triggers NotifyPropertyChanged for Color
            }
        }
    }
    
    public int Z { get; set; } = 0;
    public int MaxNum { get; set; } = 100; // General MaxNum
    
    // Widget type (Text, BorderLine, DefaultLine, GridLine)
    public WidgetType WidgetType { get; set; } = WidgetType.Text;
    
    // For canvas rendering
    public string DisplayText
    {
        get
        {
            if (string.IsNullOrEmpty(Unit))
                return Name;
            return $"75 {Unit}"; // Simulated value
        }
    }
    
    public System.Windows.Media.Brush ColorBrush
    {
        get
        {
            try
            {
                return new System.Windows.Media.SolidColorBrush((System.Windows.Media.Color)System.Windows.Media.ColorConverter.ConvertFromString(Color));
            }
            catch
            {
                return System.Windows.Media.Brushes.White;
            }
        }
    }
    
    // Bar properties (for BorderLine/DefaultLine/GridLine)
    public int BarWidth { get; set; } = 100;
    public int BarHeight { get; set; } = 10;
    // public int MaxNum { get; set; } = 100; // This was the bar-specific MaxNum, now there's a general one above
    public int CornerRadius { get; set; } = 5;
    public string Fill { get; set; } = "#00FF00";
    public string BackColor { get; set; } = "#333333";
    
    // GridLine specific
    public int Margin { get; set; } = 5;
    public int MaxCount { get; set; } = 10;
    public string Orientation { get; set; } = "Horizontal";
    
    public string Display => WidgetType == WidgetType.Text 
        ? $"{Name} ({X},{Y})" 
        : $"[{WidgetType}] {Name} ({X},{Y})";
    
    // Canvas preview properties (scaled for display)
    public double CanvasX => X * 0.3; // Scale factor for preview
    public double CanvasY => Y * 0.3;
    public double ScaledFontSize => Math.Max(8, FontSize * 0.3);
    public double ScaledBarWidth => BarWidth * 0.3;
    public double ScaledBarHeight => BarHeight * 0.3;
}
