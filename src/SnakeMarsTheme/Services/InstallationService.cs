using System.IO;

namespace SnakeMarsTheme.Services;

/// <summary>
/// Service for installing themes to SOEYI and Mars Gaming applications.
/// </summary>
public class InstallationService
{
    // Default Fallback Paths
    private const string DEFAULT_SOEYI_PROGRAMME = @"C:\Program Files (x86)\SOEYI\Programme";
    private const string DEFAULT_MARS_PROGRAMME = @"C:\Program Files (x86)\MARS GAMING\Programme";
    
    // Scheme Subfolders
    private const string SOEYI_SCHEME_AIO = @"ThemeScheme\VMAXA180240_0320S252400400";
    private const string SOEYI_SCHEME_CHASIS = @"ThemeScheme\VMAXC230360_0960S252800005";
    private const string MARS_SCHEME_CHASIS = @"ThemeScheme\VMAXC230360_0960S252800005";
    
    private readonly string[] _marsExeNames = { "MarsGaming.exe", "Product/MarsGaming.exe" };
    private readonly string[] _soeyiExeNames = { "Soeyi.exe", "bin/Release/Soeyi.exe" };
    
    // Dynamic Paths Caches
    private string? _cachedSoeyiPath;
    private string? _cachedMarsPath;
    
    /// <summary>
    /// Check if SOEYI is installed.
    /// </summary>
    public bool IsSOEYIInstalled() => !string.IsNullOrEmpty(FindSoeyiPath());
    
    /// <summary>
    /// Check if Mars Gaming is installed.
    /// </summary>
    public bool IsMarsGamingInstalled() => !string.IsNullOrEmpty(FindMarsPath());
    
    /// <summary>
    /// Find the actual installation path for SOEYI.
    /// </summary>
    private string? FindSoeyiPath()
    {
        if (_cachedSoeyiPath != null && Directory.Exists(_cachedSoeyiPath)) return _cachedSoeyiPath;
        
        // 1. Check Registry
        var path = GetInstallPathFromRegistry("SOEYI") ?? GetInstallPathFromRegistry("VMAX");
        if (path != null && Directory.Exists(Path.Combine(path, "Programme")))
        {
            _cachedSoeyiPath = Path.Combine(path, "Programme");
            return _cachedSoeyiPath;
        }

        // 2. Check Standard Locations (C:, D:, E:)
        string[] drives = { "C:", "D:", "E:" };
        string[] candidates = { 
            @"Program Files (x86)\SOEYI\Programme", 
            @"Program Files\SOEYI\Programme",
            @"SOEYI\Programme" 
        };

        foreach (var drive in drives)
        {
            foreach (var candidate in candidates)
            {
                var fullPath = Path.Combine(drive + Path.DirectorySeparatorChar, candidate);
                if (Directory.Exists(fullPath))
                {
                    _cachedSoeyiPath = fullPath;
                    return _cachedSoeyiPath;
                }
            }
        }
        
        return null;
    }
    
    /// <summary>
    /// Find the actual installation path for Mars Gaming.
    /// </summary>
    private string? FindMarsPath()
    {
        if (_cachedMarsPath != null && Directory.Exists(_cachedMarsPath)) return _cachedMarsPath;
        
        // 1. Check Registry
        var path = GetInstallPathFromRegistry("Mars Gaming");
        if (path != null && Directory.Exists(Path.Combine(path, "Programme")))
        {
            _cachedMarsPath = Path.Combine(path, "Programme");
            return _cachedMarsPath;
        }

        // 2. Check Standard Locations
        string[] drives = { "C:", "D:", "E:" };
        string[] candidates = { 
            @"Program Files (x86)\MARS GAMING\Programme", 
            @"Program Files\MARS GAMING\Programme",
            @"MARS GAMING\Programme" 
        };

        foreach (var drive in drives)
        {
            foreach (var candidate in candidates)
            {
                var fullPath = Path.Combine(drive + Path.DirectorySeparatorChar, candidate);
                if (Directory.Exists(fullPath))
                {
                    _cachedMarsPath = fullPath;
                    return _cachedMarsPath;
                }
            }
        }
        
        return null;
    }

    private string? GetInstallPathFromRegistry(string appName)
    {
        try
        {
            // Windows Registry Uninstall key
            string registryKey = @"SOFTWARE\Microsoft\Windows\CurrentVersion\Uninstall";
            
            // Search in both 32-bit and 64-bit views
            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryKey))
            {
                if (key != null)
                {
                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        using (var subKey = key.OpenSubKey(subKeyName))
                        {
                            var displayName = subKey?.GetValue("DisplayName") as string;
                            if (displayName != null && displayName.Contains(appName, StringComparison.OrdinalIgnoreCase))
                            {
                                return subKey?.GetValue("InstallLocation") as string;
                            }
                        }
                    }
                }
            }
            
            // Check WOW6432Node
            string registryKey32 = @"SOFTWARE\WOW6432Node\Microsoft\Windows\CurrentVersion\Uninstall";
            using (var key = Microsoft.Win32.Registry.LocalMachine.OpenSubKey(registryKey32))
            {
                if (key != null)
                {
                    foreach (var subKeyName in key.GetSubKeyNames())
                    {
                        using (var subKey = key.OpenSubKey(subKeyName))
                        {
                            var displayName = subKey?.GetValue("DisplayName") as string;
                            if (displayName != null && displayName.Contains(appName, StringComparison.OrdinalIgnoreCase))
                            {
                                return subKey?.GetValue("InstallLocation") as string;
                            }
                        }
                    }
                }
            }
        }
        catch { /* Ignore registry errors */ }
        
        return null;
    }
    
    /// <summary>
    /// Install theme to SOEYI.
    /// </summary>
    public InstallResult InstallToSOEYI(string extractedThemePath)
    {
        var programmePath = FindSoeyiPath();
        if (string.IsNullOrEmpty(programmePath))
        {
            return new InstallResult
            {
                Success = false,
                Error = $"No se encontró la instalación de SOEYI en C:, D: o E: ni en Registro."
            };
        }
        
        // Root path is parent of Programme
        var rootPath = Directory.GetParent(programmePath)?.FullName;
        if (rootPath == null) return new InstallResult { Success = false, Error = "Ruta SOEYI inválida" };

        var schemePaths = new[] 
        { 
            Path.Combine(rootPath, SOEYI_SCHEME_AIO), 
            Path.Combine(rootPath, SOEYI_SCHEME_CHASIS) 
        };
        
        return InstallTheme(extractedThemePath, programmePath, schemePaths);
    }
    
    /// <summary>
    /// Install theme to Mars Gaming.
    /// </summary>
    public InstallResult InstallToMarsGaming(string extractedThemePath)
    {
         var programmePath = FindMarsPath();
         if (string.IsNullOrEmpty(programmePath))
         {
             return new InstallResult
             {
                 Success = false,
                 Error = $"No se encontró la instalación de Mars Gaming en C:, D: o E: ni en Registro."
             };
         }
         
         // Root path is parent of Programme
         var rootPath = Directory.GetParent(programmePath)?.FullName;
         if (rootPath == null) return new InstallResult { Success = false, Error = "Ruta Mars Gaming inválida" };
 
         var schemePaths = new[] 
         { 
             Path.Combine(rootPath, MARS_SCHEME_CHASIS) 
         };
         
         return InstallTheme(extractedThemePath, programmePath, schemePaths);
    }
    
    private InstallResult InstallTheme(string extractedPath, string programmePath, string[] schemePaths)
    {
        var installedThemes = new List<string>();
        
        try
        {
            if (!Directory.Exists(programmePath)) Directory.CreateDirectory(programmePath);

            foreach (var themeFolder in Directory.GetDirectories(extractedPath))
            {
                var themeName = Path.GetFileName(themeFolder);
                if (string.IsNullOrEmpty(themeName)) continue;
                
                // 1. Copy folder to Programme/
                var destProgramme = Path.Combine(programmePath, themeName);
                if (Directory.Exists(destProgramme))
                {
                    Directory.Delete(destProgramme, true);
                }
                CopyDirectory(themeFolder, destProgramme);
                
                // 2. Copy JSON to ThemeScheme folders
                var jsonFile = Path.Combine(themeFolder, $"{themeName}.json");
                if (File.Exists(jsonFile))
                {
                    foreach (var schemePath in schemePaths)
                    {
                        // Some users might delete unused resolutions folders, so recreate if needed or skip?
                        // Better to create if it doesn't exist for safety, or check parent.
                        if (!Directory.Exists(schemePath))
                        {
                            try { Directory.CreateDirectory(schemePath); } catch { /* ignore if fails */ }
                        }
                        
                        if (Directory.Exists(schemePath))
                        {
                            var destJson = Path.Combine(schemePath, $"{themeName}.json");
                            File.Copy(jsonFile, destJson, true);
                        }
                    }
                }
                
                installedThemes.Add(themeName);
            }
            
            return new InstallResult
            {
                Success = true,
                InstalledThemes = installedThemes,
                Message = $"Instalados: {string.Join(", ", installedThemes)}\n\n⚠️ IMPORTANTE: Reinicia la aplicación para ver los nuevos temas."
            };
        }
        catch (UnauthorizedAccessException)
        {
            return new InstallResult
            {
                Success = false,
                Error = "Acceso denegado. Ejecuta como Administrador."
            };
        }
        catch (Exception ex)
        {
            return new InstallResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }
    
    private static void CopyDirectory(string sourceDir, string destDir)
    {
        Directory.CreateDirectory(destDir);
        
        foreach (var file in Directory.GetFiles(sourceDir))
        {
            var destFile = Path.Combine(destDir, Path.GetFileName(file));
            File.Copy(file, destFile, true);
        }
        
        foreach (var dir in Directory.GetDirectories(sourceDir))
        {
            var destSubDir = Path.Combine(destDir, Path.GetFileName(dir));
            CopyDirectory(dir, destSubDir);
        }
    }
    
    /// <summary>
    /// Restart SOEYI application.
    /// </summary>
    public async Task<bool> RestartSOEYIAsync()
    {
        try
        {
            // Kill existing process
            foreach (var proc in System.Diagnostics.Process.GetProcessesByName("Soeyi"))
            {
                proc.Kill();
                await Task.Delay(2000);
            }
            
            // Find and start
            var path = FindSoeyiPath();
            if (path != null)
            {
                var root = Directory.GetParent(path)?.FullName;
                if (root != null)
                {
                    // Check standard locations inside root
                    string[] possibilities = { "Soeyi.exe", "bin/Release/Soeyi.exe" };
                    foreach (var p in possibilities)
                    {
                        var fullPath = Path.Combine(root, p.Replace("/", "\\"));
                        if (File.Exists(fullPath))
                        {
                            System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                            {
                                FileName = fullPath,
                                WorkingDirectory = Path.GetDirectoryName(fullPath),
                                UseShellExecute = true
                            });
                            return true;
                        }
                    }
                }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
    
    /// <summary>
    /// Restart Mars Gaming application.
    /// </summary>
    public async Task<bool> RestartMarsGamingAsync()
    {
        try
        {
            // Kill existing process
            foreach (var proc in System.Diagnostics.Process.GetProcessesByName("MarsGaming"))
            {
                proc.Kill();
                await Task.Delay(2000);
            }
            
            // Find and start
            var path = FindMarsPath();
            if (path != null)
            {
                 var root = Directory.GetParent(path)?.FullName;
                 if (root != null)
                 {
                     string[] possibilities = { "MarsGaming.exe", "bin/Release/MarsGaming.exe", "Product/MarsGaming.exe" };
                     foreach (var p in possibilities)
                     {
                         var fullPath = Path.Combine(root, p.Replace("/", "\\"));
                         if (File.Exists(fullPath))
                         {
                             System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                             {
                                 FileName = fullPath,
                                 WorkingDirectory = Path.GetDirectoryName(fullPath),
                                 UseShellExecute = true
                             });
                             return true;
                         }
                     }
                 }
            }
            return false;
        }
        catch
        {
            return false;
        }
    }
}

public class InstallResult
{
    public bool Success { get; set; }
    public List<string> InstalledThemes { get; set; } = new();
    public string? Message { get; set; }
    public string? Error { get; set; }
}
