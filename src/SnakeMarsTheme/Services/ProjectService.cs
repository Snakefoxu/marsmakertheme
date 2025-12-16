using System.IO;
using System.Text.Json;
using SnakeMarsTheme.Models;

namespace SnakeMarsTheme.Services;

/// <summary>
/// Service for saving and loading theme projects (.smtproj files).
/// </summary>
public class ProjectService
{
    private readonly string _projectsPath;
    private static readonly JsonSerializerOptions _jsonOptions = new()
    {
        WriteIndented = true,
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase
    };

    public const string ProjectExtension = ".smtproj";
    public const string ProjectFilter = "SnakeMars Theme Project|*.smtproj|Todos los archivos|*.*";

    public ProjectService(string basePath)
    {
        _projectsPath = Path.Combine(basePath, "resources", "Projects");
        
        if (!Directory.Exists(_projectsPath))
            Directory.CreateDirectory(_projectsPath);
    }

    /// <summary>
    /// Get the default projects folder path.
    /// </summary>
    public string GetProjectsFolder() => _projectsPath;

    /// <summary>
    /// Save a project to a file.
    /// </summary>
    public ProjectSaveResult SaveProject(ThemeProject project, string filePath)
    {
        try
        {
            // Update modification time
            project.ModifiedAt = DateTime.Now;
            
            // Ensure extension
            if (!filePath.EndsWith(ProjectExtension, StringComparison.OrdinalIgnoreCase))
                filePath += ProjectExtension;
            
            // Serialize and save
            var json = JsonSerializer.Serialize(project, _jsonOptions);
            File.WriteAllText(filePath, json);
            
            return new ProjectSaveResult
            {
                Success = true,
                FilePath = filePath,
                Message = $"Proyecto guardado: {Path.GetFileName(filePath)}"
            };
        }
        catch (Exception ex)
        {
            return new ProjectSaveResult
            {
                Success = false,
                Error = $"Error al guardar: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Load a project from a file.
    /// </summary>
    public ProjectLoadResult LoadProject(string filePath)
    {
        try
        {
            if (!File.Exists(filePath))
            {
                return new ProjectLoadResult
                {
                    Success = false,
                    Error = $"Archivo no encontrado: {filePath}"
                };
            }
            
            var json = File.ReadAllText(filePath);
            var project = JsonSerializer.Deserialize<ThemeProject>(json, _jsonOptions);
            
            if (project == null)
            {
                return new ProjectLoadResult
                {
                    Success = false,
                    Error = "No se pudo deserializar el proyecto"
                };
            }
            
            return new ProjectLoadResult
            {
                Success = true,
                Project = project,
                FilePath = filePath
            };
        }
        catch (Exception ex)
        {
            return new ProjectLoadResult
            {
                Success = false,
                Error = $"Error al cargar: {ex.Message}"
            };
        }
    }

    /// <summary>
    /// Get list of recent projects in the projects folder.
    /// </summary>
    public List<ProjectInfo> GetRecentProjects(int maxCount = 10)
    {
        var projects = new List<ProjectInfo>();
        
        try
        {
            var files = Directory.GetFiles(_projectsPath, $"*{ProjectExtension}")
                .Select(f => new FileInfo(f))
                .OrderByDescending(f => f.LastWriteTime)
                .Take(maxCount);
            
            foreach (var file in files)
            {
                projects.Add(new ProjectInfo
                {
                    Name = Path.GetFileNameWithoutExtension(file.Name),
                    FilePath = file.FullName,
                    ModifiedAt = file.LastWriteTime,
                    SizeBytes = file.Length
                });
            }
        }
        catch { /* Ignore errors */ }
        
        return projects;
    }
}

public class ProjectSaveResult
{
    public bool Success { get; set; }
    public string? FilePath { get; set; }
    public string? Message { get; set; }
    public string? Error { get; set; }
}

public class ProjectLoadResult
{
    public bool Success { get; set; }
    public ThemeProject? Project { get; set; }
    public string? FilePath { get; set; }
    public string? Error { get; set; }
}

public class ProjectInfo
{
    public string Name { get; set; } = "";
    public string FilePath { get; set; } = "";
    public DateTime ModifiedAt { get; set; }
    public long SizeBytes { get; set; }
    
    public string SizeFormatted => SizeBytes < 1024 
        ? $"{SizeBytes} B" 
        : $"{SizeBytes / 1024.0:F1} KB";
    
    public string ModifiedFormatted => ModifiedAt.ToString("dd/MM/yyyy HH:mm");
}
