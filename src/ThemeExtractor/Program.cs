using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using System.Reflection;
using System.Drawing;
using System.Collections;

namespace ThemeExtractor
{
    class Program
    {
        static int successCount = 0;
        static int errorCount = 0;

        static void Main(string[] args)
        {
            // Resolver ensamblados manualmente
            AppDomain.CurrentDomain.AssemblyResolve += (sender, resultArgs) =>
            {
                string shortName = new AssemblyName(resultArgs.Name).Name;
                if (shortName.Equals("UsbMonitorL", StringComparison.OrdinalIgnoreCase))
                {
                    string turzxPath = Path.GetFullPath("TURZX.exe");
                    if (File.Exists(turzxPath))
                        return Assembly.LoadFrom(turzxPath);
                }
                return null;
            };

            // Pre-cargar ensamblado
            try 
            {
                Assembly.LoadFrom("TURZX.exe");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error cargando TURZX.exe: {ex.Message}");
                return;
            }

            // Modo batch: procesar carpeta completa
            if (args.Length >= 2)
            {
                string inputFolder = args[0];
                string outputFolder = args[1];
                
                Console.WriteLine($"Modo Batch: {inputFolder} -> {outputFolder}");
                ProcessFolder(inputFolder, outputFolder);
            }
            // Modo single file
            else if (args.Length == 1)
            {
                string themePath = args[0];
                string outputDir = Path.Combine(Path.GetDirectoryName(themePath), "extracted");
                ExtractTheme(themePath, outputDir);
            }
            else
            {
                Console.WriteLine("Uso:");
                Console.WriteLine("  ThemeExtractor.exe <carpeta_temas> <carpeta_previews>  (batch)");
                Console.WriteLine("  ThemeExtractor.exe <archivo.turtheme>                   (single)");
            }
            
            Console.WriteLine($"\nResumen: {successCount} exitosos, {errorCount} errores");
        }

        static void ProcessFolder(string inputFolder, string outputFolder)
        {
            Directory.CreateDirectory(outputFolder);
            
            // Buscar .turtheme en subcarpetas (por resolución)
            var themes = Directory.GetFiles(inputFolder, "*.turtheme", SearchOption.AllDirectories);
            Console.WriteLine($"Encontrados {themes.Length} temas");
            
            foreach (var themePath in themes)
            {
                string themeName = Path.GetFileNameWithoutExtension(themePath);
                string previewPath = Path.Combine(outputFolder, $"{themeName}.png");
                
                if (File.Exists(previewPath))
                {
                    Console.WriteLine($"[SKIP] {themeName} (ya existe)");
                    continue;
                }
                
                ExtractPreviewOnly(themePath, previewPath);
            }
        }

        static void ExtractPreviewOnly(string themePath, string previewPath)
        {
            try
            {
                var formatter = new BinaryFormatter();
                formatter.Binder = new AllowAllAssemblyVersionsDeserializationBinder();

                using (var stream = File.OpenRead(themePath))
                {
                    object themeObj = formatter.Deserialize(stream);
                    
                    // Buscar themePic (preview del tema)
                    var field = themeObj.GetType().GetField("themePic", 
                        BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance);
                    
                    if (field != null)
                    {
                        var bmp = field.GetValue(themeObj) as Bitmap;
                        if (bmp != null)
                        {
                            bmp.Save(previewPath, System.Drawing.Imaging.ImageFormat.Png);
                            Console.WriteLine($"[OK] {Path.GetFileNameWithoutExtension(themePath)}");
                            successCount++;
                            return;
                        }
                    }
                    
                    // Si no hay themePic, buscar cualquier bitmap
                    foreach (var f in themeObj.GetType().GetFields(BindingFlags.NonPublic | BindingFlags.Instance))
                    {
                        var val = f.GetValue(themeObj);
                        if (val is Bitmap bmp)
                        {
                            bmp.Save(previewPath, System.Drawing.Imaging.ImageFormat.Png);
                            Console.WriteLine($"[OK] {Path.GetFileNameWithoutExtension(themePath)} (from {f.Name})");
                            successCount++;
                            return;
                        }
                    }
                    
                    Console.WriteLine($"[WARN] {Path.GetFileNameWithoutExtension(themePath)} - No preview found");
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ERR] {Path.GetFileNameWithoutExtension(themePath)}: {ex.Message}");
                errorCount++;
            }
        }

        static void ExtractTheme(string themePath, string outputDir)
        {
            // Implementación original completa...
            Console.WriteLine($"Extrayendo {themePath} a {outputDir}");
        }
    }

    sealed class AllowAllAssemblyVersionsDeserializationBinder : System.Runtime.Serialization.SerializationBinder
    {
        public override Type BindToType(string assemblyName, string typeName)
        {
            if (assemblyName.IndexOf("UsbMonitorL", StringComparison.OrdinalIgnoreCase) >= 0)
            {
                foreach (var asm in AppDomain.CurrentDomain.GetAssemblies())
                {
                    if (asm.GetName().Name == "UsbMonitorL")
                        return asm.GetType(typeName);
                }
            }
            return Type.GetType($"{typeName}, {assemblyName}");
        }
    }
}
