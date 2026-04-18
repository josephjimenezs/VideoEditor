using VideoProcessor.Models;
using VideoProcessor.Utils;
using Microsoft.AspNetCore.SignalR;
using VideoProcessor.Hubs;

namespace VideoProcessor.Services;

public class VideoProcessingService : IVideoProcessingService
{
    private readonly IFFmpegService _ffmpegService;
    private readonly IHubContext<ProgressHub> _hubContext;
    private readonly ILogger<VideoProcessingService> _logger;
    private readonly string _dataPath = "/data";
    private const long MaxFileSizeBytes = 5L * 1024 * 1024 * 1024; // 5GB

    public VideoProcessingService(
        IFFmpegService ffmpegService,
        IHubContext<ProgressHub> hubContext,
        ILogger<VideoProcessingService> logger)
    {
        _ffmpegService = ffmpegService;
        _hubContext = hubContext;
        _logger = logger;
    }

    public async Task<ProcessingResponse> ProcessVideoAsync(IFormFile file, string outputFormat, string outputName, CancellationToken cancellationToken)
    {
        // Validate input
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file provided");

        if (file.Length > MaxFileSizeBytes)
            throw new ArgumentException($"File size exceeds maximum allowed size of 5GB");

        if (!FFmpegUtils.IsValidVideoFile(file.FileName))
            throw new ArgumentException($"Invalid video file type: {file.FileName}");

        if (!FFmpegUtils.IsValidFormat(outputFormat))
            throw new ArgumentException($"Invalid output format: {outputFormat}");

        var fileId = Guid.NewGuid().ToString();
        var inputFileName = $"{fileId}_input{Path.GetExtension(file.FileName)}";
        var inputPath = Path.Combine(_dataPath, inputFileName);
        var outputExtension = FFmpegUtils.GetValidOutputExtension(outputFormat);
        var outputFileName = $"{outputName}{outputExtension}";
        var outputPath = Path.Combine(_dataPath, outputFileName);

        try
        {
            // Save uploaded file
            _logger.LogInformation($"Saving uploaded file to {inputPath}");
            using (var stream = new FileStream(inputPath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            // Process video with progress reporting via SignalR
            var progress = new Progress<double>(percent =>
            {
                _logger.LogInformation($"Processing progress: {percent:F1}%");
                // Send progress to all connected clients via SignalR
                _ = _hubContext.Clients.All.SendAsync("progress", percent, 0, cancellationToken: CancellationToken.None);
            });

            _logger.LogInformation($"Starting video processing: {inputPath} -> {outputPath} ({outputFormat})");
            await _ffmpegService.ProcessVideoAsync(inputPath, outputPath, outputFormat, progress, cancellationToken);

            // Get output file info
            if (!File.Exists(outputPath))
                throw new InvalidOperationException("Output file was not created");

            var fileInfo = new FileInfo(outputPath);

            _logger.LogInformation($"Video processing completed: {fileInfo.Length} bytes");

            // Send completion signal
            await _hubContext.Clients.All.SendAsync("completed", fileId, $"/api/v1/video/download/{Uri.EscapeDataString(outputFileName)}", cancellationToken: CancellationToken.None);

            return new ProcessingResponse
            {
                FileId = fileId,
                DownloadUrl = $"/api/v1/video/download/{Uri.EscapeDataString(outputFileName)}",
                Filename = outputFileName,
                FileSizeBytes = fileInfo.Length
            };
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing video: {ex.Message}");
            // Send error signal
            await _hubContext.Clients.All.SendAsync("error", ex.Message, cancellationToken: CancellationToken.None);
            // Cleanup on error
            if (File.Exists(inputPath))
                File.Delete(inputPath);
            if (File.Exists(outputPath))
                File.Delete(outputPath);
            throw;
        }
        finally
        {
            // Cleanup input file
            if (File.Exists(inputPath))
                File.Delete(inputPath);
        }
    }

    public async Task<Stream?> GetProcessedFileAsync(string fileId)
    {
        var filePath = Path.Combine(_dataPath, fileId);
        if (!File.Exists(filePath))
            return null;

        return new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, useAsync: true);
    }

    public Task<bool> FileExistsAsync(string fileId)
    {
        var filePath = Path.Combine(_dataPath, fileId);
        return Task.FromResult(File.Exists(filePath));
    }
}
