using BahyWay.SharedKernel.Interfaces;
using StackExchange.Redis;
using System;
using System.Text.Json; // <--- REQUIRED for JsonSerializer
using System.Threading.Tasks;

namespace BahyWay.SharedKernel.Infrastructure.Messaging
{
    public class RedisMessageBus : IMessageBus
    {
        private readonly IConnectionMultiplexer _redis;
        private readonly IDatabase _db;

        // --- THIS WAS MISSING ---
        private readonly JsonSerializerOptions _jsonOptions = new JsonSerializerOptions
        {
            PropertyNameCaseInsensitive = true,
            WriteIndented = false,
            IncludeFields = true
        };
        // ------------------------

        public RedisMessageBus(string connectionString)
        {
            try
            {
                _redis = ConnectionMultiplexer.Connect(connectionString);
                _db = _redis.GetDatabase();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Redis Error] Connection failed: {ex.Message}");
                throw;
            }
        }

        public async Task PublishParticleAsync(string topic, object payload)
        {
            try
            {
                // FIX: Use the specific type to ensure all properties are serialized
                var json = JsonSerializer.Serialize(payload, payload.GetType(), _jsonOptions);

                await _db.PublishAsync(topic, json);

                // Debug Log
                Console.WriteLine($"[Redis Sent] Topic: {topic} | Payload: {json}");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Redis Error] Publish failed: {ex.Message}");
            }
        }

        public async Task PushToStreamAsync(string streamKey, object payload)
        {
            try
            {
                var json = JsonSerializer.Serialize(payload, payload.GetType(), _jsonOptions);

                var entries = new NameValueEntry[]
                {
                    new NameValueEntry("data", json)
                };
                await _db.StreamAddAsync(streamKey, entries);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Redis Error] Stream Push failed: {ex.Message}");
            }
        }

        public async Task AcknowledgeStreamAsync(string streamKey, string messageId)
        {
            try
            {
                await _db.StreamAcknowledgeAsync(streamKey, "etl_group", messageId);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Redis Error] Ack failed: {ex.Message}");
            }
        }

        public async Task SubscribeAsync<T>(string topic, Action<T> handler)
        {
            try
            {
                var subscriber = _redis.GetSubscriber();

                await subscriber.SubscribeAsync(topic, (channel, value) =>
                {
                    if (value.HasValue)
                    {
                        try
                        {
                            string json = value.ToString();
                            // Debug Log
                            // System.Diagnostics.Debug.WriteLine($"[Redis Recv] {json}");

                            var obj = JsonSerializer.Deserialize<T>(json, _jsonOptions);

                            if (obj != null) handler(obj);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine($"[Bus Error] Deserialize failed: {ex.Message}");
                        }
                    }
                });
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Redis Error] Subscribe failed: {ex.Message}");
            }
        }
    }
}