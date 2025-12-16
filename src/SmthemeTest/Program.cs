using System;
using System.IO;
using System.Drawing;
using System.Drawing.Imaging;
using SnakeMarsTheme.Services;

namespace SmthemeTest
{
    class Program
    {
        static void Main(string[] args)
        {
            Console.WriteLine("=== Batch Convert TURZX Themes to .smtheme ===\n");

            var packager = new SmthemePackagerService();
            string turzxFolder = @"d:\REPOS_GITHUB\SnakeMarsTheme\resources\Themes_TURZX";
            string previewsFolder = @"d:\REPOS_GITHUB\SnakeMarsTheme\resources\Previews\TURZX";
            string outputFolder = @"d:\REPOS_GITHUB\SnakeMarsTheme\resources\Themes_SMTHEME";

            Directory.CreateDirectory(outputFolder);

            // Buscar todos los .turtheme
            var turthemeFiles = Directory.GetFiles(turzxFolder, "*.turtheme", SearchOption.AllDirectories);
            int total = turthemeFiles.Length;
            int success = 0;
            int errors = 0;
            int skipped = 0;

            foreach (var turthemePath in turthemeFiles)
            {
                string themeName = Path.GetFileNameWithoutExtension(turthemePath);
                string resolution = Path.GetFileName(Path.GetDirectoryName(turthemePath));
                string outputName = $"TURZX_{themeName}_{resolution}.smtheme";
                string outputPath = Path.Combine(outputFolder, outputName);

                // Skip if already exists
                if (File.Exists(outputPath))
                {
                    skipped++;
                    continue;
                }

                // Buscar preview correspondiente
                string previewPath = Path.Combine(previewsFolder, $"{themeName}.png");
                if (!File.Exists(previewPath))
                {
                    Console.WriteLine($"[SKIP] {themeName} - sin preview");
                    skipped++;
                    continue;
                }

                try
                {
                    // Crear carpeta temporal con los archivos
                    string tempFolder = Path.Combine(Path.GetTempPath(), $"turzx_temp_{Guid.NewGuid()}");
                    Directory.CreateDirectory(tempFolder);

                    // Copiar preview como background
                    File.Copy(previewPath, Path.Combine(tempFolder, "background.png"));

                    // Crear manifest
                    var manifest = new SmthemePackagerService.ThemeManifest
                    {
                        Name = themeName,
                        Author = "TURZX",
                        Source = "turzx",
                        Description = $"Tema TURZX - Resolucion {resolution}"
                    };

                    packager.PackTheme(tempFolder, outputPath, manifest);
                    Console.WriteLine($"[OK] {themeName} ({resolution})");
                    success++;

                    // Limpiar temp
                    Directory.Delete(tempFolder, true);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[ERR] {themeName}: {ex.Message}");
                    errors++;
                }
            }

            Console.WriteLine($"\n=== RESUMEN ===");
            Console.WriteLine($"Total .turtheme: {total}");
            Console.WriteLine($"Convertidos: {success}");
            Console.WriteLine($"Omitidos: {skipped}");
            Console.WriteLine($"Errores: {errors}");
        }
    }
}
