using Xunit;
using Bahyway.SharedKernel.Identity;

namespace Bahyway.Tests
{
    public class VisualKeyTests
    {
        [Fact]
        public void Should_Generate_Unique_Keys_For_Neighbors()
        {
            // Grave A: Specific spot in Najaf
            double latA = 32.00010;
            double lonA = 44.32010;
            double depth = 2.0; // Standard grave depth

            // Grave B: A neighbor, 1 meter away (GPS differs slightly)
            double latB = 32.00011;
            double lonB = 44.32011;

            // Generate Keys
            string keyA = VisualKeyService.Generate(latA, lonA, depth);
            string keyB = VisualKeyService.Generate(latB, lonB, depth);

            // Output for debugging
            System.Console.WriteLine($"Grave A: {keyA}");
            System.Console.WriteLine($"Grave B: {keyB}");

            // 1. Check Format
            Assert.StartsWith("#", keyA);
            Assert.Equal(9, keyA.Length); // # + 8 hex chars

            // 2. Check Uniqueness (The Alpha/RGB mix must differ)
            Assert.NotEqual(keyA, keyB);
        }

        [Fact]
        public void Should_Handle_Depth_Variation()
        {
            // Two bodies in the SAME grave shaft, but different depths
            string surface = VisualKeyService.Generate(32.0, 44.32, 0.0);
            string deep = VisualKeyService.Generate(32.0, 44.32, 5.0);

            // The Blue channel (3rd pair) should differ
            Assert.NotEqual(surface, deep);
        }
    }
}