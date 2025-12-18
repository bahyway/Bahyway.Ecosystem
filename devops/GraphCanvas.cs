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

        // Method to feed data into the renderer
        public void RenderGraph(GraphTopology graph)
        {
            _graph = graph;
            InvalidateVisual(); // Triggers the Render loop
        }

        public override void Render(DrawingContext context)
        {
            if (_graph == null) return;

            // 1. Draw a dark background
            context.FillRectangle(Brushes.Black, new Rect(Bounds.Size));

            // 2. Draw Edges (Lines)
            foreach (var edge in _graph.Edges)
            {
                var source = _graph.Nodes.FirstOrDefault(n => n.Id == edge.SourceId);
                var target = _graph.Nodes.FirstOrDefault(n => n.Id == edge.TargetId);

                if (source != null && target != null)
                {
                    context.DrawLine(_edgePen, new Point(source.X, source.Y), new Point(target.X, target.Y));
                }
            }

            // 3. Draw Nodes (Circles)
            foreach (var node in _graph.Nodes)
            {
                var brush = Brush.Parse(node.ColorHex);
                var center = new Point(node.X, node.Y);

                // Draw the node circle (Radius 20)
                context.DrawEllipse(brush, null, center, 20, 20);

                // Draw the text label
                var text = new FormattedText(
                    node.Label,
                    System.Globalization.CultureInfo.CurrentCulture,
                    FlowDirection.LeftToRight,
                    Typeface.Default,
                    12,
                    _textBrush
                );

                // Center text
                context.DrawText(text, new Point(center.X - text.Width / 2, center.Y + 25));
            }
        }
    }
}