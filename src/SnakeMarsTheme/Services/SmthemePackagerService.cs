using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Text.Json;
using System.Drawing;
using System.Drawing.Imaging;

namespace SnakeMarsTheme.Services
{
    /// <summary>
    /// Servicio para crear y leer archivos .smtheme (SnakeMars Theme)
    /// Formato ZIP sin contraseña, abierto y democrático
    /// </summary>
    public class SmthemePackagerService
    {
        private const string MANIFEST_FILE = "manifest.json";
        private const string BACKGROUND_FILE = "background.png";
        private const string PREVIEW_FILE = "preview.png";
        private const string SETTINGS_FILE = "settings.txt";
        private const string THEME_YAML_FILE = "theme.yaml";
        private const string FRAMES_FOLDER = "frames";
        private const int PREVIEW_WIDTH = 200;

        /// <summary>
        /// Metadatos del tema
        /// </summary>
        public class ThemeManifest
        {
            public string Name { get; set; } = "";
            public string Version { get; set; } = "1.0";
            public string Author { get; set; } = "SnakeMars";
            public string Resolution { get; set; } = "";
            public bool Animated { get; set; } = false;
            public int FrameCount { get; set; } = 0;
            public string Source { get; set; } = "custom"; // python, turzx, soeyi, custom
            public string Created { get; set; } = DateTime.Now.ToString("yyyy-MM-dd");
            public string Description { get; set; } = "";
        }

        /// <summary>
        /// Empaqueta una carpeta de tema en un archivo .smtheme
        /// </summary>
        public string PackTheme(string sourceFolder, string outputPath, ThemeManifest? manifest = null)
        {
            if (!Directory.Exists(sourceFolder))
                throw new DirectoryNotFoundException($"Carpeta no encontrada: {sourceFolder}");

            // Generar manifest si no se proporciona
            manifest ??= new ThemeManifest
            {
                Name = Path.GetFileName(sourceFolder)
            };

            // Verificar que existe background.png
            string backgroundPath = Path.Combine(sourceFolder, BACKGROUND_FILE);
            if (!File.Exists(backgroundPath))
            {
                // Buscar cualquier PNG como background
                var pngs = Directory.GetFiles(sourceFolder, "*.png");
                if (pngs.Length > 0)
                    backgroundPath = pngs[0];
                else
                    throw new FileNotFoundException("No se encontró background.png ni ningún PNG");
            }

            // Detectar resolución del background
            using (var img = Image.FromFile(backgroundPath))
            {
                manifest.Resolution = $"{img.Width}x{img.Height}";
            }

            // Crear archivo ZIP temporal
            string tempZip = Path.Combine(Path.GetTempPath(), $"smtheme_{Guid.NewGuid()}.zip");
            
            using (var archive = ZipFile.Open(tempZip, ZipArchiveMode.Create))
            {
                // Añadir manifest.json
                var manifestEntry = archive.CreateEntry(MANIFEST_FILE);
                using (var writer = new StreamWriter(manifestEntry.Open()))
                {
                    var options = new JsonSerializerOptions { WriteIndented = true };
                    writer.Write(JsonSerializer.Serialize(manifest, options));
                }

                // Añadir background.png
                archive.CreateEntryFromFile(backgroundPath, BACKGROUND_FILE);

                // Generar y añadir preview.png
                string previewPath = Path.Combine(sourceFolder, PREVIEW_FILE);
                if (!File.Exists(previewPath))
                {
                    previewPath = GeneratePreview(backgroundPath);
                }
                archive.CreateEntryFromFile(previewPath, PREVIEW_FILE);

                // Añadir settings.txt si existe
                string settingsPath = Path.Combine(sourceFolder, SETTINGS_FILE);
                if (File.Exists(settingsPath))
                {
                    archive.CreateEntryFromFile(settingsPath, SETTINGS_FILE);
                }

                // Añadir theme.yaml si existe (para temas Python)
                string yamlPath = Path.Combine(sourceFolder, THEME_YAML_FILE);
                if (File.Exists(yamlPath))
                {
                    archive.CreateEntryFromFile(yamlPath, THEME_YAML_FILE);
                    manifest.Source = "python";
                }

                // Añadir frames si es animado
                string framesPath = Path.Combine(sourceFolder, FRAMES_FOLDER);
                if (Directory.Exists(framesPath))
                {
                    var frames = Directory.GetFiles(framesPath, "*.png");
                    manifest.Animated = true;
                    manifest.FrameCount = frames.Length;
                    
                    foreach (var frame in frames)
                    {
                        string entryName = $"{FRAMES_FOLDER}/{Path.GetFileName(frame)}";
                        archive.CreateEntryFromFile(frame, entryName);
                    }
                }
            }

            // Renombrar a .smtheme
            if (File.Exists(outputPath))
                File.Delete(outputPath);
            File.Move(tempZip, outputPath);

            return outputPath;
        }

        /// <summary>
        /// Desempaqueta un archivo .smtheme a una carpeta
        /// </summary>
        public string UnpackTheme(string smthemePath, string outputFolder)
        {
            if (!File.Exists(smthemePath))
                throw new FileNotFoundException($"Archivo no encontrado: {smthemePath}");

            // Crear carpeta de salida
            Directory.CreateDirectory(outputFolder);

            // Extraer ZIP
            ZipFile.ExtractToDirectory(smthemePath, outputFolder, overwriteFiles: true);

            return outputFolder;
        }

        /// <summary>
        /// Lee el manifest de un archivo .smtheme sin extraerlo
        /// </summary>
        public ThemeManifest ReadManifest(string smthemePath)
        {
            if (!File.Exists(smthemePath))
                throw new FileNotFoundException($"Archivo no encontrado: {smthemePath}");

            using (var archive = ZipFile.OpenRead(smthemePath))
            {
                var manifestEntry = archive.GetEntry(MANIFEST_FILE);
                if (manifestEntry == null)
                    throw new InvalidDataException("El archivo no contiene manifest.json");

                using (var reader = new StreamReader(manifestEntry.Open()))
                {
                    string json = reader.ReadToEnd();
                    return JsonSerializer.Deserialize<ThemeManifest>(json);
                }
            }
        }

        /// <summary>
        /// Extrae solo el preview de un archivo .smtheme
        /// </summary>
        public byte[] ExtractPreview(string smthemePath)
        {
            using (var archive = ZipFile.OpenRead(smthemePath))
            {
                var previewEntry = archive.GetEntry(PREVIEW_FILE);
                if (previewEntry == null)
                    return null;

                using (var stream = previewEntry.Open())
                using (var ms = new MemoryStream())
                {
                    stream.CopyTo(ms);
                    return ms.ToArray();
                }
            }
        }

        /// <summary>
        /// Lista todos los archivos dentro de un .smtheme
        /// </summary>
        public List<string> ListContents(string smthemePath)
        {
            var files = new List<string>();
            
            using (var archive = ZipFile.OpenRead(smthemePath))
            {
                foreach (var entry in archive.Entries)
                {
                    files.Add(entry.FullName);
                }
            }

            return files;
        }

        /// <summary>
        /// Genera una preview redimensionada de una imagen
        /// </summary>
        private string GeneratePreview(string imagePath)
        {
            string previewPath = Path.Combine(Path.GetTempPath(), $"preview_{Guid.NewGuid()}.png");
            
            using (var original = Image.FromFile(imagePath))
            {
                int newWidth = PREVIEW_WIDTH;
                int newHeight = (int)(original.Height * ((float)PREVIEW_WIDTH / original.Width));
                
                using (var preview = new Bitmap(newWidth, newHeight))
                using (var graphics = Graphics.FromImage(preview))
                {
                    graphics.InterpolationMode = System.Drawing.Drawing2D.InterpolationMode.HighQualityBicubic;
                    graphics.DrawImage(original, 0, 0, newWidth, newHeight);
                    preview.Save(previewPath, ImageFormat.Png);
                }
            }

            return previewPath;
        }

        /// <summary>
        /// Valida que un archivo .smtheme tenga la estructura correcta
        /// </summary>
        public bool ValidateSmtheme(string smthemePath, out List<string> errors)
        {
            errors = new List<string>();

            try
            {
                using (var archive = ZipFile.OpenRead(smthemePath))
                {
                    // Verificar manifest.json
                    if (archive.GetEntry(MANIFEST_FILE) == null)
                        errors.Add("Falta manifest.json");

                    // Verificar background.png
                    if (archive.GetEntry(BACKGROUND_FILE) == null)
                        errors.Add("Falta background.png");

                    // Verificar preview.png
                    if (archive.GetEntry(PREVIEW_FILE) == null)
                        errors.Add("Falta preview.png (se generará automáticamente)");
                }
            }
            catch (Exception ex)
            {
                errors.Add($"Error al leer archivo: {ex.Message}");
            }

            return errors.Count == 0;
        }
    }
}
