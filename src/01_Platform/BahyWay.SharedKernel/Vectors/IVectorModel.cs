using System.Threading.Tasks;

namespace Bahyway.SharedKernel.Vectors
{
    public interface IVectorModel
    {
        // Converts text ("Ali") into numbers ([0.12, 0.98, ...])
        Task<float[]> GetEmbeddingAsync(string text);
    }
}