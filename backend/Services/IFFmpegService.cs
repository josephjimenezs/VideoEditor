namespace VideoProcessor.Services;

public interface IFFmpegService
{
    Task<double> GetVideoDurationAsync(string inputPath);
    Task ProcessVideoAsync(string inputPath, string outputPath, string format, IProgress<double> progress, CancellationToken cancellationToken);
    Task<bool> ValidateFFmpegInstalledAsync();
}
