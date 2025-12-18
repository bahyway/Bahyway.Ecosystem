# 1. SETUP LOGGING
$LogFile = "$PWD/repair_log.txt"
Start-Transcript -Path $LogFile -Force

Write-Host "🔍 STARTING REPAIR PROCESS..." -ForegroundColor Cyan

# 2. DEFINE PATHS
$root = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI"
$canvasFile = "$root/Controls/GraphCanvas.cs"
$xamlFile   = "$root/Views/MainWindow.axaml"
$csFile     = "$root/Views/MainWindow.axaml.cs"
$projFile   = "$root/Bahyway.KGEditor.UI.csproj"

# 3. VERIFY DIRECTORIES
if (-not (Test-Path "$root/Controls")) {
    New-Item -ItemType Directory -Force -Path "$root/Controls" | Out-Null
    Write-Host "Created Controls directory."
}

# ====================================================
# 4. FIX GraphCanvas.cs (C#)
# ====================================================
Write-Host "📝 Repairing GraphCanvas.cs..."
$canvasContent = @'
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

            // Draw Background
            context.FillRectangle(Brushes.Black, new Rect(Bounds.Size));

            // Draw Edges
            foreach (var edge in _graph.Edges)
            {
                var source = _graph.Nodes.FirstOrDefault(n => n.Id == edge.SourceId);
                var target = _graph.Nodes.FirstOrDefault(n => n.Id == edge.TargetId);

                if (source != null && target != null)
                {
                    context.DrawLine(_edgePen, new Point(source.X, source.Y), new Point(target.X, target.Y));
                }
            }

            // Draw Nodes
            foreach (var node in _graph.Nodes)
            {
                var brush = Brush.Parse(node.ColorHex);
                var center = new Point(node.X, node.Y);
                context.DrawEllipse(brush, null, center, 20, 20);

                // Simple text rendering
                var text = new FormattedText(
                    node.Label,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    Typeface.Default,
                    12,
                    _textBrush
                );
                context.DrawText(text, new Point(center.X - text.Width/2, center.Y + 25));
            }
        }
    }
}
'@
Set-Content -Path $canvasFile -Value $canvasContent -Encoding UTF8

# ====================================================
# 5. FIX MainWindow.axaml (XAML/XML)
# ====================================================
Write-Host "📝 Repairing MainWindow.axaml..."
$xamlContent = @'
<Window xmlns="https://github.com/avaloniaui"
        xmlns:x="http://schemas.microsoft.com/winfx/2006/xaml"
        xmlns:d="http://schemas.microsoft.com/expression/blend/2008"
        xmlns:mc="http://schemas.openxmlformats.org/markup-compatibility/2006"
        xmlns:controls="using:Bahyway.KGEditor.UI.Controls"
        mc:Ignorable="d" d:DesignWidth="800" d:DesignHeight="600"
        x:Class="Bahyway.KGEditor.UI.Views.MainWindow"
        Title="Bahyway KGEditor"
        Width="1000" Height="700">

  <Grid>
    <!-- The Custom Graph Control -->
    <controls:GraphCanvas Name="MyGraphCanvas"/>
  </Grid>
</Window>
'@
Set-Content -Path $xamlFile -Value $xamlContent -Encoding UTF8

# ====================================================
# 6. FIX MainWindow.axaml.cs (C# Code Behind)
# ====================================================
Write-Host "📝 Repairing MainWindow.axaml.cs..."
$csContent = @'
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

            // Create Nodes
            graph.Nodes.Add(new VisualNode { Id = "1", Label = "Sensor (Critical)", X = 400, Y = 300, ColorHex = "#FF0000" });
            graph.Nodes.Add(new VisualNode { Id = "2", Label = "Valve A", X = 200, Y = 150, ColorHex = "#00FF00" });
            graph.Nodes.Add(new VisualNode { Id = "3", Label = "Valve B", X = 600, Y = 150, ColorHex = "#00FF00" });

            // Connect Edges
            graph.Edges.Add(new VisualEdge { SourceId = "2", TargetId = "1" });
            graph.Edges.Add(new VisualEdge { SourceId = "3", TargetId = "1" });

            // Render
            var canvas = this.FindControl<GraphCanvas>("MyGraphCanvas");
            if (canvas != null)
            {
                canvas.RenderGraph(graph);
            }
        }
    }
}
'@
Set-Content -Path $csFile -Value $csContent -Encoding UTF8

# ====================================================
# 7. BUILD AND RUN
# ====================================================
Write-Host "🧹 Cleaning previous build artifacts..."
dotnet clean

Write-Host "🔨 Building Project..."
dotnet build $projFile

if ($LASTEXITCODE -eq 0) {
    Write-Host "✅ Build Success! Launching..." -ForegroundColor Green
    dotnet run --project $projFile
} else {
    Write-Host "❌ Build Failed. Check the log file below." -ForegroundColor Red
}

Stop-Transcript
Write-Host "📄 LOG SAVED TO: $LogFile" -ForegroundColor Yellow
