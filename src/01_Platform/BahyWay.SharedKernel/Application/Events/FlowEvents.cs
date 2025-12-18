using System;

namespace BahyWay.SharedKernel.Application.Events
{
    // 1. The Particle (The Journey)
    public class FlowParticleEvent
    {
        public string ExecutionId { get; set; }
        public string FileName { get; set; }

        // --- NEW METADATA FOR SCORING ---
        public string SourceSystem { get; set; }  // e.g., "Najaf", "WPD", "Baghdad"
        public string FileType { get; set; }      // e.g., "Census", "Sensor", "Invoice"
        // --------------------------------

        public string FromNode { get; set; }
        public string ToNode { get; set; }
        public string Status { get; set; }      // "Moving", "Success", "Error"
        public string ColorHex { get; set; }
        public DateTime Timestamp { get; set; }
    }

    // 2. The Work Payload (The Trigger)
    public class FileArrivedEvent
    {
        public string FilePath { get; set; }
        public string FileName { get; set; }
        public long FileSizeBytes { get; set; }
        public DateTime Timestamp { get; set; }
    }

    // 3. The Score Update (The Dashboard Data) -- NEW --
    public class ScoreUpdateEvent
    {
        public string SourceSystem { get; set; }
        public double QualityScore { get; set; } // 0.0 to 100.0
        public int SuccessCount { get; set; }
        public int FailureCount { get; set; }
        public string OverallStatus { get; set; } // "Excellent", "Warning", "Critical"
    }
}