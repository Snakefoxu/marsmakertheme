using System.Collections.ObjectModel;
using System.IO;
using System.Windows;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SnakeMarsTheme.Services;

namespace SnakeMarsTheme.ViewModels;

public partial class DownloaderViewModel : ObservableObject
{
    private readonly DownloadService _downloadService;
    private readonly ExtractionService _extractionService;
    private readonly InstallationService _installationService;
    private readonly ThemeCatalogService _catalogService;
    private readonly SmthemePackagerService _smthemeService;
    
    // Audit Compliance: Split Read (Install) vs Write (User Data) paths
    private readonly string _readPath;
    private readonly string _writePath;
    
    private string? _lastExtractedPath;
    
    [ObservableProperty]
    private ObservableCollection<RemoteTheme> _availableThemes = new();
    
    [ObservableProperty]
    private ObservableCollection<string> _resolutions = new() 
    { 
        "Todas",
        "Horizontal",   // width > height (960x320, 800x480, 1920x480, etc)
        "Vertical",     // height > width (320x960, 480x800, 320x480, etc)
        "AIO/Cuadrado", // 320x240, 240x320, 480x480, small screens
    };
    
    [ObservableProperty]
    private string _selectedResolution = "Todas";
    
    [ObservableProperty]
    private RemoteTheme? _selectedTheme;
    
    [ObservableProperty]
    private string _statusText = "Haz clic en 'Cargar Temas' para ver los temas disponibles";
    
    [ObservableProperty]
    private int _downloadProgress;
    
    [ObservableProperty]
    private bool _isLoading;
    
    [ObservableProperty]
    private bool _isDownloading;
    
    [ObservableProperty]
    private string _selectedThemeName = "";
    
    [ObservableProperty]
    private string _selectedThemeSize = "";
    
    [ObservableProperty]
    private string _selectedThemeStatus = "";

    [ObservableProperty]
    private string _selectedThemeType = "";
    
    // Statistics
    [ObservableProperty]
    private string _totalThemes = "0";
    
    [ObservableProperty]
    private string _totalSize = "0 MB";
    
    [ObservableProperty]
    private bool _is7ZipAvailable;
    
    [ObservableProperty]
    private bool _isSOEYIInstalled;
    
    [ObservableProperty]
    private bool _isMarsInstalled;
    
    // Preview properties
    [ObservableProperty]
    private BitmapImage? _themePreviewImage;
    
    [ObservableProperty]
    private bool _hasPreview;
    
    [ObservableProperty]
    private string _selectedThemeId = "";
    
    [ObservableProperty]
    private string _selectedThemeResolution = "";
    
    [ObservableProperty]
    private string _selectedThemeSource = "";
    
    // Catalog info properties
    [ObservableProperty]
    private string _catalogTotalThemes = "0";
    
    [ObservableProperty]
    private string _catalogResolutions = "15+";
    
    [ObservableProperty]
    private string _catalogTotalSize = "~1.77 GB";
    
    // Bulk download properties
    [ObservableProperty]
    private string _themeIdsToDownload = "";
    
    [ObservableProperty]
    private int _bulkDownloadProgress;
    
    [ObservableProperty]
    private string _bulkDownloadStatus = "Listo para descargar";

    public DownloaderViewModel()
    {
        // PathService handles all path logic (Audit Compliant)
        _readPath = PathService.AppDir;      // Read-only stock assets (Catalog, Previews)
        _writePath = PathService.UserDataDir; // Writable user data (Downloads, Extracted Themes)
        
        _downloadService = new DownloadService(_readPath, _writePath);
        _extractionService = new ExtractionService(_writePath); // Extracts to user folder
        _installationService = new InstallationService();
        _catalogService = new ThemeCatalogService(_readPath); // Reads stock catalog
        _smthemeService = new SmthemePackagerService();
        
        // Check installations
        Is7ZipAvailable = _extractionService.Is7ZipAvailable();
        IsSOEYIInstalled = _installationService.IsSOEYIInstalled();
        IsMarsInstalled = _installationService.IsMarsGamingInstalled();
    }
    
    private void RefreshThemesList()
    {
        if (AvailableThemes.Count == 0) return;
        
        // We verify internal list, not re-fetch
        var filtered = AvailableThemes.Where(t => 
            SelectedResolution == "Todas" || 
            t.Resolution.Contains(SelectedResolution.Split(' ')[0])
        ).ToList();
    }
    
    partial void OnSelectedResolutionChanged(string value)
    {
        StatusText = $"Filtro: {value} (Recargar para aplicar cambios si no se actualiza)";
    }
    
    [RelayCommand]
    private async Task LoadThemesAsync()
    {
        IsLoading = true;
        StatusText = $"Cargando catálogo desde: {_readPath}";
        AvailableThemes.Clear();
        
        try
        {
            var themes = await _downloadService.GetAvailableThemesAsync();
            
            if (themes.Count == 0)
            {
                StatusText = $"Catálogo vacío. Path: {_readPath}";
                return;
            }
            
            long totalBytes = 0;
            
            var filteredThemes = themes;
            if (SelectedResolution != "Todas")
            {
                filteredThemes = themes.Where(t => MatchesCategory(t.Resolution, SelectedResolution)).ToList();
            }

            foreach (var theme in filteredThemes)
            {
                theme.IsDownloaded = _downloadService.IsDownloaded(theme);
                AvailableThemes.Add(theme);
                totalBytes += theme.Size;
            }
            
            TotalThemes = AvailableThemes.Count.ToString();
            TotalSize = $"{totalBytes / (1024.0 * 1024.0):F2} MB"; // Size is likely MB now or mixed
            
            // Recalculate GB if large
            if (totalBytes > 1024L * 1024 * 1024)
                TotalSize = $"{totalBytes / (1024.0 * 1024.0 * 1024.0):F2} GB";

            StatusText = $"Catálogo actualizado: {AvailableThemes.Count} temas encontrados";
            CatalogTotalThemes = themes.Count.ToString();
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
            System.Windows.MessageBox.Show($"Error cargando catálogo:\n{ex.Message}\n\nPath: {_readPath}\n\nStack:\n{ex.StackTrace}", "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
        }
        finally
        {
            IsLoading = false;
        }
    }
    
    [RelayCommand]
    private async Task DownloadSelectedAsync()
    {
        if (SelectedTheme == null)
        {
            StatusText = "Selecciona un tema primero";
            return;
        }

        bool isSmtheme = SelectedTheme.Type == "smtheme";
        
        if (_downloadService.IsDownloaded(SelectedTheme))
        {
            StatusText = $"'{SelectedTheme.Name}' ya descargado.";
            UpdateThemeStatus();
            return;
        }
        
        IsDownloading = true;
        DownloadProgress = 0;
        StatusText = $"Descargando {SelectedTheme.Name}...";
        
        var progress = new Progress<int>(p =>
        {
            DownloadProgress = p;
            StatusText = $"Descargando {SelectedTheme.Name}... {p}%";
        });
        
        try
        {
            var success = await _downloadService.DownloadThemeAsync(SelectedTheme, progress);
            
            if (success)
            {
                StatusText = $"'{SelectedTheme.Name}' descargado exitosamente!";
                SelectedTheme.IsDownloaded = true;
                UpdateThemeStatus();
            }
            else
            {
                StatusText = "Error durante la descarga";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
        finally
        {
            IsDownloading = false;
            DownloadProgress = 0;
        }
    }
    
    [RelayCommand]
    private async Task ExtractSelectedAsync()
    {
        if (SelectedTheme == null)
        {
            StatusText = "Selecciona un tema primero";
            return;
        }
        
        bool isSmtheme = SelectedTheme.Type == "smtheme";
        string filePath;

        if (isSmtheme)
        {
             filePath = Path.Combine(_writePath, "resources", "Themes_SMTHEME", SelectedTheme.FileName);
        }
        else
        {
             filePath = Path.Combine(_writePath, "resources", "ThemesPhoto", SelectedTheme.FileName);
        }
        
        if (!File.Exists(filePath))
        {
            StatusText = "Primero descarga el tema";
            return;
        }
        
        // Handle .smtheme extraction
        if (isSmtheme)
        {
            try
            {
                IsDownloading = true;
                StatusText = $"Instalando paquete unificado {SelectedTheme.Name}...";
                
                // Define extraction path (standard themes folder in User Data)
                string cleanName = SelectedTheme.Name.Trim();
                string outputFolder = Path.Combine(_writePath, "resources", "themes", cleanName);
                
                // Ensure unique
                if (Directory.Exists(outputFolder))
                {
                    outputFolder = Path.Combine(_writePath, "resources", "themes", $"{cleanName}_{DateTime.Now.Ticks}");
                }

                await Task.Run(() => _smthemeService.UnpackTheme(filePath, outputFolder));
                _lastExtractedPath = outputFolder;
                
                StatusText = $"Instalado correctamente en: {Path.GetFileName(outputFolder)}";
                UpdateThemeStatus();
                
                MessageBox.Show($"Tema instalado correctamente.\nUbicación: {outputFolder}", "Instalación Completa", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            catch (Exception ex)
            {
                 StatusText = $"Error instalando smtheme: {ex.Message}";
                 MessageBox.Show($"Error al instalar el tema: {ex.Message}", "Error", MessageBoxButton.OK, MessageBoxImage.Error);
            }
            finally
            {
                IsDownloading = false;
            }
            return;
        }

        // Handle legacy .photo extraction
        if (!Is7ZipAvailable)
        {
            MessageBox.Show("7-Zip no está instalado.\n\nDescárgalo de: https://7-zip.org", "7-Zip requerido", MessageBoxButton.OK, MessageBoxImage.Warning);
            return;
        }
        
        IsDownloading = true;
        StatusText = $"Extrayendo tema legacy {SelectedTheme.Name}...";
        
        try
        {
            var result = await _extractionService.ExtractPhotoAsync(filePath);
            
            if (result.Success)
            {
                _lastExtractedPath = result.ExtractedPath;
                StatusText = $"Extraido: {string.Join(", ", result.ThemeFolders)}";
                UpdateThemeStatus();
            }
            else
            {
                StatusText = $"Error: {result.Error}";
            }
        }
        finally
        {
            IsDownloading = false;
        }
    }
    
    [RelayCommand]
    private void InstallToSOEYI()
    {
        if (string.IsNullOrEmpty(_lastExtractedPath))
        {
            // Try to guess path if selected
             if (SelectedTheme != null)
             {
                 // Check if standard extracted path exists (in User Data)
                 string potentialPath = Path.Combine(_writePath, "resources", "themes", SelectedTheme.Name);
                 if (Directory.Exists(potentialPath))
                 {
                     _lastExtractedPath = potentialPath;
                 }
                 else
                 {
                     StatusText = "Primero extrae/instala el tema";
                     return;
                 }
             }
             else
             {
                 return;
             }
        }
        
        var result = _installationService.InstallToSOEYI(_lastExtractedPath);
        
        if (result.Success)
        {
            MessageBox.Show(result.Message, "Instalación exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
            StatusText = $"Instalado en SOEYI: {string.Join(", ", result.InstalledThemes)}";
        }
        else
        {
            MessageBox.Show(result.Error, "Error de instalación", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    [RelayCommand]
    private void InstallToMars()
    {
        if (string.IsNullOrEmpty(_lastExtractedPath))
        {
             // Try to guess path if selected
             if (SelectedTheme != null)
             {
                 // Check if standard extracted path exists
                 string potentialPath = Path.Combine(_writePath, "resources", "themes", SelectedTheme.Name);
                 if (Directory.Exists(potentialPath))
                 {
                     _lastExtractedPath = potentialPath;
                 }
                 else
                 {
                     StatusText = "Primero extrae/instala el tema";
                     return;
                 }
             }
             else
             {
                 return;
             }
        }
        
        var result = _installationService.InstallToMarsGaming(_lastExtractedPath);
        
        if (result.Success)
        {
            MessageBox.Show(result.Message, "Instalación exitosa", MessageBoxButton.OK, MessageBoxImage.Information);
            StatusText = $"Instalado en Mars Gaming: {string.Join(", ", result.InstalledThemes)}";
        }
        else
        {
            MessageBox.Show(result.Error, "Error de instalación", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
    
    [RelayCommand]
    private async Task RestartSOEYIAsync()
    {
        StatusText = "Reiniciando SOEYI...";
        var success = await _installationService.RestartSOEYIAsync();
        StatusText = success ? "SOEYI reiniciado" : "Error al reiniciar SOEYI";
    }
    
    [RelayCommand]
    private async Task RestartMarsAsync()
    {
        StatusText = "Reiniciando Mars Gaming...";
        var success = await _installationService.RestartMarsGamingAsync();
        StatusText = success ? "Mars Gaming reiniciado" : "Error al reiniciar Mars Gaming";
    }
    
    [RelayCommand]
    private async Task DownloadAllThemesAsync()
    {
        var result = MessageBox.Show(
            "¿Descargar TODOS los temas del catálogo?\n\nEsto puede llenar tu disco rápido.",
            "Descarga Masiva",
            MessageBoxButton.YesNo,
            MessageBoxImage.Question);
            
        if (result != MessageBoxResult.Yes) return;
        
        BulkDownloadProgress = 0;
        BulkDownloadStatus = "Iniciando descarga masiva...";
        
        try
        {
            var themes = await _downloadService.GetAvailableThemesAsync(); // Use cached list effectively
            int total = themes.Count;
            int downloaded = 0;
            
            foreach (var theme in themes)
            {
                if (_downloadService.IsDownloaded(theme))
                {
                    downloaded++;
                    BulkDownloadProgress = (int)((downloaded / (double)total) * 100);
                    BulkDownloadStatus = $"Tema {downloaded}/{total} ya existe, saltando...";
                    continue;
                }
                
                BulkDownloadStatus = $"Descargando {downloaded + 1}/{total}: {theme.Name}...";
                
                var progress = new Progress<int>(p =>
                {
                    BulkDownloadProgress = (int)(((downloaded / (double)total) + (p / 100.0 / total)) * 100);
                });
                
                await _downloadService.DownloadThemeAsync(theme, progress);
                downloaded++;
                BulkDownloadProgress = (int)((downloaded / (double)total) * 100);
            }
            
            BulkDownloadStatus = $"Completado: {downloaded} temas descargados!";
            BulkDownloadProgress = 100;
        }
        catch (Exception ex)
        {
            BulkDownloadStatus = $"Error: {ex.Message}";
        }
    }
    
    [RelayCommand]
    private async Task ExtractAndEditAsync()
    {
        await ExtractSelectedAsync();
        // User is notified in ExtractSelectedAsync
    }
    
    partial void OnSelectedThemeChanged(RemoteTheme? value)
    {
        if (value != null)
        {
            SelectedThemeName = value.Name;
            SelectedThemeSize = value.SizeFormatted;
            SelectedThemeId = value.Id.ToString();
            SelectedThemeResolution = value.Resolution;
            SelectedThemeType = value.Type; // photo or smtheme
            SelectedThemeSource = value.Source;
            
            UpdateThemeStatus();
            LoadPreviewImage(value);
            StatusText = $"Seleccionado: {value.Name} ({value.Type})";
        }
        else
        {
            SelectedThemeName = "";
            SelectedThemeSize = "";
            SelectedThemeId = "";
            SelectedThemeResolution = "";
            SelectedThemeStatus = "";
            SelectedThemeType = "";
            SelectedThemeSource = "";
            ThemePreviewImage = null;
            HasPreview = false;
        }
    }
    
    private void LoadPreviewImage(RemoteTheme theme)
    {
        ThemePreviewImage = null;
        HasPreview = false;
        
        // 1. Try LOCAL preview first (resources/previews/{name}.png) - READ Only
        var previewsFolder = Path.Combine(_readPath, "resources", "previews");
        var localPreview = Path.Combine(previewsFolder, $"{theme.Name}.png");
        
        if (File.Exists(localPreview))
        {
            try 
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(localPreview, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                ThemePreviewImage = bitmap;
                HasPreview = true;
                return;
            }
            catch { }
        }
        
        // 2. Fallback to remote URL (for themes without local preview)
        if (!string.IsNullOrEmpty(theme.ThumbnailUrl) && theme.ThumbnailUrl.StartsWith("http"))
        {
            try 
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.UriSource = new Uri(theme.ThumbnailUrl, UriKind.Absolute);
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.EndInit();
                bitmap.Freeze();
                ThemePreviewImage = bitmap;
                HasPreview = true;
                return;
            }
            catch { }
        }
        
        // 3. No preview available
        HasPreview = false;
    }
    
    private void UpdateThemeStatus()
    {
        if (SelectedTheme == null) return;
        
        bool isDownloaded = _downloadService.IsDownloaded(SelectedTheme);
        
        if (isDownloaded)
        {
            SelectedThemeStatus = "Descargado - Listo para instalar";
            // Check if extracted/installed in User Data
            if (Directory.Exists(Path.Combine(_writePath, "resources", "themes", SelectedTheme.Name)))
            {
                 SelectedThemeStatus = "INSTALADO & Descargado";
            }
        }
        else
        {
            SelectedThemeStatus = "Disponible en Nube";
        }
    }

    /// <summary>
    /// Clasifica una resolución en categoría: Horizontal, Vertical o AIO/Cuadrado
    /// </summary>
    private bool MatchesCategory(string resolution, string category)
    {
        if (string.IsNullOrEmpty(resolution) || resolution == "Unknown")
            return category == "Todas";

        // Parse resolution (e.g., "960x320" -> width=960, height=320)
        var parts = resolution.Split('x');
        if (parts.Length != 2 || !int.TryParse(parts[0], out int width) || !int.TryParse(parts[1], out int height))
            return false;

        // AIO: small screens (both <=480) OR square
        bool isSmallScreen = width <= 480 && height <= 480;
        bool isSquare = width == height;
        
        return category switch
        {
            "Horizontal" => !isSmallScreen && width > height,
            "Vertical" => !isSmallScreen && height > width,
            "AIO/Cuadrado" => isSmallScreen || isSquare,
            _ => true
        };
    }
}
