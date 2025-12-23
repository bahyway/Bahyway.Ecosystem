using MessagePack; // <--- This is required

namespace BahyWay.SharedKernel.Application.DTOs
{
    [MessagePackObject] // <--- Mark the class
    public class PipelineRuleConfig
    {
        // =================================================
        // GROUP 1: PHYSICAL HEALTH (Free Tier)
        // =================================================
        [Key(0)]
        public bool CheckCorruption { get; set; } = true;

        [Key(1)]
        public bool CheckEmptyArchive { get; set; } = true;

        [Key(2)]
        public bool CheckFileSize { get; set; } = true;

        [Key(3)]
        public long MinSizeBytes { get; set; } = 1024;

        // =================================================
        // GROUP 2: SCHEMA INTEGRITY (Standard Tier)
        // =================================================
        [Key(4)]
        public bool EnforceColumnNames { get; set; } = true;

        [Key(5)]
        public bool EnforceColumnOrder { get; set; } = false;

        [Key(6)]
        public bool AllowSchemaDrift { get; set; } = false;

        // =================================================
        // GROUP 3: DEEP INSPECTION (Enterprise / Premium)
        // =================================================
        [Key(7)]
        public bool AnalyzeNulls { get; set; } = true;

        [Key(8)]
        public bool StrictSLA { get; set; } = false;

        [Key(9)]
        public int MaxFileNameLength { get; set; } = 50;
    }
}