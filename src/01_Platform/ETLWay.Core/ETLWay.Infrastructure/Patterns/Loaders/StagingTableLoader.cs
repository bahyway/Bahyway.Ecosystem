using System;
using System.Threading.Tasks;
using BahyWay.SharedKernel.Application.DTOs;
using ETLWay.Application.Abstractions.Patterns;

namespace ETLWay.Infrastructure.Patterns.Loaders
{
    public class StagingTableLoader : ILoader<FileMetadataDto>
    {
        // In reality, inject IDbConnectionFactory

        public Task LoadAsync(FileMetadataDto input, EtlContext context)
        {
            // This is where we run the "CREATE TABLE STG_..." SQL
            // For now, we simulate the action

            Console.WriteLine($"[Pattern:Loader] Creating Staging Table for {input.OriginalFileName}");
            Console.WriteLine($"[Pattern:Loader] Columns detected: {input.Columns.Count}");

            // Logic to register this successful load in Redis for the UI
            // _bus.PublishAsync(...)

            return Task.CompletedTask;
        }
    }
}