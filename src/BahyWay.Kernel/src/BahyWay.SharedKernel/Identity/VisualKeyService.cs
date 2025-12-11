using System;

namespace Bahyway.SharedKernel.Identity
{
    public static class VisualKeyService
    {
        // Bounding Box for Wadi Al-Salaam, Najaf (Approximate)
        // This ensures the color resolution is focused ONLY on the cemetery, not the whole world.
        private const double LatMin = 31.9800;
        private const double LatMax = 32.0200; // ~4km range
        private const double LonMin = 44.3000;
        private const double LonMax = 44.3500; // ~5km range

        // Depth: From Surface (0m) to Deep Underground (30m)
        private const double DepthMin = 0.0;
        private const double DepthMax = 30.0;

        /// <summary>
        /// Generates a 32-bit Visual Key (#RRGGBBAA) based on 3D geospatial coordinates.
        /// </summary>
        /// <param name="lat">Latitude (GPS)</param>
        /// <param name="lon">Longitude (GPS)</param>
        /// <param name="depth">Depth in meters (0 = Surface)</param>
        /// <returns>A Hex Color String (e.g., #FF573380)</returns>
        public static string Generate(double lat, double lon, double depth)
        {
            // 1. Normalize Coordinates to 0-255 range (Byte)
            byte r = MapToByte(lat, LatMin, LatMax);
            byte g = MapToByte(lon, LonMin, LonMax);

            // 2. Map Depth to Blue (Deeper graves = Darker Blue usually, or distinct shade)
            byte b = MapToByte(depth, DepthMin, DepthMax);

            // 3. The "Infinite" Factor (Alpha Channel)
            // We use the "Micro-precision" of the GPS coordinates to set transparency.
            // This distinguishes two graves that are very close but not identical.
            double latFraction = (lat - Math.Truncate(lat)) * 10000;
            double lonFraction = (lon - Math.Truncate(lon)) * 10000;
            byte a = (byte)((int)(latFraction + lonFraction) % 256);

            // 4. Return Format: #RRGGBBAA (Red, Green, Blue, Alpha)
            return $"#{r:X2}{g:X2}{b:X2}{a:X2}";
        }

        private static byte MapToByte(double value, double min, double max)
        {
            // Clamp value to bounds
            if (value < min) value = min;
            if (value > max) value = max;

            // Normalize to 0.0 - 1.0
            double ratio = (value - min) / (max - min);

            // Scale to 0 - 255
            return (byte)(ratio * 255);
        }
    }
}