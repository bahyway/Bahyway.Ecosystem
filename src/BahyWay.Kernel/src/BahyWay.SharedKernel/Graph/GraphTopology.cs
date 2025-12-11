using System.Collections.Generic;

namespace Bahyway.SharedKernel.Graph
{
    // Represents the "Visual" state of the Knowledge Graph
    public class GraphTopology
    {
        public List<VisualNode> Nodes { get; set; } = new();
        public List<VisualEdge> Edges { get; set; } = new();
    }

    public class VisualNode
    {
        public string Id { get; set; }
        public string Label { get; set; }
        public double X { get; set; } // Visual Coordinate
        public double Y { get; set; } // Visual Coordinate
        public string ColorHex { get; set; } = "#3498db"; // Default Blue
    }

    public class VisualEdge
    {
        public string SourceId { get; set; }
        public string TargetId { get; set; }
    }
}