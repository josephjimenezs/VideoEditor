using VideoProcessor.Models;

namespace VideoProcessor.Services;

public interface IVideoProcessingService
{
    Task<ProcessingResponse> ProcessVideoAsync(IFormFile file, string outputFormat, string outputName, CancellationToken cancellationToken);
    Task<Stream?> GetProcessedFileAsync(string fileId);
    Task<bool> FileExistsAsync(string fileId);
}
