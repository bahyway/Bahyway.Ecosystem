using BahyWay.SharedKernel.Application.Abstractions;
using BahyWay.SharedKernel.Application.DTOs;
using ETLWay.Application.Abstractions.Patterns;
using System.Threading.Tasks;

namespace ETLWay.Infrastructure.Patterns.Transformers
{

    public class FuzzySchemaValidator : ITransformer<FileMetadataDto, FileMetadataDto>
    {
        // In reality, inject IRulesEngine here
        private readonly double _strictnessThreshold;
        private readonly IMessageResolver _messages; // Inject this

        // 2. Inject it in the Constructor
        public FuzzySchemaValidator(IMessageResolver messages, double strictness = 0.9)
        {
            _messages = messages;
            _strictnessThreshold = strictness;
            // ...
        }


        //    public Task<FileMetadataDto?> TransformAsync(FileMetadataDto input, EtlContext context)
        //    {
        //        // 1. Calculate Score (Simulation)
        //        double score = 100.0;

        //        // Rule 1: Must have columns
        //        if (input.Columns.Count == 0) score -= 100;

        //        // Rule 2: Critical Column "uuid" check
        //        bool hasId = input.Columns.Exists(c => c.ColumnName.ToLower().Contains("uuid"));
        //        if (!hasId) score -= 40.0; // Heavy penalty

        //        // Rule 3: Null check
        //        // If we detected nulls in critical columns during extraction
        //        // score -= 10.0;

        //        // 2. Save Score to Context
        //        context.QualityScore = Math.Max(0, score); // Cannot be negative

        //        // 3. Threshold Check (< 50% = Critical Failure)
        //        if (context.QualityScore < 50.0)
        //        {
        //        //context.Errors.Add($"CRITICAL: Quality Score {score}% is below threshold (50%).");
        //        //return Task.FromResult<FileMetadataDto?>(null); // Return Null to Stop Pipeline
        //        // NEW: Use the Code
        //        string msg = _messages.GetError("ERR_SCORE_CRITICAL", score, 50.0);
        //        context.Errors.Add(msg);

        //        return Task.FromResult<FileMetadataDto?>(null);
        //    }

        //        // 4. Warning Check (< 90% = Human Review needed)
        //        if (context.QualityScore < 90.0)
        //        {
        //            // We don't stop, but we flag it
        //            context.Metadata["RequiresApproval"] = true;
        //        }

        //        return Task.FromResult<FileMetadataDto?>(input);
        //    }
        //}
        public Task<FileMetadataDto?> TransformAsync(FileMetadataDto input, EtlContext context)
        {
            double score = 100.0;

            // 1. Logic: Check for UUID (The test case)
            bool hasId = input.Columns.Exists(c => c.ColumnName.ToLower().Contains("uuid"));

            if (!hasId)
            {
                score -= 60.0; // Penalty
                // Log detailed error for the UI
                context.Errors.Add("Schema Mismatch: Critical Column 'uuid' is missing.");
            }

            // 2. Save Score
            context.QualityScore = Math.Max(0, score);

            // 3. Threshold Check
            if (context.QualityScore < 50.0)
            {
                // If we haven't added a specific error yet, add a generic one using the Message Resolver
                if (context.Errors.Count == 0)
                {
                    string msg = _messages.GetError("ERR_SCORE_CRITICAL", score, 50.0);
                    context.Errors.Add(msg);
                }

                return Task.FromResult<FileMetadataDto?>(null); // Stop Pipeline
            }

            // 4. Warning
            if (context.QualityScore < 90.0)
            {
                context.Metadata["RequiresApproval"] = true;
            }

            return Task.FromResult<FileMetadataDto?>(input);
        }
    }
 }