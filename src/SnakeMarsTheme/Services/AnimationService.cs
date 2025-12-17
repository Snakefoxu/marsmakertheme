using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Imaging;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Xabe.FFmpeg;
using Xabe.FFmpeg.Downloader;

namespace SnakeMarsTheme.Services
{
    /// <summary>
    /// Servicio para extraer frames de archivos animados (GIF, Video)
    /// y convertirlos a secuencias PNG para temas de Mars Gaming/SOEYI.
    /// </summary>
    public class AnimationService
    {
        private readonly string _tempFramesFolder;
        private const int MAX_FRAMES = 60; // Limitación de hardware SOEYI/Mars
        private static bool _ffmpegInitialized = false;
        
        public AnimationService()
        {
            // Determinar si es modo portable o instalado
            var exePath = System.Reflection.Assembly.GetExecutingAssembly().Location;
            var appDir = Path.GetDirectoryName(exePath) ?? "";
            var portableMarker = Path.Combine(appDir, "portable.txt");
            
            if (File.Exists(portableMarker) || appDir.Contains("bin"))
            {
                // Modo portable: guardar junto a la app
                _tempFramesFolder = Path.Combine(appDir, "ConvertedGifs");
            }
            else
            {
                // Modo instalado: guardar en Documents
                var docsPath = Environment.GetFolderPath(Environment.SpecialFolder.MyDocuments);
                _tempFramesFolder = Path.Combine(docsPath, "SnakeMarsTheme", "ConvertedGifs");
            }
            
            Directory.CreateDirectory(_tempFramesFolder);
        }
        
        /// <summary>
        /// Inicializa FFmpeg descargándolo automáticamente si no está disponible.
        /// </summary>
        private async Task EnsureFFmpegReady()
        {
            if (_ffmpegInitialized) return;
            
            try
            {
                // Intentar usar FFmpeg del sistema primero
                var ffmpegPath = FFmpeg.ExecutablesPath;
                if (File.Exists(Path.Combine(ffmpegPath ?? "", "ffmpeg.exe")))
                {
                    _ffmpegInitialized = true;
                    return;
                }
            }
            catch { }
            
            try
            {
                // Descargar FFmpeg automáticamente a carpeta local
                var appDataPath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                    "SnakeMarsTheme", "FFmpeg");
                
                Directory.CreateDirectory(appDataPath);
                FFmpeg.SetExecutablesPath(appDataPath);
                
                // Verificar si ya está descargado
                if (!File.Exists(Path.Combine(appDataPath, "ffmpeg.exe")))
                {
                    // Descargar binarios de FFmpeg (~80MB)
                    await FFmpegDownloader.GetLatestVersion(FFmpegVersion.Official, appDataPath);
                }
                
                _ffmpegInitialized = true;
            }
            catch (Exception ex)
            {
                throw new Exception($"No se pudo inicializar FFmpeg: {ex.Message}", ex);
            }
        }
        
        /// <summary>
        /// Extrae frames de un archivo GIF animado.
        /// </summary>
        /// <param name="gifPath">Ruta al archivo GIF</param>
        /// <param name="outputFolder">Carpeta donde guardar los frames</param>
        /// <returns>Lista de rutas a los frames extraídos (1.png, 2.png, ...)</returns>
        public async Task<List<string>> ExtractGifFrames(string gifPath, string outputFolder)
        {
            if (!File.Exists(gifPath))
                throw new FileNotFoundException($"GIF no encontrado: {gifPath}");
            
            var framePaths = new List<string>();
            
            try
            {
                using (var gifImage = Image.FromFile(gifPath))
                {
                    var dimension = new FrameDimension(gifImage.FrameDimensionsList[0]);
                    int frameCount = gifImage.GetFrameCount(dimension);
                    
                    // Limitar a MAX_FRAMES
                    frameCount = Math.Min(frameCount, MAX_FRAMES);
                    
                    for (int i = 0; i < frameCount; i++)
                    {
                        gifImage.SelectActiveFrame(dimension, i);
                        
                        string framePath = Path.Combine(outputFolder, $"{i + 1}.png");
                        gifImage.Save(framePath, ImageFormat.Png);
                        
                        framePaths.Add(framePath);
                    }
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al extraer frames del GIF: {ex.Message}", ex);
            }
            
            return await Task.FromResult(framePaths);
        }
        
        /// <summary>
        /// Extrae frames de un archivo de video (MP4, AVI, WMV, etc.)
        /// </summary>
        /// <param name="videoPath">Ruta al archivo de video</param>
        /// <param name="outputFolder">Carpeta donde guardar los frames</param>
        /// <param name="fps">Frames por segundo a extraer (default: 10)</param>
        /// <returns>Lista de rutas a los frames extraídos</returns>
        public async Task<List<string>> ExtractVideoFrames(string videoPath, string outputFolder, int fps = 10)
        {
            if (!File.Exists(videoPath))
                throw new FileNotFoundException($"Video no encontrado: {videoPath}");
            
            // Asegurar que FFmpeg esté listo
            await EnsureFFmpegReady();
            
            var framePaths = new List<string>();
            
            try
            {
                var mediaInfo = await FFmpeg.GetMediaInfo(videoPath);
                var videoStream = mediaInfo.VideoStreams.FirstOrDefault();
                
                if (videoStream == null)
                    throw new Exception("No se encontró stream de video en el archivo");
                
                double duration = mediaInfo.Duration.TotalSeconds;
                int totalFrames = (int)(duration * fps);
                
                // Limitar a MAX_FRAMES
                totalFrames = Math.Min(totalFrames, MAX_FRAMES);
                
                // Calcular intervalo entre frames
                double interval = duration / totalFrames;
                
                for (int i = 0; i < totalFrames; i++)
                {
                    string framePath = Path.Combine(outputFolder, $"{i + 1}.png");
                    TimeSpan timestamp = TimeSpan.FromSeconds(i * interval);
                    
                    var conversion = await FFmpeg.Conversions.FromSnippet.Snapshot(
                        videoPath, 
                        framePath, 
                        timestamp);
                    
                    await conversion.Start();
                    
                    framePaths.Add(framePath);
                }
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al extraer frames del video: {ex.Message}", ex);
            }
            
            return framePaths;
        }
        
        /// <summary>
        /// Valida una secuencia de frames numerados (1.png, 2.png, ...)
        /// </summary>
        /// <param name="framePaths">Lista de rutas a frames</param>
        /// <returns>Mensaje de validación</returns>
        public (bool IsValid, string Message) ValidateFrameSequence(List<string> framePaths)
        {
            if (framePaths == null || framePaths.Count == 0)
                return (false, "No hay frames para validar");
            
            if (framePaths.Count > MAX_FRAMES)
                return (false, $"Demasiados frames ({framePaths.Count}). Máximo permitido: {MAX_FRAMES}");
            
            // Verificar que todos los archivos existen
            foreach (var path in framePaths)
            {
                if (!File.Exists(path))
                    return (false, $"Frame no encontrado: {Path.GetFileName(path)}");
            }
            
            // Verificar numeración consecutiva
            var expectedNames = Enumerable.Range(1, framePaths.Count)
                .Select(i => $"{i}.png")
                .ToList();
            
            var actualNames = framePaths.Select(p => Path.GetFileName(p)).ToList();
            
            if (!expectedNames.SequenceEqual(actualNames))
            {
                return (false, "Los frames deben estar numerados consecutivamente (1.png, 2.png, ...)");
            }
            
            return (true, $"✓ Secuencia válida: {framePaths.Count} frames");
        }
        
        /// <summary>
        /// Limpia la carpeta temporal de frames.
        /// </summary>
        public void CleanupTempFrames()
        {
            try
            {
                if (Directory.Exists(_tempFramesFolder))
                {
                    Directory.Delete(_tempFramesFolder, recursive: true);
                    Directory.CreateDirectory(_tempFramesFolder);
                }
            }
            catch
            {
                // Ignorar errores de limpieza
            }
        }
        
        /// <summary>
        /// Obtiene la ruta de la carpeta temporal de frames.
        /// </summary>
        public string GetTempFramesFolder() => _tempFramesFolder;
        
        /// <summary>
        /// Convierte un archivo de video a GIF animado usando FFmpeg.
        /// </summary>
        /// <param name="videoPath">Ruta al archivo de video</param>
        /// <param name="fps">Frames por segundo del GIF (default: 15)</param>
        /// <param name="width">Ancho del GIF en pixeles (default: 360 para Mars Gaming)</param>
        /// <returns>Ruta al archivo GIF generado</returns>
        public async Task<string> ConvertVideoToGif(string videoPath, int fps = 15, int width = 360)
        {
            if (!File.Exists(videoPath))
                throw new FileNotFoundException($"Video no encontrado: {videoPath}");
            
            await EnsureFFmpegReady();
            
            // Crear nombre único para el GIF
            var videoName = Path.GetFileNameWithoutExtension(videoPath);
            var gifPath = Path.Combine(_tempFramesFolder, $"{videoName}_{DateTime.Now:yyyyMMddHHmmss}.gif");
            
            try
            {
                // Obtener path de FFmpeg
                var ffmpegPath = FFmpeg.ExecutablesPath;
                var ffmpegExe = Path.Combine(ffmpegPath ?? "", "ffmpeg.exe");
                
                if (!File.Exists(ffmpegExe))
                {
                    // Fallback to LocalAppData
                    ffmpegPath = Path.Combine(
                        Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                        "SnakeMarsTheme", "FFmpeg");
                    ffmpegExe = Path.Combine(ffmpegPath, "ffmpeg.exe");
                }
                
                if (!File.Exists(ffmpegExe))
                    throw new Exception("FFmpeg no encontrado");
                
                // Ejecutar FFmpeg directamente como proceso
                // -t 5 = solo 5 segundos del video (evita GIFs enormes)
                // fps=10 = 10 frames por segundo (suficiente para preview)
                var args = $"-i \"{videoPath}\" -t 5 -vf \"fps=10,scale={width}:-1:flags=lanczos\" -y \"{gifPath}\"";
                
                var process = new System.Diagnostics.Process
                {
                    StartInfo = new System.Diagnostics.ProcessStartInfo
                    {
                        FileName = ffmpegExe,
                        Arguments = args,
                        UseShellExecute = false,
                        CreateNoWindow = true,
                        RedirectStandardError = true,
                        RedirectStandardOutput = true
                    }
                };
                
                process.Start();
                
                // Timeout de 120 segundos para videos complejos
                var completed = await Task.Run(() => process.WaitForExit(120000));
                if (!completed)
                {
                    process.Kill();
                    throw new Exception("Timeout: La conversión tardó más de 2 minutos.\nEste video es demasiado complejo para convertir automáticamente.");
                }
                
                if (!File.Exists(gifPath))
                {
                    var error = await process.StandardError.ReadToEndAsync();
                    throw new Exception($"FFmpeg falló: {error}");
                }
                
                return gifPath;
            }
            catch (Exception ex)
            {
                throw new Exception($"Error al convertir video a GIF: {ex.Message}", ex);
            }
        }
    }
}
