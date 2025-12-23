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

            try
            {
                // 1. Create the Unique Folder Name: {ZipName}_{Timestamp}
                var zipName = Path.GetFileNameWithoutExtension(zipFilePath);
                var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");
                var uniqueFolderName = $"{zipName}_{timestamp}";
                var targetFolder = Path.Combine(baseOutputFolder, uniqueFolderName);

                // Create the folder
                Directory.CreateDirectory(targetFolder);
                Console.WriteLine($"[ETLWay] Created folder: {targetFolder}");

                result.OriginalFileName = Path.GetFileName(zipFilePath);

                using (var archive = ArchiveFactory.Open(zipFilePath))
                {
                    // Find the largest file (Assuming it's the main data CSV)
                    var entry = archive.Entries.Where(e => !e.IsDirectory)
                                               .OrderByDescending(x => x.Size)
                                               .FirstOrDefault();

                    if (entry == null)
                        throw new Exception("Zip file is empty - no files found.");

                    result.FileSizeBytes = entry.Size;

                    // 2. Extract the Data File
                    var extension = Path.GetExtension(entry.Key);

                    // FIX: Use Path.GetFileName to strip folder path from entry.Key
                    var fileName = Path.GetFileName(entry.Key);
                    var cleanName = Path.GetFileNameWithoutExtension(fileName);
                    var finalFileName = $"{timestamp}_{cleanName}{extension}";
                    var fullOutputPath = Path.Combine(targetFolder, finalFileName);

                    result.GeneratedDataFileName = fullOutputPath; // Save full path for next steps

                    Console.WriteLine($"[ETLWay] ?? Started processing {zipName}.zip...");
                    Console.WriteLine($"[ETLWay] ?? Unzipping '{entry.Key}' → '{fileName}'...");

                    // Physical Extraction (flat - no subfolders)
                    entry.WriteToDirectory(targetFolder, new ExtractionOptions
                    {
                        ExtractFullPath = false,  // Extract flat, no folder structure
                        Overwrite = true
                    });

                    // FIX: Use just the filename (not the full entry.Key which includes folders)
                    var extractedPath = Path.Combine(targetFolder, fileName);

                    // Verify the file was actually extracted
                    if (!File.Exists(extractedPath))
                    {
                        // Detailed error message for debugging
                        var filesInFolder = Directory.GetFiles(targetFolder);
                        var fileList = string.Join(", ", filesInFolder.Select(f => Path.GetFileName(f)));

                        throw new FileNotFoundException(
                            $"Extraction failed: Expected '{fileName}' at '{extractedPath}'. " +
                            $"Files in folder: [{fileList}]",
                            extractedPath);
                    }

                    Console.WriteLine($"[ETLWay] ✓ Extracted to: {extractedPath}");

                    // Rename to timestamped name
                    if (File.Exists(fullOutputPath)) File.Delete(fullOutputPath);
                    File.Move(extractedPath, fullOutputPath);

                    Console.WriteLine($"[ETLWay] ✓ Renamed to: {fullOutputPath}");

                    // 3. Analyze the File (Metadata Extraction)
                   // FIX: Allow .txt files to be analyzed as CSVs
                    var ext = extension.ToLower();
                    if (ext == ".csv" || ext == ".txt")
                    {
                        await AnalyzeCsvDeeply(fullOutputPath, result);
                    }
                    else if (ext == ".json")
                    {
                        await AnalyzeJsonStructure(fullOutputPath, result);
                    }
                }

                // 4. Save the Metadata as a JSON file in the same folder
                var metaJsonName = $"{timestamp}_{zipName}_metadata.json";
                var metaPath = Path.Combine(targetFolder, metaJsonName);
                var jsonOptions = new JsonSerializerOptions { WriteIndented = true };
                var jsonBytes = JsonSerializer.SerializeToUtf8Bytes(result, jsonOptions);
                await File.WriteAllBytesAsync(metaPath, jsonBytes);

                result.GeneratedFormatFileName = metaPath;

                Console.WriteLine($"[ETLWay] ✓ Metadata saved: {metaPath}");
                Console.WriteLine($"[ETLWay] ✓ Extraction complete!");

                return result;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"[ETLWay] ✗✗✗ EXTRACTION FAILED!");
                Console.WriteLine($"[ETLWay] ?? Error: {ex.Message}");
                Console.WriteLine($"[ETLWay] ?? Type: {ex.GetType().Name}");

                // Re-throw so the Actor catches it and publishes error to UI
                throw;
            }
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

        // Add this method to the ZipExtractionService class

        private async Task AnalyzeJsonStructure(string filePath, FileMetadataDto result)
        {
            // Read the JSON file content
            var json = await File.ReadAllTextAsync(filePath);

            // Try to parse as a JSON array or object
            using var doc = JsonDocument.Parse(json);

            if (doc.RootElement.ValueKind == JsonValueKind.Array)
            {
                var array = doc.RootElement.EnumerateArray();
                int rowCount = 0;
                foreach (var item in array)
                {
                    if (rowCount == 0 && item.ValueKind == JsonValueKind.Object)
                    {
                        foreach (var prop in item.EnumerateObject())
                        {
                            result.Columns.Add(new FileColumnDefinition
                            {
                                ColumnName = prop.Name,
                                OrdinalPosition = result.Columns.Count,
                                EstimatedDataType = prop.Value.ValueKind.ToString(),
                                HasNulls = false,
                                MaxLength = prop.Value.ToString().Length
                            });
                        }
                    }
                    rowCount++;
                }
                result.RowCount = rowCount;
            }
            else if (doc.RootElement.ValueKind == JsonValueKind.Object)
            {
                // Single object, treat as one row
                var obj = doc.RootElement;
                foreach (var prop in obj.EnumerateObject())
                {
                    result.Columns.Add(new FileColumnDefinition
                    {
                        ColumnName = prop.Name,
                        OrdinalPosition = result.Columns.Count,
                        EstimatedDataType = prop.Value.ValueKind.ToString(),
                        HasNulls = false,
                        MaxLength = prop.Value.ToString().Length
                    });
                }
                result.RowCount = 1;
            }
            else
            {
                // Not a supported JSON structure
                result.RowCount = 0;
            }
        }
    }
}
