# 1. DEFINE PATH
$canvasFile = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI/Controls/GraphCanvas.cs"
$projFile   = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI/Bahyway.KGEditor.UI.csproj"

Write-Host "🔧 FIXING AVALONIA TYPE MISMATCH..." -ForegroundColor Cyan

# 2. OVERWRITE GraphCanvas.cs (WITH IBrush FIX)
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

        // FIX: Changed 'Pen' to 'IPen' and 'Brush' to 'IBrush' for Avalonia 11 compatibility
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
Set-Content -Path $canvasFile -Value $canvasCode -Encoding UTF8

# 3. BUILD AND RUN
Write-Host "🚀 Launching KGEditor..." -ForegroundColor Green
dotnet run --project $projFile
