using System;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using BahyWay.SharedKernel.Application.Abstractions; // The Interface
using BahyWay.SharedKernel.Application.DTOs;         // The DTO
using SharpCompress.Archives;
using SharpCompress.Common;

namespace BahyWay.SharedKernel.Infrastructure.FileStorage
{
    public class ZipExtractionService : IZipExtractionService
    {
        public async Task<FileMetadataDto> ExtractAndAnalyzeAsync(string zipFilePath, string outputFolder)
        {
            var result = new FileMetadataDto();
            var timestamp = DateTime.UtcNow.ToString("yyyyMMdd_HHmmss_fff");

            using (var archive = ArchiveFactory.Open(zipFilePath))
            {
                var entry = archive.Entries.Where(entry => !entry.IsDirectory)
                                           .OrderByDescending(x => x.Size)
                                           .FirstOrDefault();

                if (entry == null) throw new Exception("Empty Zip File");

                result.OriginalFileName = Path.GetFileName(zipFilePath);
                result.FileSizeBytes = entry.Size;

                var extension = Path.GetExtension(entry.Key);
                var baseName = Path.GetFileNameWithoutExtension(entry.Key);

                result.GeneratedDataFileName = $"{timestamp}_{baseName}{extension}";
                var fullOutputPath = Path.Combine(outputFolder, result.GeneratedDataFileName);

                entry.WriteToDirectory(outputFolder, new ExtractionOptions
                {
                    ExtractFullPath = false,
                    Overwrite = true
                });

                var extractedOriginalPath = Path.Combine(outputFolder, entry.Key);
                if (File.Exists(fullOutputPath)) File.Delete(fullOutputPath);
                File.Move(extractedOriginalPath, fullOutputPath);

                // Simple Header Analysis
                using var reader = new StreamReader(fullOutputPath);
                var headerLine = reader.ReadLine();
                if (!string.IsNullOrEmpty(headerLine))
                {
                    var headers = headerLine.Split(',');
                    for (int i = 0; i < headers.Length; i++)
                    {
                        result.Columns.Add(new FileColumnDefinition
                        {
                            ColumnName = headers[i].Trim(),
                            OrdinalPosition = i,
                            EstimatedDataType = "String"
                        });
                    }
                }
            }

            return await Task.FromResult(result);
        }
    }
}