# 1. SETUP LOGGING
$LogFile = "$PWD/mvvm_repair_log.txt"
Start-Transcript -Path $LogFile -Force
Write-Host "🔧 STARTING MVVM REPAIR..." -ForegroundColor Cyan

# Define Paths
$uiPath = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI"
$projFile = "$uiPath/Bahyway.KGEditor.UI.csproj"
$controlsPath = "$uiPath/Controls"

# ---------------------------------------------------------
# 2. RESTORE REFERENCES (Fixes CS0234 & CS0246)
# ---------------------------------------------------------
Write-Host "🔗 Restoring Project References..."
# Link to SharedKernel
dotnet add $projFile reference src/Bahyway.SharedKernel/Bahyway.SharedKernel.csproj
# Add Akka
dotnet add $projFile package Akka.Remote --version 1.5.15
# Add WebView (for WebGraphControl)
dotnet add $projFile package Avalonia.Controls.WebView

# ---------------------------------------------------------
# 3. RECREATE WebGraphControl.cs (Fixes AXN0002)
# ---------------------------------------------------------
Write-Host "📝 Recreating WebGraphControl.cs..."
$webFile = "$controlsPath/WebGraphControl.cs"
$webCode = @'
using Avalonia.Controls;
using Avalonia.WebView;
using Avalonia.Platform;
using System;
using System.IO;
using System.Text.Json;
using System.Collections.Generic;
using System.Linq;
using Bahyway.SharedKernel.Graph;

namespace Bahyway.KGEditor.UI.Controls
{
    public class WebGraphControl : UserControl
    {
        private WebView _webView;

        public WebGraphControl()
        {
            _webView = new WebView();
            this.Content = _webView;

            // Path to assets
            string htmlPath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "Assets", "ontoway_engine.html");
            if (File.Exists(htmlPath))
            {
                _webView.Source = new Uri(htmlPath);
            }
        }

        public void RenderGraph(GraphTopology topology)
        {
            // Simple Render Logic
            var elements = new List<object>();
            foreach (var node in topology.Nodes)
                elements.Add(new { group = "nodes", data = new { id = node.Id, label = node.Label, color = node.ColorHex } });

            foreach (var edge in topology.Edges)
                elements.Add(new { group = "edges", data = new { source = edge.SourceId, target = edge.TargetId } });

            string json = JsonSerializer.Serialize(elements);

            // Invoke JS
            System.Threading.Tasks.Task.Delay(500).ContinueWith(_ => {
                Avalonia.Threading.Dispatcher.UIThread.InvokeAsync(() => {
                    _webView.ExecuteScriptAsync($"loadGraph('{json}')");
                });
            });
        }
    }
}
'@
Set-Content -Path $webFile -Value $webCode -Encoding UTF8

# ---------------------------------------------------------
# 4. RECREATE GraphCanvas.cs (Ensuring Correct Namespace)
# ---------------------------------------------------------
Write-Host "📝 Verifying GraphCanvas.cs..."
$canvasFile = "$controlsPath/GraphCanvas.cs"
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
        private readonly IBrush _textBrush = Brushes.White;

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
'@
Set-Content -Path $canvasFile -Value $canvasCode -Encoding UTF8

# ---------------------------------------------------------
# 5. CLEAN BUILD & RUN
# ---------------------------------------------------------
Write-Host "🧹 Cleaning..."
dotnet clean

Write-Host "🚀 Building & Running..."
dotnet run --project $projFile

Stop-Transcript
