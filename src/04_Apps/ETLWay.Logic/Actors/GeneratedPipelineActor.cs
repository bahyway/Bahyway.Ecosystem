using Akka.Actor;
using BahyWay.SharedKernel.Application.Abstractions;
using BahyWay.SharedKernel.Application.DTOs;
using BahyWay.SharedKernel.Application.Events;
using BahyWay.SharedKernel.Interfaces;
using ETLWay.Infrastructure.Patterns.Extractors;
using ETLWay.Infrastructure.Patterns.Loaders;
using ETLWay.Infrastructure.Patterns.Pipelines;
using ETLWay.Infrastructure.Patterns.Transformers;
using System;
using System.Threading.Tasks;

namespace ETLWay.Logic.Actors
{
    public class GeneratedPipelineActor : ReceiveActor
    {
        private readonly IZipExtractionService _zipService;
        private readonly IMessageBus _bus;
        private readonly IMessageResolver _messages;

        public GeneratedPipelineActor(IZipExtractionService zipService, IMessageBus bus, IMessageResolver messages)
        {
            _zipService = zipService;
            _bus = bus;
            _messages = messages;

            ReceiveAsync<FileArrivedEvent>(async msg =>
            {
                try
                {
                    await RunPipeline(msg);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"[Actor Error] Critical Failure: {ex.Message}");
                }
            });
        }

        private async Task RunPipeline(FileArrivedEvent msg)
        {
            var context = new Application.Abstractions.Patterns.EtlContext
            {
                JobId = msg.ExecutionId ?? Guid.NewGuid().ToString()
            };

            try
            {
                // 1. Notify Start
                await SendStatus(context.JobId, msg.FileName, "Extraction", "Processing", "#FFFF00");

                var pipeline = new LinearPipeline<FileMetadataDto, FileMetadataDto>(
                    new ZipFileExtractor(_zipService, msg.FilePath, @"C:\BahyWay\Processing"),
                    new FuzzySchemaValidator(_messages, strictness: 50.0),
                    new StagingTableLoader(),
                    context
                );

                // 2. Execute (This might throw an exception!)
                await pipeline.ExecuteAsync();

                // 3. Final Decision Logic (Happy/Warning Paths)
                if (context.QualityScore < 50)
                {
                    await HandleCriticalFailure(context, msg);
                }
                else if (context.Metadata.ContainsKey("RequiresApproval"))
                {
                    await SendStatus(context.JobId, msg.FileName, "Validation", "Warning", "#FFA500");
                    await PublishScore(context.JobId, msg.FileName, context.QualityScore, "Warning", "Schema Match < 90%. Approval Required.");
                }
                else
                {
                    await SendStatus(context.JobId, msg.FileName, "StagingDB", "Success", "#00FF00");
                    await PublishScore(context.JobId, msg.FileName, context.QualityScore, "Excellent", "Processing Complete.");
                }
            }
            catch (Exception ex)
            {
                // 4. CATCH THE CRASH
                Console.WriteLine($"[ETLWay] 💥 PIPELINE CRASHED: {ex.Message}");

                // Add the exception to the context errors
                context.Errors.Add($"SYSTEM EXCEPTION: {ex.Message}");
                context.QualityScore = 0; // Force fail

                // Report it to UI
                await HandleCriticalFailure(context, msg);
            }
        }

        // Helper to report failures
        private async Task HandleCriticalFailure(Application.Abstractions.Patterns.EtlContext context, FileArrivedEvent msg)
        {
            await SendStatus(context.JobId, msg.FileName, "ErrorQueue", "Failed", "#FF0000");

            // Extract Error Text
            string errorText = context.Errors.Count > 0
                ? string.Join("\n", context.Errors)
                : "Unknown Critical Failure";

            // Send to UI
            await PublishScore(context.JobId, msg.FileName, context.QualityScore, "Critical", errorText);
        }
        private async Task SendStatus(string id, string file, string node, string status, string color)
        {
            try
            {
                await _bus.PublishParticleAsync("Flow.Particles", new FlowParticleEvent
                {
                    ExecutionId = id,
                    FileName = file,
                    ToNode = node,
                    Status = status,
                    ColorHex = color,
                    Timestamp = DateTime.UtcNow
                });
            }
            catch { /* Ignore logging failures to keep actor alive */ }
        }

        // CHANGE: Add 'id' parameter
        private async Task PublishScore(string id, string source, double score, string status, string message = "")
        {
            // Fallback for empty message
            if (string.IsNullOrWhiteSpace(message)) message = "System Error: No details provided.";

            await _bus.PublishParticleAsync("Dashboard.Scores", new ScoreUpdateEvent
            {
                ExecutionId = id, // <--- CRITICAL FIX: Link the event to the job
                SourceSystem = source,
                QualityScore = score,
                OverallStatus = status,
                Message = message
            });
        }
    }
}