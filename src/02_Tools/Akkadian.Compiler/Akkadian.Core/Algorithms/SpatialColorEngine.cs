using System;

namespace Akkadian.Core.Algorithms
{
    public static class SpatialColorEngine
    {
        // Generates a 64-bit "Color" ID
        // Format: [Source: 8 bits] [Time: 16 bits] [Layer: 8 bits] [RGB: 24 bits] = 56 bits used
        public static long GenerateCompositeId(int sourceId, DateTime time, int layer, int r, int g, int b)
        {
            long id = 0;

            // 1. Source Type (e.g., Satellite vs Drone) - 8 bits
            id |= (long)(sourceId & 0xFF) << 48;

            // 2. Time Component (Hour of year) - 16 bits
            int timeComponent = time.DayOfYear * 24 + time.Hour;
            id |= (long)(timeComponent & 0xFFFF) << 32;

            // 3. Spatial Layer (Z-index) - 8 bits
            id |= (long)(layer & 0xFF) << 24;

            // 4. Visual Color (RGB) - 24 bits
            id |= (long)((r & 0xFF) << 16);
            id |= (long)((g & 0xFF) << 8);
            id |= (long)(b & 0xFF);

            return id;
        }

        // Example for WPDWay: Hash Lat/Lon into a Color ID
        public static long GeoToColorId(double lat, double lon, int layer)
        {
            // Simple normalization for demo (Map -90..90 to 0..255, etc)
            int r = (int)(((lat + 90) / 180.0) * 255);
            int g = (int)(((lon + 180) / 360.0) * 255);
            int b = (r + g) % 255; // Synthetic blue for depth

            return GenerateCompositeId(1, DateTime.Now, layer, r, g, b);
        }
    }
}