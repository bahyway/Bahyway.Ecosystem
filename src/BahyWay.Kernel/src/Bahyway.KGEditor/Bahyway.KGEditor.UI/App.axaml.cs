using Avalonia;
using Avalonia.Controls.ApplicationLifetimes;
using Avalonia.Markup.Xaml;
using Bahyway.KGEditor.UI.ViewModels;
using Bahyway.KGEditor.UI.Views;
using AvaloniaWebView;  // ← ADD THIS LINE

namespace Bahyway.KGEditor.UI
{
    public partial class App : Application
    {
        public override void Initialize()
        {
            AvaloniaXamlLoader.Load(this);
            // ← ADD THIS LINE - Initialize WebView
            AvaloniaWebViewBuilder.Initialize(default);
        }

        public override void OnFrameworkInitializationCompleted()
        {
            if (ApplicationLifetime is IClassicDesktopStyleApplicationLifetime desktop)
            {
                desktop.MainWindow = new MainWindow
                {
                    DataContext = new MainWindowViewModel(),
                };
            }

            base.OnFrameworkInitializationCompleted();
        }
    }
}
