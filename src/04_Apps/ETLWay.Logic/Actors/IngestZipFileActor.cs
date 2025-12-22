using Akka.Actor;
using BahyWay.SharedKernel.Application.Events;
using BahyWay.SharedKernel.Interfaces;
using BahyWay.SharedKernel.Application.Abstractions; // For IZipExtractionService
using System;
using System.IO;
using System.Threading.Tasks;

namespace ETLWay.Logic.Actors
{
    public class IngestZipFileActor : ReceiveActor
    {
        private readonly IMessageBus _bus;
        private readonly IZipExtractionService _zipService; // The Worker

        // Constructor Injection
        public IngestZipFileActor(IMessageBus bus, IZipExtractionService zipService)
        {
            _bus = bus;
            _zipService = zipService;

            ReceiveAsync<FileArrivedEvent>(async msg => await ProcessFile(msg));
        }

        private async Task ProcessFile(FileArrivedEvent msg)
        {
            //var executionId = Guid.NewGuid().ToString();
            // USE THE EXISTING ID (Fixes the split row issue)
            var executionId = msg.ExecutionId;
            var (source, type) = ParseMetadata(msg.FileName);

            // 1. Notify UI: Started
            await SendStatus(executionId, msg.FileName, source, type, "LandingZone", "Extraction", "Moving", "#FFFF00");

            Console.WriteLine($"[ETLWay] 🟢 Started processing {msg.FileName}...");

            try
            {
                // 2. Define Output Folder (e.g., C:\BahyWay\Processing\{GUID})
                var processingFolder = Path.Combine(Path.GetDirectoryName(msg.FilePath), "Processing", executionId);
                Directory.CreateDirectory(processingFolder);

                // 3. CALL THE WORKER (Real Extraction)
                Console.WriteLine($"[ETLWay] 🔨 Unzipping to {processingFolder}...");

                var metadata = await _zipService.ExtractAndAnalyzeAsync(msg.FilePath, processingFolder);

                Console.WriteLine($"[ETLWay] ✅ Extraction Complete!");
                Console.WriteLine($"         - Rows: {metadata.RowCount}");
                Console.WriteLine($"         - Columns: {metadata.Columns.Count}");
                Console.WriteLine($"         - Data File: {metadata.GeneratedDataFileName}");

                // 4. Notify UI: Success (Trigger Scoreboard)
                await SendStatus(executionId, msg.FileName, source, type, "Extraction", "StagingDB", "Success", "#00FF00");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ETLWay] 🔴 Error: {ex.Message}");
                await SendStatus(executionId, msg.FileName, source, type, "Extraction", "ErrorQueue", "Error", "#FF0000");
            }
        }

        private async Task SendStatus(string id, string file, string source, string type, string from, string to, string status, string color)
        {
            await _bus.PublishParticleAsync("Flow.Particles", new FlowParticleEvent
            {
                ExecutionId = id,
                FileName = file,
                SourceSystem = source,
                FileType = type,
                FromNode = from,
                ToNode = to,
                Status = status,
                ColorHex = color,
                Timestamp = DateTime.UtcNow
            });
        }

        private (string, string) ParseMetadata(string fileName)
        {
            var name = fileName.ToLower();
            if (name.Contains("najaf")) return ("Najaf_Cemetery", "Census_Data");
            return ("Unknown_Source", "Generic_File");
        }
    }
}