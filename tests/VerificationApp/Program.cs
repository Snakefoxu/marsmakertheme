using System;
using System.Collections.Generic;
using System.IO;
using System.Text.Json;
using SnakeMarsTheme.Services;
using SnakeMarsTheme.Models;

namespace VerificationApp
{
    class Program
    {
        [STAThread]
        static void Main(string[] args)
        {
            Console.WriteLine("=== SnakeMarsTheme Verification ===");
            VerifyType0ThemeGeneration();
            Console.WriteLine("===================================");
        }

        static void VerifyType0ThemeGeneration()
        {
            Console.WriteLine("\n[TEST] Type 0 Theme Generation (BUG-001 Fix)");
            string tempPath = Path.Combine(Path.GetTempPath(), "SnakeMarsVerify_" + Guid.NewGuid());
            
            try
            {
                Console.WriteLine($"Working directory: {tempPath}");
                Directory.CreateDirectory(tempPath);
                
                // Initialize service
                var creator = new ThemeCreatorService(tempPath);
                
                // Create dummy background image
                string bgPath = Path.Combine(tempPath, "dummy_bg.png");
                using (var bmp = new System.Drawing.Bitmap(360, 960))
                {
                    using (var g = System.Drawing.Graphics.FromImage(bmp))
                    {
                        g.Clear(System.Drawing.Color.Blue);
                    }
                    bmp.Save(bgPath, System.Drawing.Imaging.ImageFormat.Png);
                }

                // Setup request
                var request = new ThemeSaveRequest
                {
                    ThemeName = "TestType0",
                    Width = 360,
                    Height = 960,
                    ThemeType = 0, // Type 0 = GIF/Simple
                    BackgroundPath = bgPath,
                    Widgets = new List<WidgetInfo>
                    {
                        new WidgetInfo 
                        { 
                            Name = "TestCPUTemp", 
                            Type = "CPUTemp",
                            WidgetType = SnakeMarsTheme.Services.WidgetType.Text, 
                            X = 10, Y = 10, 
                            Color = "#FFFFFF",
                            Font = "Arial",
                            FontSize = 12
                        }
                    }
                };

                Console.WriteLine("Generating theme...");
                var result = creator.SaveTheme(request);
                
                if (!result.Success)
                {
                    Console.WriteLine($"❌ FAILED: SaveTheme Error: {result.Error}");
                    return;
                }

                // Check JSON output
                // Expected path: tempPath/resources/ThemeScheme/TestType0.json
                string jsonPath = Path.Combine(tempPath, "resources", "ThemeScheme", "TestType0.json");
                
                if (!File.Exists(jsonPath))
                {
                     // Fallback check
                     var files = Directory.GetFiles(tempPath, "TestType0.json", SearchOption.AllDirectories);
                     if (files.Length > 0) jsonPath = files[0];
                     else 
                     {
                         Console.WriteLine("❌ FAILED: JSON file not found.");
                         return;
                     }
                }
                
                string jsonContent = File.ReadAllText(jsonPath);
                
                // Verification Logic
                // Must contain "DisplayTexts" with array containing the widget
                bool hasDisplayTexts = jsonContent.Contains("\"DisplayTexts\": [");
                bool hasWidget = jsonContent.Contains("TestCPUTemp") || jsonContent.Contains("\"TextType\": \"CPUTemp\"");
                
                if (hasDisplayTexts && hasWidget)
                {
                     Console.WriteLine("✅ SUCCESS: Type 0 JSON contains DisplayTexts and Widget data.");
                }
                else
                {
                     Console.WriteLine("❌ FAILED: JSON content invalid.");
                     if (!hasDisplayTexts) Console.WriteLine("- Missing 'DisplayTexts'");
                     if (!hasWidget) Console.WriteLine("- Missing widget data");
                     Console.WriteLine("Snippet:");
                     Console.WriteLine(jsonContent.Length > 300 ? jsonContent.Substring(0, 300) + "..." : jsonContent);
                }
            }
            catch (Exception ex)
            {
                Console.WriteLine($"❌ EXCEPTION: {ex}");
            }
            finally
            {
                // Cleanup
                try { 
                    if (Directory.Exists(tempPath)) Directory.Delete(tempPath, true); 
                } catch { }
            }
        }
    }
}
