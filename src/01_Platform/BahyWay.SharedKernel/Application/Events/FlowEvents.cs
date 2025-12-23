using System;
using MessagePack; // <--- NEW NAMESPACE
using BahyWay.SharedKernel.Application.DTOs;

namespace BahyWay.SharedKernel.Application.Events
{
    public enum ProcessingMode
    {
        FullAutomation,
        HumanControl
    }

    [MessagePackObject]
    public class FlowParticleEvent
    {
        [Key(0)] public string ExecutionId { get; set; }
        [Key(1)] public string FileName { get; set; }
        [Key(2)] public string SourceSystem { get; set; }
        [Key(3)] public string FileType { get; set; }
        [Key(4)] public string FromNode { get; set; }
        [Key(5)] public string ToNode { get; set; }
        [Key(6)] public string Status { get; set; }
        [Key(7)] public string ColorHex { get; set; }
        [Key(8)] public DateTime Timestamp { get; set; }
    }

    [MessagePackObject]
    public class FileArrivedEvent
    {
        [Key(0)] public string ExecutionId { get; set; }
        [Key(1)] public string FilePath { get; set; }
        [Key(2)] public string FileName { get; set; }
        [Key(3)] public long FileSizeBytes { get; set; }
        [Key(4)] public DateTime Timestamp { get; set; }
        [Key(5)] public ProcessingMode Mode { get; set; }
        [Key(6)] public PipelineRuleConfig RuleConfig { get; set; }
    }

    [MessagePackObject]
    public class ScoreUpdateEvent
    {
        [Key(0)] public string ExecutionId { get; set; }
        [Key(1)] public string SourceSystem { get; set; }
        [Key(2)] public double QualityScore { get; set; }
        [Key(3)] public int SuccessCount { get; set; }
        [Key(4)] public int FailureCount { get; set; }
        [Key(5)] public string OverallStatus { get; set; }

        // This is the field that was failing in JSON.
        // In MessagePack, Key(6) guarantees it arrives.
        [Key(6)] public string Message { get; set; }
    }
}