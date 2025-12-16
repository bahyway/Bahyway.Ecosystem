namespace BahyWay.SharedKernel.Interfaces
{
    public interface IMessageBus
    {
        // 1. For Particles (Fire-and-Forget / PubSub)
        Task PublishEventAsync(string topic, object payload);

        // 2. For History & Versions (Streams / Persistence)
        Task AppendToStreamAsync(string streamKey, object data);
        Task<List<T>> GetStreamHistoryAsync<T>(string streamKey, int count);
    }
}