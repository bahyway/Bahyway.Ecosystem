using BahyWay.SharedKernel.Application.Abstractions; // Interface
using BahyWay.SharedKernel.Infrastructure.FileStorage; // Implementation
using BahyWay.SharedKernel.Infrastructure.Messaging;
using BahyWay.SharedKernel.Interfaces;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using BahyWay.SharedKernel.Infrastructure; // <--- ADD THIS (Where JsonMessageResolver lives)

namespace BahyWay.SharedKernel
{
    public static class DependencyInjection
    {
        public static IServiceCollection AddBahyWayPlatform(this IServiceCollection services, IConfiguration configuration)
        {
            // 1. Register Redis Message Bus
            var redisConn = configuration.GetConnectionString("Redis") ?? "localhost:6379";
            services.AddSingleton<IMessageBus>(new RedisMessageBus(redisConn));

            // 2. Register Zip Extraction
            // Now this works because both types are local to SharedKernel!
            services.AddTransient<IZipExtractionService, ZipExtractionService>();

            // 3. Register Message Resolver
            // We assume the JSON file is copied to the bin folder
            var messagePath = Path.Combine(AppContext.BaseDirectory, "system_messages.json");
            services.AddSingleton<IMessageResolver>(new JsonMessageResolver(messagePath));


            return services;
        }
    }
}