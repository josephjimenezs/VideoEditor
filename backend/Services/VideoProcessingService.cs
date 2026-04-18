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

    // ── Original format-conversion endpoint (kept for compat) ─────────────────
    public async Task<ProcessingResponse> ProcessVideoAsync(
        IFormFile file, string outputFormat, string outputName, CancellationToken cancellationToken)
    {
        ValidateVideoFile(file);
        if (!FFmpegUtils.IsValidFormat(outputFormat))
            throw new ArgumentException($"Invalid output format: {outputFormat}");

        var fileId        = Guid.NewGuid().ToString();
        var inputFileName = $"{fileId}_input{Path.GetExtension(file.FileName)}";
        var inputPath     = Path.Combine(_dataPath, inputFileName);
        var outputExt     = FFmpegUtils.GetValidOutputExtension(outputFormat);
        var sanitized     = SanitizeName(outputName);
        var outputPath    = Path.Combine(_dataPath, $"{sanitized}{outputExt}");

        try
        {
            await SaveFileAsync(file, inputPath, cancellationToken);
            var progress = MakeProgress(cancellationToken);
            await _ffmpegService.ProcessVideoAsync(inputPath, outputPath, outputFormat, progress, cancellationToken);
            return await BuildResponse(fileId, outputPath, $"{sanitized}{outputExt}", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during processing: {ex.Message}");
            await _hubContext.Clients.All.SendAsync("error", ex.Message, cancellationToken: CancellationToken.None);
            CleanUp(inputPath, outputPath);
            throw;
        }
        finally { CleanUp(inputPath); }
    }

    // ── New multi-operation edit endpoint ─────────────────────────────────────
    public async Task<ProcessingResponse> EditVideoAsync(
        IFormFile file,
        IFormFile? watermarkFile,
        IFormFile? subtitleFile,
        IFormFileCollection? additionalFiles,
        VideoEditRequest request,
        CancellationToken cancellationToken)
    {
        ValidateVideoFile(file);
        if (!FFmpegUtils.IsValidFormat(request.OutputFormat))
            throw new ArgumentException($"Invalid output format: {request.OutputFormat}");

        var fileId        = Guid.NewGuid().ToString();
        var inputExt      = Path.GetExtension(file.FileName);
        var inputPath     = Path.Combine(_dataPath, $"{fileId}_input{inputExt}");
        var outputExt     = FFmpegUtils.GetValidOutputExtension(request.OutputFormat);
        var sanitized     = SanitizeName(request.OutputName);
        var outputPath    = Path.Combine(_dataPath, $"{sanitized}{outputExt}");

        // Secondary file paths
        string? watermarkPath  = null;
        string? subtitlePath   = null;
        string? concatListPath = null;
        var tempFiles = new List<string> { inputPath };

        try
        {
            await SaveFileAsync(file, inputPath, cancellationToken);

            // Save watermark
            if (watermarkFile != null && watermarkFile.Length > 0)
            {
                if (!FFmpegUtils.IsValidImageFile(watermarkFile.FileName))
                    throw new ArgumentException("Invalid watermark file type");
                watermarkPath = Path.Combine(_dataPath, $"{fileId}_wm{Path.GetExtension(watermarkFile.FileName)}");
                await SaveFileAsync(watermarkFile, watermarkPath, cancellationToken);
                tempFiles.Add(watermarkPath);
            }

            // Save subtitle
            if (subtitleFile != null && subtitleFile.Length > 0)
            {
                if (!FFmpegUtils.IsValidSubtitleFile(subtitleFile.FileName))
                    throw new ArgumentException("Invalid subtitle file type");
                subtitlePath = Path.Combine(_dataPath, $"{fileId}_sub{Path.GetExtension(subtitleFile.FileName)}");
                await SaveFileAsync(subtitleFile, subtitlePath, cancellationToken);
                tempFiles.Add(subtitlePath);
            }

            // Save additional files for concatenation and build list file
            if (request.Operation == VideoEditOperation.Concatenate && additionalFiles?.Count > 0)
            {
                var concatLines = new List<string>();
                concatLines.Add($"file '{inputPath.Replace("'", "\\'")}'");

                int idx = 0;
                foreach (var addFile in additionalFiles.Take(4))
                {
                    if (!FFmpegUtils.IsValidVideoFile(addFile.FileName)) continue;
                    var addPath = Path.Combine(_dataPath, $"{fileId}_concat{idx++}{Path.GetExtension(addFile.FileName)}");
                    await SaveFileAsync(addFile, addPath, cancellationToken);
                    tempFiles.Add(addPath);
                    concatLines.Add($"file '{addPath.Replace("'", "\\'")}'");
                }

                concatListPath = Path.Combine(_dataPath, $"{fileId}_list.txt");
                await File.WriteAllLinesAsync(concatListPath, concatLines, cancellationToken);
                tempFiles.Add(concatListPath);
            }

            var progress = MakeProgress(cancellationToken);
            await _ffmpegService.EditVideoAsync(inputPath, watermarkPath, subtitlePath, concatListPath, request, outputPath, progress, cancellationToken);

            return await BuildResponse(fileId, outputPath, $"{sanitized}{outputExt}", cancellationToken);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error during edit: {ex.Message}");
            await _hubContext.Clients.All.SendAsync("error", ex.Message, cancellationToken: CancellationToken.None);
            CleanUp(outputPath);
            throw;
        }
        finally
        {
            foreach (var f in tempFiles) CleanUp(f);
        }
    }

    // ── Video info (no processing) ────────────────────────────────────────────
    public async Task<VideoMetadata> GetVideoInfoAsync(IFormFile file)
    {
        ValidateVideoFile(file);
        var tempPath = Path.Combine(_dataPath, $"{Guid.NewGuid()}_probe{Path.GetExtension(file.FileName)}");
        try
        {
            using (var stream = new FileStream(tempPath, FileMode.Create))
                await file.CopyToAsync(stream);
            return await _ffmpegService.GetVideoInfoAsync(tempPath);
        }
        finally { CleanUp(tempPath); }
    }

    // ── Stream access ─────────────────────────────────────────────────────────
    public Task<Stream?> GetProcessedFileAsync(string fileId)
    {
        var filePath = Path.Combine(_dataPath, fileId);
        if (!File.Exists(filePath)) return Task.FromResult<Stream?>(null);
        return Task.FromResult<Stream?>(new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, useAsync: true));
    }

    public Task<bool> FileExistsAsync(string fileId)
        => Task.FromResult(File.Exists(Path.Combine(_dataPath, fileId)));

    // ── Private helpers ───────────────────────────────────────────────────────
    private void ValidateVideoFile(IFormFile file)
    {
        if (file == null || file.Length == 0)
            throw new ArgumentException("No file provided");
        if (file.Length > MaxFileSizeBytes)
            throw new ArgumentException("File size exceeds maximum of 5GB");
        if (!FFmpegUtils.IsValidVideoFile(file.FileName))
            throw new ArgumentException($"Invalid video file type: {file.FileName}");
    }

    private static string SanitizeName(string name)
        => System.Text.RegularExpressions.Regex.Replace(name, @"[^\w\-\.]", "_");

    private static async Task SaveFileAsync(IFormFile file, string path, CancellationToken ct)
    {
        using var stream = new FileStream(path, FileMode.Create);
        await file.CopyToAsync(stream, ct);
    }

    private IProgress<double> MakeProgress(CancellationToken ct) =>
        new Progress<double>(pct =>
        {
            _ = _hubContext.Clients.All.SendAsync("progress", pct, 0, cancellationToken: CancellationToken.None);
        });

    private async Task<ProcessingResponse> BuildResponse(string fileId, string outputPath, string outputFileName, CancellationToken ct)
    {
        if (!File.Exists(outputPath))
            throw new InvalidOperationException("Output file was not created");

        var info = new FileInfo(outputPath);
        var downloadUrl = $"/api/v1/video/download/{Uri.EscapeDataString(outputFileName)}";

        await _hubContext.Clients.All.SendAsync("completed", fileId, downloadUrl, cancellationToken: CancellationToken.None);

        return new ProcessingResponse
        {
            FileId = fileId,
            DownloadUrl = downloadUrl,
            Filename = outputFileName,
            FileSizeBytes = info.Length
        };
    }

    private static void CleanUp(params string[] paths)
    {
        foreach (var p in paths)
        {
            try { if (File.Exists(p)) File.Delete(p); }
            catch { /* best effort */ }
        }
    }
}
