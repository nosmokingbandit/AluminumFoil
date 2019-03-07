using Avalonia;
using Avalonia.Logging.Serilog;
using System.Runtime.InteropServices;

namespace AluminumFoil.Mac
{
    class Program
    {
        static void Main(string[] args)
        {
            BuildAvaloniaApp().Start<Views.MainWindow>(() => new ViewModels.MainWindow());
        }

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseReactiveUI()
                .LogToDebug();
    }
}