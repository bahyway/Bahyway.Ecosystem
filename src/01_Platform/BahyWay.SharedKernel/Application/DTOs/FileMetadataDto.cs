using System.Collections.Generic;

namespace BahyWay.SharedKernel.Application.DTOs
{
    public class FileMetadataDto
    {
        public string OriginalFileName { get; set; }
        public string GeneratedDataFileName { get; set; }   // Path to .csv
        public string GeneratedFormatFileName { get; set; } // Path to .json

        public long RowCount { get; set; }
        public long FileSizeBytes { get; set; }
        public string FormatHash { get; set; }

        public List<FileColumnDefinition> Columns { get; set; } = new();
    }

    public class FileColumnDefinition
    {
        public string ColumnName { get; set; }
        public int OrdinalPosition { get; set; }

        // --- NEW ANALYSIS FIELDS ---
        public string EstimatedDataType { get; set; } // Int, String, etc.
        public int MaxLength { get; set; }            // Max char length found
        public bool HasNulls { get; set; }            // Found any empty values?
    }
}