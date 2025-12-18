using System;
using System.IO;

namespace SnakeMarsTheme.Services;

/// <summary>
/// Gestiona las rutas de la aplicación asegurando compatibilidad Portable/Install.
/// </summary>
public static class PathService
{
    private static bool _isInitialized;

    public static bool IsPortableMode { get; private set; }
    
    // Directorio de Instalación (Solo Lectura en modo Instalado, Lectura/Escritura en Portable)
    public static string AppDir { get; private set; } = AppDomain.CurrentDomain.BaseDirectory;
    
    // Directorio de Datos de Usuario (Siempre Escritura)
    public static string UserDataDir { get; private set; } = "";

    // Subcarpetas Estandarizadas (Espejo de resources/)
    public static string UserResourcesPath => Path.Combine(UserDataDir, "resources");
    public static string UserThemesPath => Path.Combine(UserResourcesPath, "themes"); // Instalados
    public static string UserDownloadsPath => Path.Combine(UserResourcesPath, "ThemesPhoto"); // Descargas
    
    // Rutas de herramientas (AHORA CENTRALIZADAS)
    public static string FFmpegPath => Path.Combine(UserResourcesPath, "FFmpeg"); // FFmpeg binarios
    public static string ConvertedGifsPath => Path.Combine(UserDataDir, "ConvertedGifs"); // Frames temporales
    
    // Ruta legacy de FFmpeg (para migración/limpieza)
    public static string LegacyFFmpegPath => Path.Combine(
        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
        "SnakeMarsTheme", "FFmpeg");
    
    // Rutas legacy de recursos (para lectura de templates/defaults)
    public static string DefaultResourcesPath => Path.Combine(AppDir, "resources");

    public static void Initialize()
    {
        if (_isInitialized) return;

        AppDir = AppDomain.CurrentDomain.BaseDirectory;
        
        // Detección: Intentar escribir un archivo temporal en AppDir para ver si es portable
        IsPortableMode = CheckWriteAccess(AppDir);
        
        if (IsPortableMode)
        {
            // MODO PORTABLE: Datos en ./userdata
            UserDataDir = Path.Combine(AppDir, "userdata");
        }
        else
        {
            // MODO INSTALADO: Datos en Mis Documentos
            UserDataDir = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments), "SnakeMarsTheme");
        }
        
        // Asegurar carpetas base
        EnsureDirectory(UserDataDir);
        EnsureDirectory(UserThemesPath);
        EnsureDirectory(UserDownloadsPath);
        EnsureDirectory(FFmpegPath);
        EnsureDirectory(ConvertedGifsPath);
        
        // Migrar FFmpeg legacy si existe
        MigrateLegacyFFmpeg();

        _isInitialized = true;
    }

    private static bool CheckWriteAccess(string folderPath)
    {
        try
        {
            // Intentar crear un archivo temporal oculto
            string testFile = Path.Combine(folderPath, ".write_test_" + Guid.NewGuid().ToString());
            File.WriteAllText(testFile, "test");
            File.Delete(testFile);
            return true;
        }
        catch (UnauthorizedAccessException)
        {
            return false;
        }
        catch (Exception)
        {
            return false;
        }
    }
    
    private static void EnsureDirectory(string path)
    {
        if (!Directory.Exists(path))
        {
            try { Directory.CreateDirectory(path); } catch { }
        }
    }
    
    /// <summary>
    /// Migra FFmpeg desde LocalAppData a la nueva ubicación centralizada.
    /// </summary>
    private static void MigrateLegacyFFmpeg()
    {
        try
        {
            if (!Directory.Exists(LegacyFFmpegPath)) return;
            
            var legacyFfmpeg = Path.Combine(LegacyFFmpegPath, "ffmpeg.exe");
            var newFfmpeg = Path.Combine(FFmpegPath, "ffmpeg.exe");
            
            // Solo migrar si no existe en nueva ubicación
            if (File.Exists(legacyFfmpeg) && !File.Exists(newFfmpeg))
            {
                // Copiar archivos de FFmpeg
                foreach (var file in Directory.GetFiles(LegacyFFmpegPath))
                {
                    var destFile = Path.Combine(FFmpegPath, Path.GetFileName(file));
                    if (!File.Exists(destFile))
                    {
                        File.Copy(file, destFile);
                    }
                }
                
                // Intentar limpiar carpeta legacy
                CleanupLegacyFFmpeg();
            }
        }
        catch { /* Ignorar errores de migración */ }
    }
    
    /// <summary>
    /// Limpia la carpeta legacy de FFmpeg en LocalAppData.
    /// </summary>
    public static void CleanupLegacyFFmpeg()
    {
        try
        {
            if (Directory.Exists(LegacyFFmpegPath))
            {
                Directory.Delete(LegacyFFmpegPath, true);
            }
            
            // También intentar eliminar carpeta padre si está vacía
            var parentDir = Path.GetDirectoryName(LegacyFFmpegPath);
            if (!string.IsNullOrEmpty(parentDir) && Directory.Exists(parentDir))
            {
                if (Directory.GetFileSystemEntries(parentDir).Length == 0)
                {
                    Directory.Delete(parentDir);
                }
            }
        }
        catch { /* Ignorar errores de limpieza */ }
    }
    
    /// <summary>
    /// Limpia todas las carpetas temporales de la aplicación.
    /// </summary>
    public static void CleanupTempData()
    {
        try
        {
            // Limpiar GIFs convertidos
            if (Directory.Exists(ConvertedGifsPath))
            {
                foreach (var file in Directory.GetFiles(ConvertedGifsPath))
                {
                    try { File.Delete(file); } catch { }
                }
            }
        }
        catch { /* Ignorar errores */ }
    }
}
