using System.Threading.Tasks;
using BahyWay.SharedKernel.Application.DTOs;

namespace BahyWay.SharedKernel.Application.Abstractions
{
    public interface IZipExtractionService
    {
        /// <summary>
        /// Extracts a zip file, analyzes its content (CSV/Excel),
        /// and returns metadata (Columns, RowCount, etc).
        /// </summary>
        /// <param name="zipFilePath">Full path to the source zip</param>
        /// <param name="outputFolder">Where to unzip the contents</param>
        /// <returns>Metadata object describing the file</returns>
        Task<FileMetadataDto> ExtractAndAnalyzeAsync(string zipFilePath, string outputFolder);
    }
}