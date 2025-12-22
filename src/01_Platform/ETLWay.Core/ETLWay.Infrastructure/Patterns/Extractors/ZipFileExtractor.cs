using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using BahyWay.SharedKernel.Application.Abstractions; // For IZipExtractionService
using BahyWay.SharedKernel.Application.DTOs;
using ETLWay.Application.Abstractions.Patterns; // For EtlContext, IExtractor

namespace ETLWay.Infrastructure.Patterns.Extractors
{
    public class ZipFileExtractor : IExtractor<FileMetadataDto>
    {
        private readonly IZipExtractionService _zipService;
        private readonly string _zipPath;
        private readonly string _processingRoot;

        public ZipFileExtractor(IZipExtractionService zipService, string zipPath, string processingRoot)
        {
            _zipService = zipService;
            _zipPath = zipPath;
            _processingRoot = processingRoot;
        }

        // Yields metadata as it processes files
        public async IAsyncEnumerable<FileMetadataDto> ExtractAsync(EtlContext context)
        {
            var zipName = Path.GetFileNameWithoutExtension(_zipPath);
            var outputFolder = Path.Combine(_processingRoot, zipName + "_" + context.JobId);

            Directory.CreateDirectory(outputFolder);

            // Call the Worker we built earlier
            var metadata = await _zipService.ExtractAndAnalyzeAsync(_zipPath, outputFolder);

            // In a Fan-Out scenario, if the zip had 100 files, we would loop here.
            // For now, we yield the single metadata object.
            yield return metadata;
        }
    }
}