using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.WebView.Desktop;  // ← Add this

namespace Bahyway.ShoWay.UI
{
    class Program
    {
        public static void Main(string[] args) => BuildAvaloniaApp()
            .StartWithClassicDesktopLifetime(args);

        public static AppBuilder BuildAvaloniaApp()
            => AppBuilder.Configure<App>()
                .UsePlatformDetect()
                .UseDesktopWebView()  // ← Add this
                .LogToTrace();
    }
}