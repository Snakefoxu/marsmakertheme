using System.Diagnostics;
using System.IO;

namespace SnakeMarsTheme.Services;

/// <summary>
/// Service for extracting .photo files (password-protected 7z archives).
/// </summary>
public class ExtractionService
{
    private const string PHOTO_PASSWORD = "vmax2025";
    private readonly string _extractedPath;
    private readonly string _7zPath;
    
    public ExtractionService(string basePath)
    {
        _extractedPath = Path.Combine(basePath, "resources", "ExtractedThemes");
        _7zPath = @"C:\Program Files\7-Zip\7z.exe";
        
        if (!Directory.Exists(_extractedPath))
            Directory.CreateDirectory(_extractedPath);
    }
    
    /// <summary>
    /// Check if 7-Zip is installed.
    /// </summary>
    public bool Is7ZipAvailable()
    {
        return File.Exists(_7zPath);
    }
    
    /// <summary>
    /// Extract a .photo file to the ExtractedThemes folder.
    /// </summary>
    public async Task<ExtractionResult> ExtractPhotoAsync(string photoPath)
    {
        if (!Is7ZipAvailable())
        {
            return new ExtractionResult
            {
                Success = false,
                Error = "7-Zip no está instalado. Descárgalo de https://7-zip.org"
            };
        }
        
        if (!File.Exists(photoPath))
        {
            return new ExtractionResult
            {
                Success = false,
                Error = $"Archivo no encontrado: {photoPath}"
            };
        }
        
        var themeName = Path.GetFileNameWithoutExtension(photoPath);
        var destPath = Path.Combine(_extractedPath, themeName);
        
        // Clean destination if exists
        if (Directory.Exists(destPath))
        {
            try { Directory.Delete(destPath, true); }
            catch { /* Ignore */ }
        }
        
        try
        {
            var startInfo = new ProcessStartInfo
            {
                FileName = _7zPath,
                Arguments = $"x \"{photoPath}\" -o\"{destPath}\" \"-p{PHOTO_PASSWORD}\" -y",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            
            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return new ExtractionResult
                {
                    Success = false,
                    Error = "No se pudo iniciar 7-Zip"
                };
            }
            
            await process.WaitForExitAsync();
            
            if (process.ExitCode == 0 && Directory.Exists(destPath))
            {
                var folders = Directory.GetDirectories(destPath);
                return new ExtractionResult
                {
                    Success = true,
                    ExtractedPath = destPath,
                    ThemeFolders = folders.Select(Path.GetFileName).Where(n => n != null).ToList()!
                };
            }
            else
            {
                var error = await process.StandardError.ReadToEndAsync();
                return new ExtractionResult
                {
                    Success = false,
                    Error = $"Error de extracción: {error}"
                };
            }
        }
        catch (Exception ex)
        {
            return new ExtractionResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }
    
    /// <summary>
    /// Get the path where a theme would be extracted.
    /// </summary>
    public string GetExtractedPath(string photoPath)
    {
        var themeName = Path.GetFileNameWithoutExtension(photoPath);
        return Path.Combine(_extractedPath, themeName);
    }
    
    /// <summary>
    /// Check if a .photo file has already been extracted.
    /// </summary>
    public bool IsExtracted(string photoPath)
    {
        var destPath = GetExtractedPath(photoPath);
        return Directory.Exists(destPath) && Directory.GetDirectories(destPath).Length > 0;
    }
    
    /// <summary>
    /// Create a .photo file from a folder (password-protected 7z archive).
    /// </summary>
    public async Task<PhotoCreationResult> CreatePhotoAsync(string sourceFolderPath, string outputPhotoPath)
    {
        if (!Is7ZipAvailable())
        {
            return new PhotoCreationResult
            {
                Success = false,
                Error = "7-Zip no está instalado. Descárgalo de https://7-zip.org"
            };
        }
        
        if (!Directory.Exists(sourceFolderPath))
        {
            return new PhotoCreationResult
            {
                Success = false,
                Error = $"Carpeta no encontrada: {sourceFolderPath}"
            };
        }
        
        // Ensure output has .photo extension
        if (!outputPhotoPath.EndsWith(".photo", StringComparison.OrdinalIgnoreCase))
        {
            outputPhotoPath += ".photo";
        }
        
        // Delete existing file
        if (File.Exists(outputPhotoPath))
        {
            try { File.Delete(outputPhotoPath); }
            catch { /* Ignore */ }
        }
        
        try
        {
            // 7z a -t7z "output.photo" "folderPath\*" -p"password" -mhe=on
            var startInfo = new ProcessStartInfo
            {
                FileName = _7zPath,
                Arguments = $"a -t7z \"{outputPhotoPath}\" \"{sourceFolderPath}\\*\" \"-p{PHOTO_PASSWORD}\" -mhe=on",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };
            
            using var process = Process.Start(startInfo);
            if (process == null)
            {
                return new PhotoCreationResult
                {
                    Success = false,
                    Error = "No se pudo iniciar 7-Zip"
                };
            }
            
            await process.WaitForExitAsync();
            
            if (process.ExitCode == 0 && File.Exists(outputPhotoPath))
            {
                var fileInfo = new FileInfo(outputPhotoPath);
                return new PhotoCreationResult
                {
                    Success = true,
                    OutputPath = outputPhotoPath,
                    FileSizeBytes = fileInfo.Length
                };
            }
            else
            {
                var error = await process.StandardError.ReadToEndAsync();
                return new PhotoCreationResult
                {
                    Success = false,
                    Error = $"Error de compresión: {error}"
                };
            }
        }
        catch (Exception ex)
        {
            return new PhotoCreationResult
            {
                Success = false,
                Error = ex.Message
            };
        }
    }
}

public class PhotoCreationResult
{
    public bool Success { get; set; }
    public string? OutputPath { get; set; }
    public long FileSizeBytes { get; set; }
    public string? Error { get; set; }
    
    public string FileSizeFormatted
    {
        get
        {
            if (FileSizeBytes < 1024) return $"{FileSizeBytes} B";
            if (FileSizeBytes < 1024 * 1024) return $"{FileSizeBytes / 1024.0:F1} KB";
            return $"{FileSizeBytes / (1024.0 * 1024.0):F2} MB";
        }
    }
}

public class ExtractionResult
{
    public bool Success { get; set; }
    public string? ExtractedPath { get; set; }
    public List<string> ThemeFolders { get; set; } = new();
    public string? Error { get; set; }
}
