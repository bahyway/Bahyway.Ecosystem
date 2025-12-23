using System;
using System.Threading.Tasks;
using ETLWay.Application.Abstractions.Patterns;

namespace ETLWay.Infrastructure.Patterns.Pipelines
{
    /// <summary>
    /// Linear ETL Pipeline - Executes Extract → Transform → Load in sequence
    /// Production-ready with comprehensive logging and error handling
    /// </summary>
    public class LinearPipeline<TSource, TTarget> : IPipeline
    {
        private readonly IExtractor<TSource> _extractor;
        private readonly ITransformer<TSource, TTarget> _transformer;
        private readonly ILoader<TTarget> _loader;
        private readonly EtlContext _context;

        public LinearPipeline(
            IExtractor<TSource> extractor,
            ITransformer<TSource, TTarget> transformer,
            ILoader<TTarget> loader,
            EtlContext context)
        {
            _extractor = extractor ?? throw new ArgumentNullException(nameof(extractor));
            _transformer = transformer ?? throw new ArgumentNullException(nameof(transformer));
            _loader = loader ?? throw new ArgumentNullException(nameof(loader));
            _context = context ?? throw new ArgumentNullException(nameof(context));

            Console.WriteLine("[Pipeline] Pipeline initialized successfully");
            Console.WriteLine($"[Pipeline] JobId: {_context.JobId}");
        }

        public async Task ExecuteAsync()
        {
            try
            {
                Console.WriteLine("");
                Console.WriteLine("[Pipeline] ══════════════════════════════════════════════════");
                Console.WriteLine("[Pipeline] STARTING PIPELINE EXECUTION");
                Console.WriteLine($"[Pipeline] JobId: {_context.JobId}");
                Console.WriteLine($"[Pipeline] Timestamp: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC");
                Console.WriteLine("[Pipeline] ══════════════════════════════════════════════════");

                int itemCount = 0;
                int successCount = 0;
                int failureCount = 0;

                // 1. EXTRACTION PHASE
                try
                {
                    Console.WriteLine("[Pipeline] ┌─ PHASE 1: EXTRACTION");

                    await foreach (var item in _extractor.ExtractAsync(_context))
                    {
                        itemCount++;
                        Console.WriteLine($"[Pipeline] │");
                        Console.WriteLine($"[Pipeline] ├─ Item #{itemCount} extracted");
                        Console.WriteLine($"[Pipeline] │  Type: {item?.GetType().Name ?? "NULL"}");

                        if (item == null)
                        {
                            Console.WriteLine($"[Pipeline] │  ⚠️ WARNING: Extractor returned NULL");
                            failureCount++;
                            continue;
                        }

                        // 2. TRANSFORMATION PHASE (Validation)
                        TTarget? transformed = default;

                        try
                        {
                            Console.WriteLine($"[Pipeline] │");
                            Console.WriteLine($"[Pipeline] ├─ PHASE 2: TRANSFORMATION (Validation)");
                            Console.WriteLine($"[Pipeline] │  Calling transformer...");

                            transformed = await _transformer.TransformAsync(item, _context);

                            if (transformed == null)
                            {
                                Console.WriteLine($"[Pipeline] │  ◄ Transformer returned: NULL");
                                Console.WriteLine($"[Pipeline] │  ⚠️ VALIDATION FAILED!");
                                Console.WriteLine($"[Pipeline] │  Quality Score: {_context.QualityScore}%");
                                Console.WriteLine($"[Pipeline] │  Errors Count: {_context.Errors.Count}");

                                if (_context.Errors.Count > 0)
                                {
                                    Console.WriteLine($"[Pipeline] │  Errors:");
                                    foreach (var error in _context.Errors)
                                    {
                                        Console.WriteLine($"[Pipeline] │    • {error}");
                                    }
                                }

                                failureCount++;
                                Console.WriteLine($"[Pipeline] │  ⛔ STOPPING PIPELINE - Validation failed");
                                break; // Stop processing further items
                            }
                            else
                            {
                                Console.WriteLine($"[Pipeline] │  ◄ Transformer returned: VALID OBJECT");
                                Console.WriteLine($"[Pipeline] │  ✓ VALIDATION PASSED");
                                Console.WriteLine($"[Pipeline] │  Quality Score: {_context.QualityScore}%");
                            }
                        }
                        catch (Exception transformEx)
                        {
                            Console.WriteLine($"[Pipeline] │  ✗✗✗ TRANSFORMATION ERROR!");
                            Console.WriteLine($"[Pipeline] │  Error: {transformEx.Message}");
                            Console.WriteLine($"[Pipeline] │  Type: {transformEx.GetType().Name}");
                            Console.WriteLine($"[Pipeline] │  StackTrace: {transformEx.StackTrace}");

                            _context.Errors.Add($"Transformation failed: {transformEx.Message}");
                            _context.QualityScore = 0;
                            failureCount++;

                            throw; // Re-throw to be caught by outer try-catch
                        }

                        // 3. LOADING PHASE
                        if (transformed != null)
                        {
                            try
                            {
                                Console.WriteLine($"[Pipeline] │");
                                Console.WriteLine($"[Pipeline] ├─ PHASE 3: LOADING");
                                Console.WriteLine($"[Pipeline] │  Calling loader...");

                                await _loader.LoadAsync(transformed, _context);

                                Console.WriteLine($"[Pipeline] │  ◄ Loader complete");
                                Console.WriteLine($"[Pipeline] │  ✓ LOAD SUCCESSFUL");
                                successCount++;
                            }
                            catch (Exception loadEx)
                            {
                                Console.WriteLine($"[Pipeline] │  ✗✗✗ LOADING ERROR!");
                                Console.WriteLine($"[Pipeline] │  Error: {loadEx.Message}");
                                Console.WriteLine($"[Pipeline] │  Type: {loadEx.GetType().Name}");
                                Console.WriteLine($"[Pipeline] │  StackTrace: {loadEx.StackTrace}");

                                _context.Errors.Add($"Loading failed: {loadEx.Message}");
                                failureCount++;

                                throw; // Re-throw to be caught by outer try-catch
                            }
                        }

                        Console.WriteLine($"[Pipeline] └─ Item #{itemCount} processing complete");
                    }
                }
                catch (Exception extractEx)
                {
                    Console.WriteLine($"[Pipeline] ✗✗✗ EXTRACTION ERROR!");
                    Console.WriteLine($"[Pipeline] Error: {extractEx.Message}");
                    Console.WriteLine($"[Pipeline] Type: {extractEx.GetType().Name}");
                    Console.WriteLine($"[Pipeline] StackTrace: {extractEx.StackTrace}");

                    _context.Errors.Add($"Extraction failed: {extractEx.Message}");
                    _context.QualityScore = 0;

                    throw; // Re-throw to be caught by outer try-catch
                }

                // FINAL SUMMARY
                Console.WriteLine("");
                Console.WriteLine("[Pipeline] ══════════════════════════════════════════════════");
                Console.WriteLine("[Pipeline] PIPELINE EXECUTION COMPLETE");
                Console.WriteLine($"[Pipeline] Total Items Processed: {itemCount}");
                Console.WriteLine($"[Pipeline] Successful: {successCount}");
                Console.WriteLine($"[Pipeline] Failed: {failureCount}");
                Console.WriteLine($"[Pipeline] Final Quality Score: {_context.QualityScore}%");
                Console.WriteLine($"[Pipeline] Total Errors: {_context.Errors.Count}");

                if (_context.Errors.Count > 0)
                {
                    Console.WriteLine("[Pipeline] Error Summary:");
                    for (int i = 0; i < _context.Errors.Count; i++)
                    {
                        Console.WriteLine($"[Pipeline]   {i + 1}. {_context.Errors[i]}");
                    }
                }
                else
                {
                    Console.WriteLine("[Pipeline] ✓ No errors - pipeline executed cleanly");
                }

                Console.WriteLine($"[Pipeline] Completed at: {DateTime.UtcNow:yyyy-MM-dd HH:mm:ss.fff} UTC");
                Console.WriteLine("[Pipeline] ══════════════════════════════════════════════════");
                Console.WriteLine("");
            }
            catch (Exception ex)
            {
                Console.WriteLine("");
                Console.WriteLine("[Pipeline] ✗✗✗ CRITICAL PIPELINE FAILURE ✗✗✗");
                Console.WriteLine($"[Pipeline] Error: {ex.Message}");
                Console.WriteLine($"[Pipeline] Type: {ex.GetType().Name}");
                Console.WriteLine($"[Pipeline] Source: {ex.Source}");
                Console.WriteLine($"[Pipeline] StackTrace:");
                Console.WriteLine(ex.StackTrace);

                if (ex.InnerException != null)
                {
                    Console.WriteLine($"[Pipeline] Inner Exception: {ex.InnerException.Message}");
                    Console.WriteLine($"[Pipeline] Inner StackTrace:");
                    Console.WriteLine(ex.InnerException.StackTrace);
                }

                Console.WriteLine("[Pipeline] ══════════════════════════════════════════════════");
                Console.WriteLine("");

                // Ensure error is recorded in context
                if (!_context.Errors.Contains(ex.Message))
                {
                    _context.Errors.Add($"Pipeline failure: {ex.Message}");
                }
                _context.QualityScore = 0;

                // Re-throw so Actor can catch it
                throw;
            }
        }
    }
}
