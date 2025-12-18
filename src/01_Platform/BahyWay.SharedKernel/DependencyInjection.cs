using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Configuration;
using BahyWay.SharedKernel.Infrastructure.Messaging;
using BahyWay.SharedKernel.Interfaces;
using BahyWay.SharedKernel.Application.Abstractions; // Interface
using BahyWay.SharedKernel.Infrastructure.FileStorage; // Implementation

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

            return services;
        }
    }
}