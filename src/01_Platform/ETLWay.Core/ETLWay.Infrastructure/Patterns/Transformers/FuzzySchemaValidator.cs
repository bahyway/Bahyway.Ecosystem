using System;
using System.Linq;
using System.Threading.Tasks;
using BahyWay.SharedKernel.Application.Abstractions;
using BahyWay.SharedKernel.Application.DTOs;
using ETLWay.Application.Abstractions.Patterns;

namespace ETLWay.Infrastructure.Patterns.Transformers
{
    /// <summary>
    /// Fuzzy Schema Validator - Validates CSV schema against expected format
    /// Production-ready with comprehensive logging and error handling
    /// </summary>
    public class FuzzySchemaValidator : ITransformer<FileMetadataDto, FileMetadataDto>
    {
        private readonly double _strictnessThreshold;
        private readonly IMessageResolver _messages;

        public FuzzySchemaValidator(IMessageResolver messages, double strictness = 0.9)
        {
            _messages = messages ?? throw new ArgumentNullException(nameof(messages));
            _strictnessThreshold = strictness;

            Console.WriteLine($"[Validator] Initialized with strictness threshold: {_strictnessThreshold}%");
        }

        public Task<FileMetadataDto?> TransformAsync(FileMetadataDto input, EtlContext context)
        {
            try
            {
                Console.WriteLine("");
                Console.WriteLine("[Validator] ══════════════════════════════════════════════════");
                Console.WriteLine("[Validator] FUZZY SCHEMA VALIDATOR STARTED");
                Console.WriteLine($"[Validator] JobId: {context.JobId}");
                Console.WriteLine($"[Validator] Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC");
                Console.WriteLine("[Validator] ══════════════════════════════════════════════════");

                // Validate input
                if (input == null)
                {
                    Console.WriteLine("[Validator] ✗ ERROR: Input is NULL");
                    context.Errors.Add("Validator received null input");
                    context.QualityScore = 0;
                    return Task.FromResult<FileMetadataDto?>(null);
                }

                if (input.Columns == null || input.Columns.Count == 0)
                {
                    Console.WriteLine("[Validator] ✗ ERROR: No columns found in input");
                    Console.WriteLine($"[Validator] Columns Count: {input.Columns?.Count ?? 0}");
                    context.Errors.Add("No columns detected in file");
                    context.QualityScore = 0;
                    return Task.FromResult<FileMetadataDto?>(null);
                }

                // Log file information
                Console.WriteLine($"[Validator] File: {input.OriginalFileName}");
                Console.WriteLine($"[Validator] Size: {input.FileSizeBytes:N0} bytes");
                Console.WriteLine($"[Validator] Rows: {input.RowCount:N0}");
                Console.WriteLine($"[Validator] Columns: {input.Columns.Count}");
                Console.WriteLine("");

                // Display column list
                Console.WriteLine("[Validator] Column Analysis:");
                Console.WriteLine("[Validator] ┌────────────────────────────────────────");

                int displayCount = Math.Min(input.Columns.Count, 15);
                for (int i = 0; i < displayCount; i++)
                {
                    var col = input.Columns[i];
                    Console.WriteLine($"[Validator] │ {i + 1:D2}. {col.ColumnName,-30} " +
                                    $"({col.EstimatedDataType}, Max:{col.MaxLength}, " +
                                    $"Nulls:{(col.HasNulls ? "Yes" : "No")})");
                }

                if (input.Columns.Count > displayCount)
                {
                    Console.WriteLine($"[Validator] │ ... and {input.Columns.Count - displayCount} more columns");
                }

                Console.WriteLine("[Validator] └────────────────────────────────────────");
                Console.WriteLine("");

                // Initialize score
                double score = 100.0;
                Console.WriteLine($"[Validator] Initial Score: {score}%");
                Console.WriteLine("");

                // RULE 1: Check for UUID column
                Console.WriteLine("[Validator] ┌─ RULE 1: UUID Column Check");

                try
                {
                    bool hasUuid = input.Columns.Exists(c =>
                        c.ColumnName.ToLower().Contains("uuid"));

                    Console.WriteLine($"[Validator] │  Searching for 'uuid' column...");
                    Console.WriteLine($"[Validator] │  Result: {(hasUuid ? "✓ FOUND" : "✗ NOT FOUND")}");

                    if (!hasUuid)
                    {
                        double penalty = 60.0;
                        score -= penalty;

                        string error = "Schema Mismatch: Critical Column 'uuid' is missing.";
                        context.Errors.Add(error);

                        Console.WriteLine($"[Validator] │  ✗ UUID column is MISSING!");
                        Console.WriteLine($"[Validator] │  Penalty Applied: -{penalty} points");
                        Console.WriteLine($"[Validator] │  New Score: {score}%");
                        Console.WriteLine($"[Validator] │  Error Logged: {error}");
                    }
                    else
                    {
                        var uuidColumn = input.Columns.First(c =>
                            c.ColumnName.ToLower().Contains("uuid"));

                        Console.WriteLine($"[Validator] │  ✓ UUID column found: '{uuidColumn.ColumnName}'");
                        Console.WriteLine($"[Validator] │  Type: {uuidColumn.EstimatedDataType}");
                        Console.WriteLine($"[Validator] │  No penalty applied");
                    }
                }
                catch (Exception ruleEx)
                {
                    Console.WriteLine($"[Validator] │  ✗ ERROR in UUID check: {ruleEx.Message}");
                    context.Errors.Add($"UUID validation error: {ruleEx.Message}");
                    score -= 50.0;
                }

                Console.WriteLine("[Validator] └─ Rule 1 Complete");
                Console.WriteLine("");

                // Save final score
                context.QualityScore = Math.Max(0, score);
                Console.WriteLine($"[Validator] Final Quality Score: {context.QualityScore}%");
                Console.WriteLine($"[Validator] Threshold: 50%");
                Console.WriteLine("");

                // THRESHOLD CHECK
                if (context.QualityScore < 50.0)
                {
                    Console.WriteLine("[Validator] ✗✗✗ CRITICAL FAILURE ✗✗✗");
                    Console.WriteLine($"[Validator] Score {context.QualityScore}% < 50% threshold");
                    Console.WriteLine("");

                    // Ensure we have an error message
                    if (context.Errors.Count == 0)
                    {
                        try
                        {
                            string msg = _messages.GetError("ERR_SCORE_CRITICAL", score, 50.0);
                            context.Errors.Add(msg);
                            Console.WriteLine($"[Validator] Generic error added: {msg}");
                        }
                        catch (Exception msgEx)
                        {
                            string fallback = $"Critical quality failure: Score {context.QualityScore}% below threshold";
                            context.Errors.Add(fallback);
                            Console.WriteLine($"[Validator] Message resolver failed: {msgEx.Message}");
                            Console.WriteLine($"[Validator] Fallback error added: {fallback}");
                        }
                    }

                    Console.WriteLine($"[Validator] Total Errors: {context.Errors.Count}");
                    Console.WriteLine("[Validator] Error List:");
                    for (int i = 0; i < context.Errors.Count; i++)
                    {
                        Console.WriteLine($"[Validator]   {i + 1}. {context.Errors[i]}");
                    }
                    Console.WriteLine("");

                    Console.WriteLine("[Validator] Decision: REJECT FILE");
                    Console.WriteLine("[Validator] Returning: NULL (stop pipeline)");
                    Console.WriteLine("[Validator] ══════════════════════════════════════════════════");
                    Console.WriteLine("");

                    return Task.FromResult<FileMetadataDto?>(null); // Stop Pipeline
                }

                // APPROVAL CHECK (50-90%)
                if (context.QualityScore < 90.0)
                {
                    Console.WriteLine("[Validator] ⚠️ WARNING: Score below 90%");
                    Console.WriteLine("[Validator] Action: Flagging for manual approval");
                    context.Metadata["RequiresApproval"] = true;
                    Console.WriteLine("");
                }

                // VALIDATION PASSED
                Console.WriteLine("[Validator] ✓✓✓ VALIDATION PASSED ✓✓✓");
                Console.WriteLine($"[Validator] Score {context.QualityScore}% >= 50% threshold");
                Console.WriteLine("[Validator] Decision: ACCEPT FILE");
                Console.WriteLine("[Validator] Returning: FileMetadataDto (continue pipeline)");
                Console.WriteLine("[Validator] ══════════════════════════════════════════════════");
                Console.WriteLine("");

                return Task.FromResult<FileMetadataDto?>(input);
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[Validator] ✗✗✗ VALIDATOR EXCEPTION ✗✗✗");
                Console.WriteLine($"[Validator] Error: {ex.Message}");
                Console.WriteLine($"[Validator] Type: {ex.GetType().Name}");
                Console.WriteLine($"[Validator] Source: {ex.Source}");
                Console.WriteLine($"[Validator] StackTrace:");
                Console.WriteLine(ex.StackTrace);

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[Validator] Inner Exception: {ex.InnerException.Message}");
                }

                Console.WriteLine("[Validator] ══════════════════════════════════════════════════");
                Console.WriteLine("");

                // Log error in context
                context.Errors.Add($"Validation exception: {ex.Message}");
                context.QualityScore = 0;

                // Return null to stop pipeline
                return Task.FromResult<FileMetadataDto?>(null);
            }
        }
    }
}
