using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Bahyway.SharedKernel.Vectors
{
    public class ArabicNameVectorizer
    {
        private readonly IVectorModel _aiModel;
        private const int VectorDimensions = 1536; // Standard OpenAI size

        // The Skeleton Weights defined in your DSL
        private readonly double[] _weights = new double[]
        {
            1.0,  // First Name (Ism)
            0.8,  // Father (Nasab 1)
            0.6,  // Grandfather (Nasab 2)
            2.5   // Tribe (Nisba) - Highest Weight for Clustering
        };

        public ArabicNameVectorizer(IVectorModel aiModel)
        {
            _aiModel = aiModel;
        }

        public async Task<float[]> VectorizeAsync(string fullName)
        {
            // 1. Preprocessing (Normalization)
            var cleanName = NormalizeArabic(fullName);

            // 2. Segmentation (Splitting the skeleton)
            var parts = cleanName.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            // 3. Get Embeddings for each part
            var vectors = new List<float[]>();
            foreach (var part in parts)
            {
                vectors.Add(await _aiModel.GetEmbeddingAsync(part));
            }

            // 4. Adjust Weights dynamically based on name length
            // If name has 3 parts, we use weights [0], [1], [3] (Treat last as Tribe)
            double[] currentWeights;
            if (parts.Length == 3)
                currentWeights = new double[] { _weights[0], _weights[1], _weights[3] };
            else if (parts.Length >= 4)
                currentWeights = _weights;
            else
                currentWeights = _weights.Take(parts.Length).ToArray();

            // 5. Combine into single "Skeleton Vector"
            return VectorMath.WeightedAverage(vectors.ToArray(), currentWeights, VectorDimensions);
        }

        private string NormalizeArabic(string input)
        {
            if (string.IsNullOrWhiteSpace(input)) return "";

            return input
                .Replace("عبد الله", "عبدالله") // Merge Abdullah
                .Replace("أ", "ا") // Normalize Alef
                .Replace("إ", "ا")
                .Replace("آ", "ا")
                .Replace("ى", "ي") // Normalize Ya
                .Replace("ة", "h"); // Normalize Ta Marbuta (optional strategy)
        }
    }
}