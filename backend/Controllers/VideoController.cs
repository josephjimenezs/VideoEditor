using Microsoft.AspNetCore.Mvc;
using VideoProcessor.Hubs;
using VideoProcessor.Services;
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
            _logger.LogInformation($"Received video processing request: {file?.FileName}, format: {outputFormat}");

            if (file == null || file.Length == 0)
                return BadRequest(new { error = "No file provided" });

            if (string.IsNullOrWhiteSpace(outputFormat))
                return BadRequest(new { error = "Output format is required" });

            if (string.IsNullOrWhiteSpace(outputName))
                return BadRequest(new { error = "Output name is required" });

            // Sanitize output name
            var sanitizedName = System.Text.RegularExpressions.Regex.Replace(outputName, @"[^\w\-\.]", "_");

            var result = await _videoProcessingService.ProcessVideoAsync(
                file,
                outputFormat,
                sanitizedName,
                cancellationToken);

            await _hubContext.Clients.All.SendAsync("completed", result.FileId, result.DownloadUrl, cancellationToken);

            return Ok(result);
        }
        catch (OperationCanceledException)
        {
            _logger.LogWarning("Video processing was cancelled");
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
            _logger.LogError($"Error processing video: {ex.Message}");
            await _hubContext.Clients.All.SendAsync("error", "An error occurred during processing", cancellationToken: CancellationToken.None);
            return StatusCode(500, new { error = "An error occurred during processing" });
        }
    }

    [HttpGet("download/{fileName}")]
    public async Task<IActionResult> DownloadVideo(string fileName)
    {
        try
        {
            var filePath = Path.Combine("/data", Uri.UnescapeDataString(fileName));

            // Security check - ensure path is within /data
            var fullPath = Path.GetFullPath(filePath);
            var dataPath = Path.GetFullPath("/data");
            if (!fullPath.StartsWith(dataPath))
                return BadRequest(new { error = "Invalid file path" });

            if (!System.IO.File.Exists(fullPath))
                return NotFound(new { error = "File not found" });

            var stream = new FileStream(fullPath, FileMode.Open, FileAccess.Read, FileShare.Read, 65536, useAsync: true);
            var contentType = Path.GetExtension(fileName).ToLower() switch
            {
                ".mp4" => "video/mp4",
                ".mkv" => "video/x-matroska",
                ".webm" => "video/webm",
                _ => "application/octet-stream"
            };

            return File(stream, contentType, fileName, enableRangeProcessing: true);
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error downloading video: {ex.Message}");
            return StatusCode(500, new { error = "An error occurred during download" });
        }
    }
}
