using System.Collections.Generic;

namespace BahyWay.SharedKernel.Application.DTOs
{
    public class FileMetadataDto
    {
        // Identification
        public string OriginalFileName { get; set; }
        public string GeneratedDataFileName { get; set; } // The CSV/Excel extracted
        public string GeneratedFormatFileName { get; set; } // The JSON Schema

        // The "Shape" Statistics
        public long RowCount { get; set; }
        public long FileSizeBytes { get; set; }
        public string FormatHash { get; set; } // SHA256 Hash for change detection

        // Detailed Structure
        public List<FileColumnDefinition> Columns { get; set; } = new();
    }

    public class FileColumnDefinition
    {
        public string ColumnName { get; set; }
        public string EstimatedDataType { get; set; } // "Int", "String", "DateTime"
        public int OrdinalPosition { get; set; }
    }
}