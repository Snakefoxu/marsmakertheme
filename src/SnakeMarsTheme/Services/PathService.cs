using System;
using System.IO;
using System.Security.AccessControl;
using System.Security.Principal;

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

    // Subcarpetas Estandarizadas de Usuario
    // Subcarpetas Estandarizadas (Espejo de resources/)
    public static string UserResourcesPath => Path.Combine(UserDataDir, "resources");
    public static string UserThemesPath => Path.Combine(UserResourcesPath, "themes"); // Instalados
    public static string UserDownloadsPath => Path.Combine(UserResourcesPath, "ThemesPhoto"); // Descargas
    
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
}
