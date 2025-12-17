using System.Collections.ObjectModel;
using System.IO;
using System.Windows.Media.Imaging;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using SnakeMarsTheme.Services;

namespace SnakeMarsTheme.ViewModels;

public partial class MainViewModel : ObservableObject
{
    private readonly ThemeService _themeService;
    
    [ObservableProperty]
    private ObservableCollection<ThemeInfo> _themes = new();
    
    [ObservableProperty]
    private ThemeInfo? _selectedTheme;
    
    [ObservableProperty]
    private BitmapImage? _previewImage;
    
    [ObservableProperty]
    private string _statusText = "Listo";
    
    [ObservableProperty]
    private string _themeDetails = "";
    
    public MainViewModel()
    {
        // Use PathService for robust path handling (Portable vs Installed)
        _themeService = new ThemeService(PathService.AppDir, PathService.UserDataDir);
        LoadThemes();
    }
    
    [RelayCommand]
    private void LoadThemes()
    {
        StatusText = "Cargando temas...";
        Themes.Clear();
        
        var themeList = _themeService.GetAllThemes();
        foreach (var theme in themeList)
        {
            Themes.Add(theme);
        }
        
        StatusText = $"Listo - {Themes.Count} temas encontrados";
    }
    
    partial void OnSelectedThemeChanged(ThemeInfo? value)
    {
        if (value == null)
        {
            PreviewImage = null;
            ThemeDetails = "";
            return;
        }
        
        // Load preview image
        var thumbnailPath = _themeService.GetThumbnailPath(value.Name);
        if (thumbnailPath != null && File.Exists(thumbnailPath))
        {
            try
            {
                var bitmap = new BitmapImage();
                bitmap.BeginInit();
                bitmap.CacheOption = BitmapCacheOption.OnLoad;
                bitmap.UriSource = new Uri(thumbnailPath);
                bitmap.EndInit();
                PreviewImage = bitmap;
            }
            catch
            {
                PreviewImage = null;
            }
        }
        else
        {
            PreviewImage = null;
        }
        
        // Update details
        ThemeDetails = $"Nombre: {value.Name}\n" +
                       $"Resolucion: {value.Resolution}\n" +
                       $"Tipo: {value.TypeName}";
        
        StatusText = $"Tema seleccionado: {value.Name}";
    }
    
    [RelayCommand]
    private void CreateTheme()
    {
        StatusText = "Wizard en desarrollo...";
    }
    
    [RelayCommand]
    private void RefreshThemes()
    {
        LoadThemes();
    }
    
    [RelayCommand]
    private void OpenResourcesFolder()
    {
        var resourcesPath = Path.Combine(_themeService.BasePath, "resources");
        if (Directory.Exists(resourcesPath))
        {
            System.Diagnostics.Process.Start("explorer.exe", resourcesPath);
            StatusText = $"Abriendo: {resourcesPath}";
        }
        else
        {
            StatusText = "Carpeta de recursos no encontrada";
        }
    }
    
    [RelayCommand]
    private void ClearPreviewCache()
    {
        try
        {
            var tempPath = Path.Combine(Path.GetTempPath(), "SnakeMarsTheme");
            if (Directory.Exists(tempPath))
            {
                Directory.Delete(tempPath, true);
                StatusText = "Cache de previews limpiada";
            }
            else
            {
                StatusText = "No hay cache que limpiar";
            }
        }
        catch (Exception ex)
        {
            StatusText = $"Error: {ex.Message}";
        }
    }
}
