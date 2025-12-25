using System.Threading.Tasks;

namespace BahyWay.SharedKernel.Interfaces
{
    public interface IMessageBus
    {
        // 1. For Visuals (Fire-and-Forget / PubSub) -> Goes to ShoWay
        Task PublishParticleAsync(string topic, object payload);

        // 2. For Work (Reliable / Streams) -> Goes to ETLWay
        Task PushToStreamAsync(string streamKey, object payload);

        // 3. For ETLWay to acknowledge work is done
        Task AcknowledgeStreamAsync(string streamKey, string messageId);

        // Add this line
        Task SubscribeAsync<T>(string topic, Action<T> handler);
    }
}