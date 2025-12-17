using System;
using System.IO;
using System.Threading.Tasks;
using System.Windows;
using SnakeMarsTheme.Services;

namespace SnakeMarsTheme;

/// <summary>
/// Interaction logic for App.xaml
/// </summary>
public partial class App : Application
{
    protected override void OnStartup(StartupEventArgs e)
    {
        // 1. Inicializar sistema de rutas (Portable vs Install detection)
        PathService.Initialize();
        
        base.OnStartup(e);
        
        // Global exception handlers
        AppDomain.CurrentDomain.UnhandledException += (s, args) =>
        {
            var ex = args.ExceptionObject as Exception;
            LogAndShowError("AppDomain", ex);
        };
        
        DispatcherUnhandledException += (s, args) =>
        {
            LogAndShowError("Dispatcher", args.Exception);
            args.Handled = true; // Prevent app from closing
        };
        
        TaskScheduler.UnobservedTaskException += (s, args) =>
        {
            LogAndShowError("Task", args.Exception);
            args.SetObserved();
        };
    }
    
    private void LogAndShowError(string source, Exception? ex)
    {
        if (ex == null) return;
        
        string message = $"[{source}] {ex.GetType().Name}: {ex.Message}\n\nStack:\n{ex.StackTrace}";
        
        // Log seguro en carpeta de usuario
        try
        {
            string logDir = Path.Combine(PathService.UserDataDir, "logs");
            if (!Directory.Exists(logDir)) Directory.CreateDirectory(logDir);
            
            string logPath = Path.Combine(logDir, "error.log");
            File.AppendAllText(logPath, $"\n[{DateTime.Now}] {message}\n");
        }
        catch { }
        
        // Show to user
        MessageBox.Show(message, "Error no manejado", MessageBoxButton.OK, MessageBoxImage.Error);
    }
}
