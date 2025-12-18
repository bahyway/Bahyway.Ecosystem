# 1. SETUP LOGGING
$LogFile = "$PWD/clean_build_log.txt"
Start-Transcript -Path $LogFile -Force

Write-Host "🔥 STARTING DEEP CLEAN & REBUILD WITH LOGGING..." -ForegroundColor Cyan

# 2. SETUP PATHS
$uiPath = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI"
$projFile = "$uiPath/Bahyway.KGEditor.UI.csproj"

# ---------------------------------------------------------
# 3. DELETE CONFLICTING FOLDERS (The Fix for Duplicate Classes)
# ---------------------------------------------------------
Write-Host "🧹 Deleting old folders to remove ghost files..."
Remove-Item -Path "$uiPath/ViewModels" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "$uiPath/Models" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "$uiPath/Controls" -Recurse -Force -ErrorAction SilentlyContinue
Remove-Item -Path "$uiPath/Views" -Recurse -Force -ErrorAction SilentlyContinue

# 4. RECREATE DIRECTORY STRUCTURE
Write-Host "📂 Recreating clean folders..."
New-Item -ItemType Directory -Force -Path "$uiPath/ViewModels" | Out-Null
New-Item -ItemType Directory -Force -Path "$uiPath/Controls" | Out-Null
New-Item -ItemType Directory -Force -Path "$uiPath/Views" | Out-Null
New-Item -ItemType Directory -Force -Path "$uiPath/Assets" | Out-Null

# ---------------------------------------------------------
# 5. REGENERATE ASSETS (HTML Engine)
# ---------------------------------------------------------
$htmlPath = "$uiPath/Assets/ontoway_engine.html"
$htmlCode = @'
<!DOCTYPE html>
<html>
<head><style>body{margin:0;background:#1e1e1e;overflow:hidden;}</style></head>
<body><h1 style="color:white;font-family:sans-serif;text-align:center;margin-top:20%;">Ontoway Web Engine Loaded</h1></body>
</html>
'@
Set-Content -Path $htmlPath -Value $htmlCode

# ---------------------------------------------------------
# 6. REGENERATE VIEWMODELS (Only one copy now)
# ---------------------------------------------------------
Write-Host "📝 Generating ViewModels..."

$nodeVmPath = "$uiPath/ViewModels/NodeViewModel.cs"
$nodeVmCode = @'
using CommunityToolkit.Mvvm.ComponentModel;
using Avalonia.Media;

namespace Bahyway.KGEditor.UI.ViewModels
{
    public partial class NodeViewModel : ObservableObject
    {
        [ObservableProperty] private string _id;
        [ObservableProperty] private string _name;
        [ObservableProperty] private double _x;
        [ObservableProperty] private double _y;
        [ObservableProperty] private IBrush _color;

        public NodeViewModel(string id, string name, double x, double y, string colorHex)
        {
            _id = id; _name = name; _x = x; _y = y;
            _color = Brush.Parse(colorHex);
        }
    }
}
'@
Set-Content -Path $nodeVmPath -Value $nodeVmCode -Encoding UTF8

$mainVmPath = "$uiPath/ViewModels/MainWindowViewModel.cs"
$mainVmCode = @'
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using System.Collections.ObjectModel;

namespace Bahyway.KGEditor.UI.ViewModels
{
    public partial class MainWindowViewModel : ObservableObject
    {
        public ObservableCollection<NodeViewModel> Nodes { get; } = new();
        [ObservableProperty] private string _consoleLog = "System Ready...";

        public MainWindowViewModel()
        {
            Nodes.Add(new NodeViewModel("1", "Sensor 03", 400, 300, "#FF0000"));
            Nodes.Add(new NodeViewModel("2", "Valve A", 200, 150, "#00FF00"));
        }

        [RelayCommand]
        public void AddNode() {
            Nodes.Add(new NodeViewModel("NEW", "New Node", 100, 100, "#3498db"));
            ConsoleLog += "\n[CMD] Node Added.";
        }
    }
}
'@
Set-Content -Path $mainVmPath -Value $mainVmCode -Encoding UTF8

# ---------------------------------------------------------
# 7. REGENERATE CONTROLS
# ---------------------------------------------------------
Write-Host "📝 Generating Controls..."

$canvasPath = "$uiPath/Controls/GraphCanvas.cs"
$canvasCode = @'
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Bahyway.SharedKernel.Graph;
using System.Linq;

namespace Bahyway.KGEditor.UI.Controls
{
    public class GraphCanvas : Control
    {
        private GraphTopology? _graph;
        private readonly IPen _edgePen = new Pen(Brushes.Gray, 2);

        public void RenderGraph(GraphTopology graph) {
            _graph = graph;
            InvalidateVisual();
        }

        public override void Render(DrawingContext context) {
            if (_graph == null) return;
            context.FillRectangle(Brushes.Black, new Rect(Bounds.Size));

            foreach (var edge in _graph.Edges) {
                var s = _graph.Nodes.FirstOrDefault(n => n.Id == edge.SourceId);
                var t = _graph.Nodes.FirstOrDefault(n => n.Id == edge.TargetId);
                if (s != null && t != null) context.DrawLine(_edgePen, new Point(s.X, s.Y), new Point(t.X, t.Y));
            }
            foreach (var node in _graph.Nodes) {
                var brush = Brush.Parse(node.ColorHex);
                context.DrawEllipse(brush, null, new Point(node.X, node.Y), 20, 20);
            }
        }
    }
}
'@
Set-Content -Path $canvasPath -Value $canvasCode -Encoding UTF8

$webPath = "$uiPath/Controls/WebGraphControl.cs"
$webCode = @'
using Avalonia.Controls;
using Avalonia.WebView;
using System;
using System.IO;

namespace Bahyway.KGEditor.UI.Controls
{
    public class WebGraphControl : UserControl
    {
        private WebView _webView;
        public WebGraphControl() {
            _webView = new WebView();
            this.Content = _webView;
            string htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "ontoway_engine.html");
            if (File.Exists(htmlPath)) _webView.Source = new Uri(htmlPath);
        }
    }
}
'@
Set-Content -Path $webPath -Value $webCode -Encoding UTF8

# ---------------------------------------------------------
# 8. REGENERATE VIEWS
# ---------------------------------------------------------
Write-Host "📝 Generating Views..."

$evPath = "$uiPath/Views/EditorView.axaml"
$evXaml = @'
<UserControl xmlns="https://github.com/avaloniaui"
             xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
             x:Class="Bahyway.KGEditor.UI.Views.EditorView">
  <Grid Background="#1e1e1e"><TextBlock Text="Editor Loaded" Foreground="Gray"/></Grid>
</UserControl>
'@
Set-Content -Path $evPath -Value $evXaml -Encoding UTF8

$evCsPath = "$uiPath/Views/EditorView.axaml.cs"
$evCs = @'
using Avalonia.Controls;
namespace Bahyway.KGEditor.UI.Views {
    public partial class EditorView : UserControl {
        public EditorView() { InitializeComponent(); }
    }
}
'@
Set-Content -Path $evCsPath -Value $evCs -Encoding UTF8

$mainXamlPath = "$uiPath/Views/MainWindow.axaml"
$mainXaml = @'
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:vm="using:Bahyway.KGEditor.UI.ViewModels"
        xmlns:controls="using:Bahyway.KGEditor.UI.Controls"
        x:Class="Bahyway.KGEditor.UI.Views.MainWindow"
        x:DataType="vm:MainWindowViewModel"
        Title="Ontoway" Width="1200" Height="800">
    <Design.DataContext><vm:MainWindowViewModel/></Design.DataContext>
    <Grid ColumnDefinitions="200, *" RowDefinitions="*, 150">
        <Border Grid.Column="0" Background="#252526"><TextBlock Text="TOOLBOX" Foreground="White" Margin="10"/></Border>
        <TabControl Grid.Column="1">
            <TabItem Header="Native"><controls:GraphCanvas Name="NativeCanvas"/></TabItem>
            <TabItem Header="Web"><controls:WebGraphControl Name="WebCanvas"/></TabItem>
        </TabControl>
        <Border Grid.Row="1" Grid.ColumnSpan="2" Background="#1e1e1e">
            <TextBlock Text="{Binding ConsoleLog}" Foreground="#00FF00" Margin="10"/>
        </Border>
    </Grid>
</Window>
'@
Set-Content -Path $mainXamlPath -Value $mainXaml -Encoding UTF8

$mainCsPath = "$uiPath/Views/MainWindow.axaml.cs"
$mainCs = @'
using Avalonia.Controls;
using Bahyway.KGEditor.UI.ViewModels;
namespace Bahyway.KGEditor.UI.Views {
    public partial class MainWindow : Window {
        public MainWindow() {
            InitializeComponent();
            DataContext = new MainWindowViewModel();
        }
    }
}
'@
Set-Content -Path $mainCsPath -Value $mainCs -Encoding UTF8

# ---------------------------------------------------------
# 9. BUILD & RUN
# ---------------------------------------------------------
Write-Host "🚀 BUILDING..." -ForegroundColor Yellow
dotnet clean
dotnet build $projFile

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ SUCCESS! Launching..." -ForegroundColor Green
    dotnet run --project $projFile
}
else {
    Write-Host "❌ BUILD FAILED. Check '$LogFile' for details." -ForegroundColor Red
}

Stop-Transcript
