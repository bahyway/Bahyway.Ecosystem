using System;
using System.Collections.Generic;
using System.Threading.Tasks;

// Namespace matches the project structure
namespace ETLWay.Application.Abstractions.Patterns
{
    // The "Context" that flows through the pipeline
    public class EtlContext
    {
        public string JobId { get; set; } = Guid.NewGuid().ToString();
        public int AttemptNumber { get; set; } = 1; // Track Retries
        public double QualityScore { get; set; } = 100.0; // Starts at 100, drops on errors

        public Dictionary<string, object> Metadata { get; set; } = new();
        public List<string> Errors { get; set; } = new();
        public bool IsSuccess => Errors.Count == 0;
    }

    // 1. EXTRACTOR: Gets data (Zip, CSV, SQL)
    public interface IExtractor<TOut>
    {
        IAsyncEnumerable<TOut> ExtractAsync(EtlContext context);
    }

    // 2. TRANSFORMER: Changes data (Fuzzy Logic, Validation)
    public interface ITransformer<TIn, TOut>
    {
        Task<TOut?> TransformAsync(TIn input, EtlContext context);
    }

    // 3. LOADER: Saves data (Postgres, Redis)
    public interface ILoader<TIn>
    {
        Task LoadAsync(TIn input, EtlContext context);
    }

    // 4. PIPELINE: The Orchestrator
    public interface IPipeline
    {
        Task ExecuteAsync();
    }
}