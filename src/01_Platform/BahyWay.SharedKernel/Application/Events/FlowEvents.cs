using System;
using BahyWay.SharedKernel.Application.DTOs;

namespace BahyWay.SharedKernel.Application.Events
{
    public enum ProcessingMode
    {
        FullAutomation,
        HumanControl
    }

    // Pure POCOs - No Attributes
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

    public class FileArrivedEvent
    {
        public string ExecutionId { get; set; }
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public long FileSizeBytes { get; set; }
        public DateTime Timestamp { get; set; }
        public ProcessingMode Mode { get; set; }
        public PipelineRuleConfig RuleConfig { get; set; }
    }

    public class ScoreUpdateEvent
    {
        public string ExecutionId { get; set; }
        public string SourceSystem { get; set; }
        public double QualityScore { get; set; }
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public string OverallStatus { get; set; }
        public string Message { get; set; }
    }
}