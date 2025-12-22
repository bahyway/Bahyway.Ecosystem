using System;
using BahyWay.SharedKernel.Application.DTOs; // <--- Add this using

namespace BahyWay.SharedKernel.Application.Events
{
    // 1. Define the Modes
    public enum ProcessingMode
    {
        FullAutomation,
        HumanControl
    }

    // 2. The Particle (Visuals)
    public class FlowParticleEvent
    {
        public string ExecutionId { get; set; }
        public string FileName { get; set; }
        public string SourceSystem { get; set; }
        public string FileType { get; set; }
        public string FromNode { get; set; }
        public string ToNode { get; set; }
        public string Status { get; set; }
        public string ColorHex { get; set; }
        public DateTime Timestamp { get; set; }
    }

    // 3. The Work Payload (Logic)
    public class FileArrivedEvent
    {
        public string ExecutionId { get; set; } // <--- CRITICAL: Passes the ID
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public long FileSizeBytes { get; set; }
        public DateTime Timestamp { get; set; }
        public ProcessingMode Mode { get; set; }
        // --- NEW: The Configuration Payload ---
        public PipelineRuleConfig RuleConfig { get; set; } = new();
    }

    // 4. Score Updates
    public class ScoreUpdateEvent
    {
        public string ExecutionId { get; set; } // <--- CRITICAL: Matches the ID
        public string SourceSystem { get; set; }
        public double QualityScore { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public string OverallStatus { get; set; }
        public string Message { get; set; } = ""; // Initialize it
    }

    // 5. Resume Command
    public class ResumeJobCommand
    {
        public string ExecutionId { get; set; }
        public string Action { get; set; }
        public string User { get; set; }
    }
}