using Microsoft.AspNetCore.Mvc;
using VideoProcessor.Hubs;
using VideoProcessor.Models;
using VideoProcessor.Services;
using VideoProcessor.Utils;
using Microsoft.AspNetCore.SignalR;

namespace VideoProcessor.Controllers;

[ApiController]
[Route("api/v1/video")]
public class VideoController : ControllerBase
{
    private readonly IVideoProcessingService _videoProcessingService;
    private readonly IHubContext<ProgressHub> _hubContext;
    private readonly ILogger<VideoController> _logger;

    public VideoController(
        IVideoProcessingService videoProcessingService,
        IHubContext<ProgressHub> hubContext,
        ILogger<VideoController> logger)
    {
        _videoProcessingService = videoProcessingService;
        _hubContext = hubContext;
        _logger = logger;
    }

    // ── Original format-conversion endpoint (backward compat) ─────────────────
    [HttpPost("process")]
    [Consumes("multipart/form-data")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> ProcessVideo(
        [FromForm] IFormFile file,
        [FromForm] string outputFormat,
        [FromForm] string outputName,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation($"ProcessVideo: {file?.FileName}, format: {outputFormat}");

            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file provided" });
            if (string.IsNullOrWhiteSpace(outputFormat))
                return BadRequest(new { error = "Output format is required" });
            if (string.IsNullOrWhiteSpace(outputName))
                return BadRequest(new { error = "Output name is required" });

            var sanitized = System.Text.RegularExpressions.Regex.Replace(outputName, @"[^\w\-\.]", "_");
            var result = await _videoProcessingService.ProcessVideoAsync(file, outputFormat, sanitized, cancellationToken);
            await _hubContext.Clients.All.SendAsync("completed", result.FileId, result.DownloadUrl, cancellationToken);
            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            await _hubContext.Clients.All.SendAsync("error", "Processing was cancelled", cancellationToken: CancellationToken.None);
            return BadRequest(new { error = "Processing was cancelled" });
        }
        catch (ArgumentException ex)
        {
            await _hubContext.Clients.All.SendAsync("error", ex.Message, cancellationToken: CancellationToken.None);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing video: {ex.Message}");
            await _hubContext.Clients.All.SendAsync("error", "An error occurred during processing", cancellationToken: CancellationToken.None);
            return StatusCode(500, new { error = "An error occurred during processing" });
        }
    }

    // ── New multi-operation edit endpoint ─────────────────────────────────────
    [HttpPost("edit")]
    [Consumes("multipart/form-data")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> EditVideo(
        [FromForm] IFormFile file,
        [FromForm] string operation,
        [FromForm] string outputFormat,
        [FromForm] string outputName,
        // Trim
        [FromForm] string? startTime,
        [FromForm] string? endTime,
        // Crop
        [FromForm] int? cropWidth,
        [FromForm] int? cropHeight,
        [FromForm] int? cropX,
        [FromForm] int? cropY,
        // Resize
        [FromForm] int? scaleWidth,
        [FromForm] int? scaleHeight,
        // Speed
        [FromForm] float? speed,
        // Volume
        [FromForm] float? volume,
        // GIF
        [FromForm] int? gifFps,
        [FromForm] int? gifWidth,
        [FromForm] int? gifLoop,
        // Watermark
        [FromForm] IFormFile? watermarkFile,
        [FromForm] string? watermarkPosition,
        [FromForm] int? watermarkX,
        [FromForm] int? watermarkY,
        // Equalizer
        [FromForm] float? brightness,
        [FromForm] float? contrast,
        [FromForm] float? saturation,
        // Rotate
        [FromForm] int? rotation,
        [FromForm] bool? flipHorizontal,
        [FromForm] bool? flipVertical,
        // Compress
        [FromForm] int? crf,
        // Subtitles
        [FromForm] IFormFile? subtitleFile,
        // Thumbnail
        [FromForm] string? thumbnailTime,
        // Concat additional files
        [FromForm] IFormFileCollection? additionalFiles,
        CancellationToken cancellationToken)
    {
        try
        {
            _logger.LogInformation($"EditVideo: operation={operation}, file={file?.FileName}, format={outputFormat}");

            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file provided" });
            if (string.IsNullOrWhiteSpace(outputFormat))
                return BadRequest(new { error = "Output format is required" });
            if (string.IsNullOrWhiteSpace(outputName))
                return BadRequest(new { error = "Output name is required" });
            if (!Enum.TryParse<VideoEditOperation>(operation, true, out var editOp))
                return BadRequest(new { error = $"Unknown operation: {operation}" });

            var request = new VideoEditRequest
            {
                OutputFormat     = outputFormat,
                OutputName       = outputName,
                Operation        = editOp,
                StartTime        = startTime,
                EndTime          = endTime,
                CropWidth        = cropWidth,
                CropHeight       = cropHeight,
                CropX            = cropX,
                CropY            = cropY,
                ScaleWidth       = scaleWidth,
                ScaleHeight      = scaleHeight,
                Speed            = speed,
                Volume           = volume,
                GifFps           = gifFps,
                GifWidth         = gifWidth,
                GifLoop          = gifLoop,
                WatermarkPosition = watermarkPosition,
                WatermarkX       = watermarkX,
                WatermarkY       = watermarkY,
                Brightness       = brightness,
                Contrast         = contrast,
                Saturation       = saturation,
                Rotation         = rotation,
                FlipHorizontal   = flipHorizontal,
                FlipVertical     = flipVertical,
                Crf              = crf,
                ThumbnailTime    = thumbnailTime
            };

            var result = await _videoProcessingService.EditVideoAsync(
                file, watermarkFile, subtitleFile, additionalFiles, request, cancellationToken);
            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            await _hubContext.Clients.All.SendAsync("error", "Processing was cancelled", cancellationToken: CancellationToken.None);
            return BadRequest(new { error = "Processing was cancelled" });
        }
        catch (ArgumentException ex)
        {
            _logger.LogWarning($"Invalid request: {ex.Message}");
            await _hubContext.Clients.All.SendAsync("error", ex.Message, cancellationToken: CancellationToken.None);
            return BadRequest(new { error = ex.Message });
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error editing video: {ex.Message}");
            await _hubContext.Clients.All.SendAsync("error", "An error occurred during editing", cancellationToken: CancellationToken.None);
            return StatusCode(500, new { error = "An error occurred during editing" });
        }
    }

    // ── Video metadata endpoint ────────────────────────────────────────────────
    [HttpPost("info")]
    [Consumes("multipart/form-data")]
    [DisableRequestSizeLimit]
    public async Task<IActionResult> GetVideoInfo(
        [FromForm] IFormFile file,
        CancellationToken cancellationToken)
    {
        try
        {
            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file provided" });
            if (!FFmpegUtils.IsValidVideoFile(file.FileName))
                return BadRequest(new { error = "Invalid video file type" });

            var metadata = await _videoProcessingService.GetVideoInfoAsync(file);
            return Ok(metadata);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting video info: {ex.Message}");
            return StatusCode(500, new { error = "Could not read video information" });
        }
    }

    // ── Download endpoint ─────────────────────────────────────────────────────
    [HttpGet("download/{fileName}")]
    public IActionResult DownloadVideo(string fileName)
    {
        try
        {
            var filePath  = Path.Combine("/data", Uri.UnescapeDataString(fileName));
            var fullPath  = Path.GetFullPath(filePath);
            var dataPath  = Path.GetFullPath("/data");

            if (!fullPath.StartsWith(dataPath))
                return BadRequest(new { error = "Invalid file path" });

            if (!System.IO.File.Exists(fullPath))
                return NotFound(new { error = "File not found" });

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, useAsync: true);
            var contentType = Path.GetExtension(fileName).ToLower() switch
            {
                ".mp4"  => "video/mp4",
                ".mkv"  => "video/x-matroska",
                ".webm" => "video/webm",
                ".mp3"  => "audio/mpeg",
                ".aac"  => "audio/aac",
                ".ogg"  => "audio/ogg",
                ".wav"  => "audio/wav",
                ".flac" => "audio/flac",
                ".gif"  => "image/gif",
                ".jpg"  => "image/jpeg",
                ".jpeg" => "image/jpeg",
                ".png"  => "image/png",
                _       => "application/octet-stream"
            };

            return File(stream, contentType, fileName, enableRangeProcessing: true);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error downloading file: {ex.Message}");
            return StatusCode(500, new { error = "An error occurred during download" });
        }
    }
}
