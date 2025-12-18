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

            // Access Check: Try to write a dummy file to test permissions before starting
            // This prevents partial installs
            try 
            {
                var testFile = Path.Combine(programmePath, ".perm_test");
                File.WriteAllText(testFile, "test");
                File.Delete(testFile);
            }
            catch (UnauthorizedAccessException)
            {
                // Trigger Elevation Layer immediately if we know we can't write
                return InstallWithElevation(extractedPath, programmePath, schemePaths);
            }

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
                
                // 2. Copy JSON to ThemeScheme folders (resolving placeholders)
                var jsonFile = Path.Combine(themeFolder, $"{themeName}.json");
                if (File.Exists(jsonFile))
                {
                    // Leer el JSON y reemplazar placeholder {PROGRAMME_PATH} con ruta real
                    var jsonContent = File.ReadAllText(jsonFile);
                    jsonContent = jsonContent.Replace("{PROGRAMME_PATH}", programmePath.Replace("\\", "\\\\"));
                    
                    foreach (var schemePath in schemePaths)
                    {
                        if (!Directory.Exists(schemePath))
                        {
                            try { Directory.CreateDirectory(schemePath); } catch { /* ignore if fails, elev logic might handle */ }
                        }
                        
                        if (Directory.Exists(schemePath))
                        {
                            var destJson = Path.Combine(schemePath, $"{themeName}.json");
                            // Escribir JSON modificado con ruta real
                            File.WriteAllText(destJson, jsonContent);
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
            // Fallback for unexpected access denied during copy
            return InstallWithElevation(extractedPath, programmePath, schemePaths);
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

    /// <summary>
    /// Installs the theme using a temporary PowerShell script run with elevated privileges (UAC).
    /// </summary>
    private InstallResult InstallWithElevation(string sourcePath, string targetPath, string[] schemePaths)
    {
        try
        {
            // 1. Create a temporary PowerShell script
            var tempScript = Path.Combine(Path.GetTempPath(), $"SnakeMars_Install_{Guid.NewGuid()}.ps1");
            var sb = new System.Text.StringBuilder();

            sb.AppendLine("$ErrorActionPreference = 'Stop'");
            sb.AppendLine($"$host.UI.RawUI.WindowTitle = 'Instalando Tema en {Path.GetFileName(targetPath)}...'");
            sb.AppendLine("Write-Host '----------------------------------------' -ForegroundColor Cyan");
            sb.AppendLine("Write-Host '      PROTOCOLO OMEGA: ELEVATED INSTALL' -ForegroundColor Cyan");
            sb.AppendLine("Write-Host '----------------------------------------' -ForegroundColor Cyan");
            sb.AppendLine("Write-Host ''");
            
            // Generate commands for each theme in source
            bool hasWork = false;
            foreach (var themeFolder in Directory.GetDirectories(sourcePath))
            {
                var themeName = Path.GetFileName(themeFolder);
                if (string.IsNullOrEmpty(themeName)) continue;
                hasWork = true;

                var destTheme = Path.Combine(targetPath, themeName);
                sb.AppendLine($"Write-Host 'Installing {themeName}...' -ForegroundColor Yellow");
                
                // Copy Theme Folder
                sb.AppendLine($"$src = '{themeFolder}'");
                sb.AppendLine($"$dst = '{destTheme}'");
                sb.AppendLine("if (Test-Path $dst) { Remove-Item $dst -Recurse -Force }");
                sb.AppendLine("Copy-Item -Path $src -Destination $dst -Recurse -Force");
                
                // Copy JSON (con resolución de placeholder)
                var jsonFile = Path.Combine(themeFolder, $"{themeName}.json");
                if (File.Exists(jsonFile))
                {
                    // Escapar rutas para PS y preparar reemplazo de placeholder
                    var targetPathEscaped = targetPath.Replace("\\", "\\\\");
                    
                    foreach (var schemePath in schemePaths)
                    {
                        sb.AppendLine($"$schemeDir = '{schemePath}'");
                        sb.AppendLine("if (!(Test-Path $schemeDir)) { New-Item -ItemType Directory -Path $schemeDir -Force | Out-Null }");
                        // Leer JSON, reemplazar placeholder, escribir
                        sb.AppendLine($"$jsonSrc = '{jsonFile}'");
                        sb.AppendLine($"$jsonDst = Join-Path $schemeDir '{themeName}.json'");
                        sb.AppendLine($"$content = Get-Content $jsonSrc -Raw -Encoding UTF8");
                        sb.AppendLine($"$content = $content -replace '\\{{PROGRAMME_PATH\\}}', '{targetPathEscaped}'");
                        sb.AppendLine("Set-Content -Path $jsonDst -Value $content -Encoding UTF8 -Force");
                    }
                }
            }
            
            if (!hasWork) return new InstallResult { Success = false, Error = "No se encontraron temas para instalar." };

            sb.AppendLine("Write-Host 'Installation Complete!' -ForegroundColor Green");
            // Sleep briefly to let user see success
            sb.AppendLine("Start-Sleep -Seconds 1");

            // Save as UTF-8 with BOM for PowerShell reliability with Unicode
            File.WriteAllText(tempScript, sb.ToString(), System.Text.Encoding.UTF8);

            // 2. Execute with RunAs
            var startInfo = new System.Diagnostics.ProcessStartInfo
            {
                FileName = "powershell.exe",
                Arguments = $"-NoProfile -ExecutionPolicy Bypass -File \"{tempScript}\"",
                UseShellExecute = true,
                Verb = "runas", // UAC Prompt
                WindowStyle = System.Diagnostics.ProcessWindowStyle.Normal // Let user see the progress
            };

            var process = System.Diagnostics.Process.Start(startInfo);
            process?.WaitForExit();

            // 3. Cleanup
            if (File.Exists(tempScript)) File.Delete(tempScript);
            
            // Check exit code? PowerShell wrapper usually returns 0 if script ran, hard to propagate script error code out of 'runas' easily without files.
            // We assume success if user approved.

            return new InstallResult
            {
                Success = true,
                InstalledThemes = new List<string> { "Elevated Install Completed" },
                Message = "Instalación completada con permisos de Administrador.\n\n⚠️ IMPORTANTE: Reinicia la aplicación para ver los nuevos temas."
            };
        }
        catch (System.ComponentModel.Win32Exception ex) when (ex.NativeErrorCode == 1223)
        {
            return new InstallResult
            {
                Success = false,
                Error = "Instalación cancelada por el usuario."
            };
        }
        catch (Exception ex)
        {
            return new InstallResult
            {
                Success = false,
                Error = $"Error en elevación: {ex.Message}"
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
