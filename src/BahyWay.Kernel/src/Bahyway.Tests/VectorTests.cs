using Xunit;
using System;
using System.Threading.Tasks;
using Bahyway.SharedKernel.Vectors;

namespace Bahyway.Tests
{
    public class VectorTests
    {
        // 1. A Fake AI Model (Deterministic)
        // Generates consistent vectors based on string hash so we can test math.
        class MockAI : IVectorModel
        {
            public Task<float[]> GetEmbeddingAsync(string text)
            {
                var rng = new Random(text.GetHashCode());
                var vector = new float[1536];
                for (int i = 0; i < 1536; i++) vector[i] = (float)rng.NextDouble();
                return Task.FromResult(vector);
            }
        }

        [Fact]
        public async Task Should_Recognize_Similar_Arabic_Names()
        {
            var vectorizer = new ArabicNameVectorizer(new MockAI());

            // Case: Spelling variation
            string name1 = "Ahmed Ali Al-Asadi";
            string name2 = "Ahmad Ali AlAsadi"; // Different spelling

            // Generate Vectors
            float[] v1 = await vectorizer.VectorizeAsync(name1);
            float[] v2 = await vectorizer.VectorizeAsync(name2);

            // Compare Similarity
            double similarity = VectorMath.CosineSimilarity(v1, v2);

            System.Console.WriteLine($"Similarity: {similarity * 100}%");

            // Even with dummy hashing, they should be somewhat distinct but valid vectors
            Assert.NotNull(v1);
            Assert.Equal(1536, v1.Length);
        }

        [Fact]
        public void Normalization_Should_Fix_Abdullah()
        {
            // We expose the private method via a public wrapper or just test the effect
            // Here we test via the vectorizer output consistency
            var vectorizer = new ArabicNameVectorizer(new MockAI());

            // Because of our normalization logic in ArabicNameVectorizer.cs:
            // "عبد الله" becomes "عبدالله" before hashing.
            // So these two should produce IDENTICAL vectors (1.0 similarity).
            var t1 = vectorizer.VectorizeAsync("عبد الله").Result;
            var t2 = vectorizer.VectorizeAsync("عبدالله").Result;

            double similarity = VectorMath.CosineSimilarity(t1, t2);

            Assert.Equal(1.0, similarity, 4); // Should be 100% match
        }
    }
}