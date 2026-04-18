using VideoProcessor.Models;

namespace VideoProcessor.Services;

public interface IVideoProcessingService
{
    Task<ProcessingResponse> ProcessVideoAsync(IFormFile file, string outputFormat, string outputName, CancellationToken cancellationToken);
    Task<ProcessingResponse> EditVideoAsync(IFormFile file, IFormFile? watermarkFile, IFormFile? subtitleFile, IFormFileCollection? additionalFiles, VideoEditRequest request, CancellationToken cancellationToken);
    Task<VideoMetadata> GetVideoInfoAsync(IFormFile file);
    Task<Stream?> GetProcessedFileAsync(string fileId);
    Task<bool> FileExistsAsync(string fileId);
}
