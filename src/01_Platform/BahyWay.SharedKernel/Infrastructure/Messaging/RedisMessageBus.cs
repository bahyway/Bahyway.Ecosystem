using BahyWay.SharedKernel.Interfaces;
using StackExchange.Redis;
using System;
using System.Threading.Tasks;
using MessagePack; // <--- NEW

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

        public async Task PublishParticleAsync(string topic, object payload)
        {
            try
            {
                // Serialize to Binary (Byte Array)
                byte[] bytes = MessagePackSerializer.Serialize(payload);
                await _db.PublishAsync(topic, bytes);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Redis Error] Publish failed: {ex.Message}");
            }
        }

        // Note: For Streams, Redis expects NameValues, so we usually store JSON or base64.
        // For simplicity in this specific "PushToStreamAsync" method, we can keep JSON
        // OR convert the binary to Base64 string. Let's stick to JSON for Streams for readability
        // in Redis tools, but use MessagePack for the UI Pub/Sub (Particles/Scores).
        public async Task PushToStreamAsync(string streamKey, object payload)
        {
            // Keep JSON here for now as Streams are often read by humans/scripts
            var json = System.Text.Json.JsonSerializer.Serialize(payload);
            await _db.StreamAddAsync(streamKey, new NameValueEntry[] { new("data", json) });
        }

        public async Task AcknowledgeStreamAsync(string streamKey, string messageId)
        {
            await _db.StreamAcknowledgeAsync(streamKey, "etl_group", messageId);
        }

        public async Task SubscribeAsync<T>(string topic, Action<T> handler)
        {
            var subscriber = _redis.GetSubscriber();

            await subscriber.SubscribeAsync(topic, (channel, value) =>
            {
                try
                {
                    // RedisValue can be cast to byte[]
                    byte[] bytes = (byte[])value;

                    // Deserialize Binary directly to Object
                    var obj = MessagePackSerializer.Deserialize<T>(bytes);

                    if (obj != null) handler(obj);
                }
                catch (Exception ex)
                {
                    System.Diagnostics.Debug.WriteLine($"[Binary Error] {ex.Message}");
                }
            });
        }
    }
}