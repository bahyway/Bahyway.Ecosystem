# Start logging to a file
$LogFile = "$PWD/fix_log.txt"
Start-Transcript -Path $LogFile -Force

Write-Host "🔍 STARTING DIAGNOSTIC FIX..." -ForegroundColor Cyan

# 1. Define the Project Path
$uiProjectPath = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI"
$controlsPath = "$uiProjectPath/Controls"
$viewsPath = "$uiProjectPath/Views"

# 2. Check if folders exist
if (-not (Test-Path $uiProjectPath)) {
    Write-Error "❌ CRITICAL: UI Project folder not found at: $uiProjectPath"
    Stop-Transcript
    return
}

# 3. Force Create/Recreate Controls Folder
if (-not (Test-Path $controlsPath)) {
    New-Item -ItemType Directory -Force -Path $controlsPath | Out-Null
    Write-Host "Created Controls folder."
}

# ---------------------------------------------------------
# 4. REWRITE GraphCanvas.cs (Force correct namespace)
# ---------------------------------------------------------
$canvasFile = "$controlsPath/GraphCanvas.cs"
Write-Host "📝 Writing GraphCanvas.cs to: $canvasFile"
$canvasCode = @"
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
        private readonly Pen _edgePen = new Pen(Brushes.Gray, 2);
        private readonly Brush _textBrush = Brushes.White;

        public void RenderGraph(GraphTopology graph)
        {
            _graph = graph;
            InvalidateVisual();
        }

        public override void Render(DrawingContext context)
        {
            if (_graph == null) return;
            context.FillRectangle(Brushes.Black, new Rect(Bounds.Size));

            foreach (var edge in _graph.Edges)
            {
                var source = _graph.Nodes.FirstOrDefault(n => n.Id == edge.SourceId);
                var target = _graph.Nodes.FirstOrDefault(n => n.Id == edge.TargetId);
                if (source != null && target != null)
                {
                    context.DrawLine(_edgePen, new Point(source.X, source.Y), new Point(target.X, target.Y));
                }
            }

            foreach (var node in _graph.Nodes)
            {
                var brush = Brush.Parse(node.ColorHex);
                var center = new Point(node.X, node.Y);
                context.DrawEllipse(brush, null, center, 20, 20);
            }
        }
    }
}
"@
Set-Content -Path $canvasFile -Value $canvasCode

# ---------------------------------------------------------
# 5. REWRITE MainWindow.axaml.cs (Force correct using)
# ---------------------------------------------------------
$csFile = "$viewsPath/MainWindow.axaml.cs"
Write-Host "📝 Writing MainWindow.axaml.cs to: $csFile"
$csCode = @"
using Avalonia.Controls;
using Bahyway.KGEditor.UI.Controls;
using Bahyway.SharedKernel.Graph;
using System;

namespace Bahyway.KGEditor.UI.Views
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
            this.Opened += OnWindowOpened;
        }

        private void OnWindowOpened(object? sender, EventArgs e)
        {
            var graph = new GraphTopology();
            graph.Nodes.Add(new VisualNode { Id = ""1"", Label = ""Sensor"", X = 400, Y = 300, ColorHex = ""#FF0000"" });
            graph.Nodes.Add(new VisualNode { Id = ""2"", Label = ""Valve"", X = 200, Y = 150, ColorHex = ""#00FF00"" });
            graph.Edges.Add(new VisualEdge { SourceId = ""2"", TargetId = ""1"" });

            var canvas = this.FindControl<GraphCanvas>(""MyGraphCanvas"");
            if (canvas != null) canvas.RenderGraph(graph);
        }
    }
}
"@
Set-Content -Path $csFile -Value $csCode

# ---------------------------------------------------------
# 6. REWRITE MainWindow.axaml (Force correct XMLNS)
# ---------------------------------------------------------
$xamlFile = "$viewsPath/MainWindow.axaml"
Write-Host "📝 Writing MainWindow.axaml to: $xamlFile"
$xamlCode = @"
<Window xmlns=""https://github.com/avaloniaui""
        xmlns:x=""http://schemas.microsoft.com/winfx/2006/xaml""
        xmlns:vm=""using:Bahyway.KGEditor.UI.ViewModels""
        xmlns:controls=""using:Bahyway.KGEditor.UI.Controls""
        x:Class=""Bahyway.KGEditor.UI.Views.MainWindow""
        Title=""Bahyway Visualizer""
        Width=""800"" Height=""600"">
  <Grid>
    <controls:GraphCanvas Name=""MyGraphCanvas""/>
  </Grid>
</Window>
"@
Set-Content -Path $xamlFile -Value $xamlCode

# ---------------------------------------------------------
# 7. CLEAN AND RUN
# ---------------------------------------------------------
Write-Host "🧹 Cleaning Solution..."
dotnet clean

Write-Host "🚀 Attempting Run..."
dotnet run --project "$uiProjectPath/Bahyway.KGEditor.UI.csproj"

Stop-Transcript
Write-Host "📄 LOG FILE GENERATED AT: $LogFile" -ForegroundColor Yellow
