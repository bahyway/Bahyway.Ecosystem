using System;
using System.Threading.Tasks;
using Akka.Actor;
using BahyWay.SharedKernel.Application.Abstractions;
using BahyWay.SharedKernel.Application.DTOs;
using BahyWay.SharedKernel.Application.Events;
using BahyWay.SharedKernel.Interfaces;
using ETLWay.Infrastructure.Patterns.Extractors;
using ETLWay.Infrastructure.Patterns.Loaders;
using ETLWay.Infrastructure.Patterns.Pipelines;
using ETLWay.Infrastructure.Patterns.Transformers;

namespace ETLWay.Logic.Actors
{
    /// <summary>
    /// Generated Pipeline Actor - Orchestrates ETL workflow with Akka.NET
    /// Production-ready with comprehensive logging and error handling
    /// </summary>
    public class GeneratedPipelineActor : ReceiveActor
    {
        private readonly IZipExtractionService _zipService;
        private readonly IMessageBus _bus;
        private readonly IMessageResolver _messages;

        public GeneratedPipelineActor(IZipExtractionService zipService, IMessageBus bus, IMessageResolver messages)
        {
            _zipService = zipService ?? throw new ArgumentNullException(nameof(zipService));
            _bus = bus ?? throw new ArgumentNullException(nameof(bus));
            _messages = messages ?? throw new ArgumentNullException(nameof(messages));

            Console.WriteLine("[Actor] GeneratedPipelineActor initialized");

            ReceiveAsync<FileArrivedEvent>(async msg =>
            {
                try
                {
                    Console.WriteLine("");
                    Console.WriteLine("[Actor] ╔════════════════════════════════════════════════════════════");
                    Console.WriteLine($"[Actor] ║ FILE ARRIVED EVENT RECEIVED");
                    Console.WriteLine($"[Actor] ║ File: {msg.FileName}");
                    Console.WriteLine($"[Actor] ║ Path: {msg.FilePath}");
                    Console.WriteLine($"[Actor] ║ ExecutionId: {msg.ExecutionId ?? "NULL"}");
                    Console.WriteLine($"[Actor] ║ Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC");
                    Console.WriteLine("[Actor] ╚════════════════════════════════════════════════════════════");
                    Console.WriteLine("");

                    await RunPipeline(msg);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("");
                    Console.WriteLine("[Actor] ✗✗✗ CRITICAL ACTOR FAILURE ✗✗✗");
                    Console.WriteLine($"[Actor] Error: {ex.Message}");
                    Console.WriteLine($"[Actor] Type: {ex.GetType().Name}");
                    Console.WriteLine($"[Actor] StackTrace:");
                    Console.WriteLine(ex.StackTrace);
                    Console.WriteLine("");

                    // Try to publish error to UI even if everything else failed
                    try
                    {
                        await PublishScore(
                            msg.ExecutionId ?? Guid.NewGuid().ToString(),
                            msg.FileName ?? "Unknown",
                            0,
                            "Critical",
                            $"Actor Exception: {ex.Message}"
                        );
                    }
                    catch (Exception pubEx)
                    {
                        Console.WriteLine($"[Actor] ✗ Failed to publish error: {pubEx.Message}");
                    }
                }
            });
        }

        private async Task RunPipeline(FileArrivedEvent msg)
        {
            // Create shared context for the entire pipeline
            var context = new Application.Abstractions.Patterns.EtlContext
            {
                JobId = msg.ExecutionId ?? Guid.NewGuid().ToString()
            };

            Console.WriteLine($"[Actor] Created EtlContext with JobId: {context.JobId}");

            try
            {
                // 1. SEND START STATUS
                try
                {
                    Console.WriteLine($"[Actor] Publishing start status...");
                    await SendStatus(context.JobId, msg.FileName, "Extraction", "Processing", "#FFFF00");
                    Console.WriteLine($"[Actor] ✓ Start status published");
                }
                catch (Exception statusEx)
                {
                    Console.WriteLine($"[Actor] ⚠️ Failed to send start status: {statusEx.Message}");
                    // Continue anyway - don't fail the job for a UI update
                }

                // 2. CREATE PIPELINE
                Console.WriteLine("");
                Console.WriteLine($"[Actor] Creating pipeline...");

                LinearPipeline<FileMetadataDto, FileMetadataDto> pipeline;

                try
                {
                    pipeline = new LinearPipeline<FileMetadataDto, FileMetadataDto>(
                        new ZipFileExtractor(_zipService, msg.FilePath, @"C:\BahyWay\Processing"),
                        new FuzzySchemaValidator(_messages, strictness: 50.0),
                        new StagingTableLoader(),
                        context
                    );
                    Console.WriteLine($"[Actor] ✓ Pipeline created successfully");
                }
                catch (Exception pipeEx)
                {
                    Console.WriteLine($"[Actor] ✗ Pipeline creation failed: {pipeEx.Message}");
                    throw;
                }

                // 3. EXECUTE PIPELINE
                Console.WriteLine($"[Actor] Executing pipeline...");
                Console.WriteLine("");

                try
                {
                    await pipeline.ExecuteAsync();
                    Console.WriteLine($"[Actor] ✓ Pipeline execution completed");
                }
                catch (Exception execEx)
                {
                    Console.WriteLine($"[Actor] ✗ Pipeline execution failed: {execEx.Message}");

                    // Add to context if not already there
                    if (!context.Errors.Contains(execEx.Message))
                    {
                        context.Errors.Add($"Pipeline execution failed: {execEx.Message}");
                    }
                    context.QualityScore = 0;
                }

                // 4. FINAL DECISION LOGIC
                Console.WriteLine("");
                Console.WriteLine($"[Actor] ═══════════════════════════════════════════");
                Console.WriteLine($"[Actor] FINAL DECISION");
                Console.WriteLine($"[Actor] Quality Score: {context.QualityScore}%");
                Console.WriteLine($"[Actor] Error Count: {context.Errors.Count}");
                Console.WriteLine($"[Actor] ═══════════════════════════════════════════");

                if (context.QualityScore < 50.0)
                {
                    // CRITICAL FAILURE
                    Console.WriteLine($"[Actor] Decision: CRITICAL FAILURE (Score < 50%)");
                    await HandleCriticalFailure(context, msg);
                }
                else if (context.Metadata.ContainsKey("RequiresApproval"))
                {
                    // WARNING - NEEDS APPROVAL
                    Console.WriteLine($"[Actor] Decision: WARNING - Requires Approval (50% <= Score < 90%)");

                    try
                    {
                        await SendStatus(context.JobId, msg.FileName, "Validation", "Warning", "#FFA500");
                        await PublishScore(context.JobId, msg.FileName, context.QualityScore,
                            "Warning", "Schema Match < 90%. Approval Required.");
                        Console.WriteLine($"[Actor] ✓ Warning status published");
                    }
                    catch (Exception warnEx)
                    {
                        Console.WriteLine($"[Actor] ✗ Failed to publish warning: {warnEx.Message}");
                    }
                }
                else
                {
                    // SUCCESS
                    Console.WriteLine($"[Actor] Decision: SUCCESS (Score >= 90%)");

                    try
                    {
                        await SendStatus(context.JobId, msg.FileName, "StagingDB", "Success", "#00FF00");
                        await PublishScore(context.JobId, msg.FileName, context.QualityScore,
                            "Excellent", "Processing Complete.");
                        Console.WriteLine($"[Actor] ✓ Success status published");
                    }
                    catch (Exception successEx)
                    {
                        Console.WriteLine($"[Actor] ✗ Failed to publish success: {successEx.Message}");
                    }
                }

                Console.WriteLine("");
                Console.WriteLine($"[Actor] RunPipeline completed for {msg.FileName}");
                Console.WriteLine("");
            }
            catch (Exception ex)
            {
                // CATCH-ALL FOR ANY UNEXPECTED ERRORS
                Console.WriteLine("");
                Console.WriteLine("[Actor] ✗✗✗ PIPELINE CRASHED ✗✗✗");
                Console.WriteLine($"[Actor] Error: {ex.Message}");
                Console.WriteLine($"[Actor] Type: {ex.GetType().Name}");
                Console.WriteLine($"[Actor] StackTrace:");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("");

                // Add exception to context
                if (!context.Errors.Contains(ex.Message))
                {
                    context.Errors.Add($"SYSTEM EXCEPTION: {ex.Message}");
                }
                context.QualityScore = 0;

                // Report failure to UI
                await HandleCriticalFailure(context, msg);
            }
        }

        private async Task HandleCriticalFailure(Application.Abstractions.Patterns.EtlContext context, FileArrivedEvent msg)
        {
            try
            {
                Console.WriteLine($"[Actor] Handling critical failure...");

                await SendStatus(context.JobId, msg.FileName, "ErrorQueue", "Failed", "#FF0000");

                // Extract error text
                string errorText;
                if (context.Errors.Count > 0)
                {
                    errorText = string.Join("\n", context.Errors);
                    Console.WriteLine($"[Actor] Error text ({context.Errors.Count} errors):");
                    for (int i = 0; i < context.Errors.Count; i++)
                    {
                        Console.WriteLine($"[Actor]   {i + 1}. {context.Errors[i]}");
                    }
                }
                else
                {
                    errorText = "Unknown Critical Failure - No error details available";
                    Console.WriteLine($"[Actor] ⚠️ No errors in context! Using generic message");
                }

                // Send to UI
                await PublishScore(context.JobId, msg.FileName, context.QualityScore, "Critical", errorText);

                Console.WriteLine($"[Actor] ✓ Critical failure handled");
            }
            catch (Exception failEx)
            {
                Console.WriteLine($"[Actor] ✗ Failed to handle critical failure: {failEx.Message}");
                Console.WriteLine($"[Actor] This is bad - error handling failed!");
            }
        }

        private async Task SendStatus(string id, string file, string node, string status, string color)
        {
            try
            {
                Console.WriteLine($"[Actor] Sending status: {file} → {node} ({status})");

                await _bus.PublishParticleAsync("Flow.Particles", new FlowParticleEvent
                {
                    ExecutionId = id,
                    FileName = file,
                    ToNode = node,
                    Status = status,
                    ColorHex = color,
                    Timestamp = DateTime.UtcNow
                });

                Console.WriteLine($"[Actor] ✓ Status sent successfully");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[Actor] ✗ SendStatus failed: {ex.Message}");
                // Don't throw - we don't want status updates to crash the actor
            }
        }

        private async Task PublishScore(string id, string source, double score, string status, string message = "")
        {
            try
            {
                // Fallback for empty message
                if (string.IsNullOrWhiteSpace(message))
                {
                    message = "System Error: No details provided.";
                    Console.WriteLine($"[Actor] ⚠️ Empty message - using fallback");
                }

                // DEBUG LOGGING - CRITICAL FOR TROUBLESHOOTING
                Console.WriteLine("");
                Console.WriteLine("[Actor] ╔════════════════════════════════════════════════════════════");
                Console.WriteLine("[Actor] ║ PUBLISHING SCORE TO REDIS");
                Console.WriteLine($"[Actor] ║ ExecutionId: {id}");
                Console.WriteLine($"[Actor] ║ Source: {source}");
                Console.WriteLine($"[Actor] ║ Score: {score}%");
                Console.WriteLine($"[Actor] ║ Status: {status}");
                Console.WriteLine($"[Actor] ║ Message: {message}");
                Console.WriteLine($"[Actor] ║ Topic: Dashboard.Scores");
                Console.WriteLine("[Actor] ╚════════════════════════════════════════════════════════════");

                await _bus.PublishParticleAsync("Dashboard.Scores", new ScoreUpdateEvent
                {
                    ExecutionId = id,
                    SourceSystem = source,
                    QualityScore = score,
                    OverallStatus = status,
                    Message = message
                });

                Console.WriteLine("[Actor] ✓✓✓ Score published successfully to Redis");
                Console.WriteLine("");
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[Actor] ✗✗✗ FAILED TO PUBLISH SCORE ✗✗✗");
                Console.WriteLine($"[Actor] Error: {ex.Message}");
                Console.WriteLine($"[Actor] Type: {ex.GetType().Name}");
                Console.WriteLine($"[Actor] StackTrace:");
                Console.WriteLine(ex.StackTrace);
                Console.WriteLine("");

                // Don't throw - we don't want publish failures to crash the actor
                // But log it prominently so we know something is wrong
            }
        }
    }
}
