using BahyWay.SharedKernel.Interfaces;
using StackExchange.Redis;
using System.Text.Json;
using System.Threading.Tasks;

namespace BahyWay.SharedKernel.Infrastructure.Messaging
{
    public class RedisMessageBus : IMessageBus
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        public RedisMessageBus(string connectionString)
        {
            _redis = ConnectionMultiplexer.Connect(connectionString);
            _db = _redis.GetDatabase();
        }

        // Fast: Broadcasts to UI (KGEditor)
        public async Task PublishParticleAsync(string topic, object payload)
        {
            var json = JsonSerializer.Serialize(payload);
            await _db.PublishAsync(topic, json);
        }

        // Reliable: Pushes to Queue (ETLWay)
        public async Task PushToStreamAsync(string streamKey, object payload)
        {
            var json = JsonSerializer.Serialize(payload);
            // XADD stream * data json
            await _db.StreamAddAsync(streamKey, "data", json);
        }

        public async Task AcknowledgeStreamAsync(string streamKey, string messageId)
        {
            await _db.StreamAcknowledgeAsync(streamKey, "etl_group", messageId);
        }
    }
}