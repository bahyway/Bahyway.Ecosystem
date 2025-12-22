namespace BahyWay.SharedKernel.Application.DTOs
{
    public class PipelineRuleConfig
    {
        // =================================================
        // GROUP 1: PHYSICAL HEALTH (Free Tier)
        // =================================================
        public bool CheckCorruption { get; set; } = true;
        public bool CheckEmptyArchive { get; set; } = true;

        // "File Size Slider" in UI
        public bool CheckFileSize { get; set; } = true;
        public long MinSizeBytes { get; set; } = 1024; // Default 1KB

        // =================================================
        // GROUP 2: SCHEMA INTEGRITY (Standard Tier)
        // =================================================
        public bool EnforceColumnNames { get; set; } = true;

        // "Strict Mode" Toggle
        public bool EnforceColumnOrder { get; set; } = false;
        public bool AllowSchemaDrift { get; set; } = false;

        // =================================================
        // GROUP 3: DEEP INSPECTION (Enterprise / Premium)
        // =================================================
        // Expensive CPU operation - requires license
        public bool AnalyzeNulls { get; set; } = true;

        // "SLA Compliance" Toggle
        public bool StrictSLA { get; set; } = false;
        public int MaxFileNameLength { get; set; } = 50;
    }
}