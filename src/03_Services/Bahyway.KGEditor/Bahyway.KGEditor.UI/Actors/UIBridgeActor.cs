using Akka.Actor;
using Avalonia.Threading; // Crucial for UI updates
using Bahyway.KGEditor.UI.Controls; // To access GraphCanvas
using Bahyway.SharedKernel.Actors; // To see the message
using Bahyway.SharedKernel.Graph; // To see VisualNode
using System.Linq;

namespace Bahyway.KGEditor.UI.Actors
{
    public class UIBridgeActor : ReceiveActor
    {
        private readonly GraphCanvas _canvas;
        private readonly GraphTopology _topology;

        public UIBridgeActor(GraphCanvas canvas, GraphTopology topology)
        {
            _canvas = canvas;
            _topology = topology;

            Receive<NodeColorUpdate>(update =>
            {
                // Find the node in memory
                var node = _topology.Nodes.FirstOrDefault(n => n.Id == update.NodeId);
                if (node != null)
                {
                    // Update Data
                    node.ColorHex = update.ColorHex;

                    // Update UI (Must happen on UI Thread)
                    Dispatcher.UIThread.InvokeAsync(() =>
                    {
                        _canvas.RenderGraph(_topology);
                    });
                }
            });
        }
    }
}
