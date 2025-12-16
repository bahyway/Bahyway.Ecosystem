using Avalonia;
using BahyWay.SharedKernel.Domain; // Your generated models
using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace Bahyway.KGEditor.UI.Services
{
    public class SpatialViewportService
    {
        // 1. Define the Screen Boundaries (The Camera)
        public Rect CurrentViewport { get; private set; }

        // 2. The Logic: Convert Screen Coordinates to Akkadian Spatial IDs
        public (long MinId, long MaxId) GetVisibleRange(Rect viewport)
        {
            // Simplified Logic:
            // In WPDWay, ID = (Lat/Lon Hash).
            // We reverse-map the screen X/Y to Lat/Lon, then to Color_ID.

            long minId = SpatialIdGenerator.FromCoordinates(viewport.X, viewport.Y);
            long maxId = SpatialIdGenerator.FromCoordinates(viewport.Right, viewport.Bottom);

            return (minId, maxId);
        }

        // 3. Fetch ONLY what is visible
        public async Task<List<object>> LoadVisibleNodesAsync(Rect viewport)
        {
            var range = GetVisibleRange(viewport);

            // TODO: Call the Actor System here
            // var nodes = await _actorSystem.Ask(new QuerySpatialRange(range.MinId, range.MaxId));

            // Mock Data for now to test UI
            return new List<object>();
        }
    }

    // Mock Helper (You will replace this with your Core Algorithm later)
    public static class SpatialIdGenerator
    {
        public static long FromCoordinates(double x, double y) => (long)(x + y);
    }
}