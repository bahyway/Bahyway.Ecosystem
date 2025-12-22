using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Text.Json;
using BahyWay.SharedKernel.Application.Abstractions;
using BahyWay.SharedKernel.Application.DTOs;
using SharpCompress.Archives;
using SharpCompress.Common;

namespace BahyWay.SharedKernel.Infrastructure.FileStorage
{
    public class ZipExtractionService : IZipExtractionService
    {
        public async Task<FileMetadataDto> ExtractAndAnalyzeAsync(string zipFilePath, string baseOutputFolder)
        {
            var result = new FileMetadataDto();

            // 1. Create the Unique Folder Name: {ZipName}_{Timestamp}
            var zipName = Path.GetFileNameWithoutExtension(zipFilePath);
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
            var uniqueFolderName = $"{zipName}_{timestamp}";
            var targetFolder = Path.Combine(baseOutputFolder, uniqueFolderName);

            // Create the folder
            Directory.CreateDirectory(targetFolder);

            result.OriginalFileName = Path.GetFileName(zipFilePath);

            using (var archive = ArchiveFactory.Open(zipFilePath))
            {
                // Find the largest file (Assuming it's the main data CSV)
                var entry = archive.Entries.Where(e => !e.IsDirectory)
                                           .OrderByDescending(x => x.Size)
                                           .FirstOrDefault();

                if (entry == null) throw new Exception("Zip file is empty.");

                result.FileSizeBytes = entry.Size;

                // 2. Extract the Data File
                // We keep the original name but prepend timestamp to avoid collisions inside the folder
                var extension = Path.GetExtension(entry.Key);
                var cleanName = Path.GetFileNameWithoutExtension(entry.Key);
                var finalFileName = $"{timestamp}_{cleanName}{extension}";
                var fullOutputPath = Path.Combine(targetFolder, finalFileName);

                result.GeneratedDataFileName = fullOutputPath; // Save full path for next steps

                // Physical Extraction
                entry.WriteToDirectory(targetFolder, new ExtractionOptions { ExtractFullPath = false, Overwrite = true });

                // Rename to timestamped name
                var extractedPath = Path.Combine(targetFolder, entry.Key);
                if (File.Exists(fullOutputPath)) File.Delete(fullOutputPath);
                File.Move(extractedPath, fullOutputPath);

                // 3. Analyze the File (Metadata Extraction)
                if (extension.ToLower() == ".csv")
                {
                    await AnalyzeCsvDeeply(fullOutputPath, result);
                }
            }

            // 4. Save the Metadata as a JSON file in the same folder
            var metaJsonName = $"{timestamp}_{zipName}_metadata.json";
            var metaPath = Path.Combine(targetFolder, metaJsonName);
            var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
            var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(result, jsonOptions);
            await File.WriteAllBytesAsync(metaPath, jsonBytes);

            result.GeneratedFormatFileName = metaPath;

            return result;
        }

        private async Task AnalyzeCsvDeeply(string filePath, FileMetadataDto result)
        {
            // We read the file line by line to calculate stats without loading 1GB into RAM
            using var reader = new StreamReader(filePath);

            // A. Read Header
            var headerLine = await reader.ReadLineAsync();
            if (string.IsNullOrEmpty(headerLine)) return;

            var headers = headerLine.Split(','); // Assuming standard CSV

            // Initialize Column Definitions
            for (int i = 0; i < headers.Length; i++)
            {
                result.Columns.Add(new FileColumnDefinition
                {
                    ColumnName = headers[i].Trim(),
                    OrdinalPosition = i,
                    EstimatedDataType = "Unknown", // Will detect below
                    HasNulls = false,
                    MaxLength = 0
                });
            }

            // B. Sample Data for Type & Constraint Analysis (Read first 1000 rows)
            int rowsToSample = 1000;
            int rowsRead = 0;
            string line;

            while ((line = await reader.ReadLineAsync()) != null)
            {
                rowsRead++;
                var values = line.Split(','); // Basic split (production needs proper CSV parser for quotes)

                // Update Column Stats
                for (int i = 0; i < values.Length && i < result.Columns.Count; i++)
                {
                    var col = result.Columns[i];
                    var val = values[i].Trim();

                    // Check Length
                    if (val.Length > col.MaxLength) col.MaxLength = val.Length;

                    // Check Nulls
                    if (string.IsNullOrEmpty(val)) col.HasNulls = true;

                    // Detect Type (Simple Heuristic)
                    if (rowsRead < 100) // Only check type on first 100 rows to save CPU
                    {
                        col.EstimatedDataType = DetectType(val, col.EstimatedDataType);
                    }
                }

                // If we are just counting rows now, we can skip logic to speed up
                // But for 1M rows, reading the whole stream is fast enough in C#
            }

            result.RowCount = rowsRead;
        }

        private string DetectType(string value, string currentGuess)
        {
            if (string.IsNullOrEmpty(value)) return currentGuess;
            if (long.TryParse(value, out _)) return "Integer";
            if (double.TryParse(value, out _)) return "Decimal";
            if (DateTime.TryParse(value, out _)) return "DateTime";
            return "String";
        }
    }
}