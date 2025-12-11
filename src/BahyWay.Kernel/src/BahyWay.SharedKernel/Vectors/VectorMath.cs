using System;

namespace Bahyway.SharedKernel.Vectors
{
    public static class VectorMath
    {
        public static float[] WeightedAverage(float[][] vectors, double[] weights, int dimensions)
        {
            float[] result = new float[dimensions];
            double totalWeight = 0;

            for (int i = 0; i < vectors.Length; i++)
            {
                if (i >= weights.Length) break; // Safety check

                double w = weights[i];
                totalWeight += w;

                for (int d = 0; d < dimensions; d++)
                {
                    result[d] += (float)(vectors[i][d] * w);
                }
            }

            // Normalize
            for (int d = 0; d < dimensions; d++)
            {
                result[d] /= (float)totalWeight;
            }

            return result;
        }

        public static double CosineSimilarity(float[] v1, float[] v2)
        {
            double dot = 0.0, mag1 = 0.0, mag2 = 0.0;
            for (int i = 0; i < v1.Length; i++)
            {
                dot += v1[i] * v2[i];
                mag1 += Math.Pow(v1[i], 2);
                mag2 += Math.Pow(v2[i], 2);
            }
            return dot / (Math.Sqrt(mag1) * Math.Sqrt(mag2));
        }
    }
}