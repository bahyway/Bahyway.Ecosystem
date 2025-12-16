using System.Collections.Generic;
using System.Threading.Tasks;
using Avalonia; // For Rect
using Bahyway.SharedKernel.Graph; // For VisualNode

namespace BahyWay.SharedKernel.Interfaces
{
    // Every project compiled by Akkadian will generate a class that implements this
    public interface IAkkadianModule
    {
        string ModuleName { get; }

        // The "Billions of Nodes" hook
        // The Editor gives a Viewport (Screen coordinates), the Module returns the Nodes
        Task<List<VisualNode>> LoadNodesAsync(Rect viewport);
    }
}