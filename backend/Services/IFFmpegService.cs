using VideoProcessor.Models;

namespace VideoProcessor.Services;

public interface IFFmpegService
{
    Task<double> GetVideoDurationAsync(string inputPath);
    Task ProcessVideoAsync(string inputPath, string outputPath, string format, IProgress<double> progress, CancellationToken cancellationToken);
    Task EditVideoAsync(string inputPath, string? watermarkPath, string? subtitlePath, string? concatListPath, VideoEditRequest request, string outputPath, IProgress<double> progress, CancellationToken cancellationToken);
    Task<VideoMetadata> GetVideoInfoAsync(string inputPath);
    Task<bool> ValidateFFmpegInstalledAsync();
}
