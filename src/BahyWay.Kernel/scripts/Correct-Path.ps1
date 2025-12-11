# 1. Define Paths
$wrongPath = "src/Bahyway.KGEditor/Controls/GraphCanvas.cs"
$correctDir = "src/Bahyway.KGEditor/Bahyway.KGEditor.UI/Controls"
$correctPath = "$correctDir/GraphCanvas.cs"

# 2. Create the Directory if it doesn't exist
if (-not (Test-Path $correctDir)) {
    New-Item -ItemType Directory -Force -Path $correctDir | Out-Null
    Write-Host "Created folder: $correctDir" -ForegroundColor Gray
}

# 3. Move the file if it exists in the wrong place
if (Test-Path $wrongPath) {
    Move-Item -Path $wrongPath -Destination $correctPath -Force
    Write-Host "✅ Fixed: Moved GraphCanvas.cs to the correct UI project folder." -ForegroundColor Green
} elseif (Test-Path $correctPath) {
    Write-Host "ℹ️ File is already in the correct place." -ForegroundColor Yellow
} else {
    Write-Host "⚠️ Could not find GraphCanvas.cs in either location. We will recreate it." -ForegroundColor Red

    # Re-create the file content just in case
    $content = @"
using Avalonia;
using Avalonia.Controls;
using Avalonia.Media;
using Bahyway.SharedKernel.Graph;
using System.Linq;

namespace Bahyway.KGEditor.Controls
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
"@
    Set-Content -Path $correctPath -Value $content
    Write-Host "✅ Created fresh GraphCanvas.cs in the correct location." -ForegroundColor Green
}
