using System.Diagnostics;
using System.Text.Json;
using System.Text.RegularExpressions;
using VideoProcessor.Models;
using VideoProcessor.Utils;

namespace VideoProcessor.Services;

public class FFmpegService : IFFmpegService
{
    private readonly ILogger<FFmpegService> _logger;

    public FFmpegService(ILogger<FFmpegService> logger)
    {
        _logger = logger;
    }

    // ── Duration (used for progress) ──────────────────────────────────────────
    public async Task<double> GetVideoDurationAsync(string inputPath)
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "ffprobe",
                Arguments = $"-v error -show_entries format=duration -of default=noprint_wrappers=1:nokey=1 \"{inputPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using var process = Process.Start(psi)
                ?? throw new InvalidOperationException("Failed to start ffprobe process");

            var output = await process.StandardOutput.ReadToEndAsync();
            await process.StandardError.ReadToEndAsync(); // drain stderr
            await process.WaitForExitAsync();

            var trimmed = output.Trim();
            if (double.TryParse(trimmed, System.Globalization.CultureInfo.InvariantCulture, out var duration) && duration > 0)
                return duration;

            throw new InvalidOperationException($"Could not parse duration from ffprobe output: '{trimmed}'");
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting video duration: {ex.Message}");
            throw;
        }
    }

    // ── Video metadata via ffprobe JSON ───────────────────────────────────────
    public async Task<VideoMetadata> GetVideoInfoAsync(string inputPath)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "ffprobe",
            Arguments = $"-v quiet -print_format json -show_streams -show_format \"{inputPath}\"",
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start ffprobe");

        var json = await process.StandardOutput.ReadToEndAsync();
        await process.StandardError.ReadToEndAsync();
        await process.WaitForExitAsync();

        var doc = JsonDocument.Parse(json);
        var root = doc.RootElement;

        var metadata = new VideoMetadata();

        if (root.TryGetProperty("format", out var fmt))
        {
            if (fmt.TryGetProperty("duration", out var dur))
            {
                if (double.TryParse(dur.GetString(), System.Globalization.CultureInfo.InvariantCulture, out var duration))
                    metadata.Duration = duration;
            }
            if (fmt.TryGetProperty("size", out var sz))
            {
                if (long.TryParse(sz.GetString(), System.Globalization.CultureInfo.InvariantCulture, out var fileSize))
                    metadata.FileSizeBytes = fileSize;
            }
        }

        if (root.TryGetProperty("streams", out var streams))
        {
            foreach (var stream in streams.EnumerateArray())
            {
                if (!stream.TryGetProperty("codec_type", out var codecType)) continue;
                var type = codecType.GetString();

                if (type == "video" && metadata.VideoCodec == "")
                {
                    metadata.Width = stream.TryGetProperty("width", out var w) ? w.GetInt32() : 0;
                    metadata.Height = stream.TryGetProperty("height", out var h) ? h.GetInt32() : 0;
                    metadata.VideoCodec = stream.TryGetProperty("codec_name", out var vc) ? vc.GetString() ?? "" : "";

                    if (stream.TryGetProperty("r_frame_rate", out var fr))
                    {
                        var parts = fr.GetString()?.Split('/');
                        if (parts?.Length == 2 && double.TryParse(parts[0], out var num) && double.TryParse(parts[1], out var den) && den > 0)
                            metadata.Fps = Math.Round(num / den, 2);
                    }
                }
                else if (type == "audio" && metadata.AudioCodec == "")
                {
                    metadata.AudioCodec = stream.TryGetProperty("codec_name", out var ac) ? ac.GetString() ?? "" : "";
                    metadata.AudioBitrate = stream.TryGetProperty("bit_rate", out var ab) ? ab.GetString() ?? "" : "";
                }
            }
        }

        return metadata;
    }

    // ── Original format-conversion processor (kept for compatibility) ─────────
    public async Task ProcessVideoAsync(string inputPath, string outputPath, string format, IProgress<double> progress, CancellationToken cancellationToken)
    {
        var duration = await GetVideoDurationAsync(inputPath);
        var arguments = FFmpegUtils.GetFFmpegArguments(inputPath, outputPath, format);
        await RunFFmpegAsync($"-y {arguments}", duration, progress, cancellationToken);
    }

    // ── New operation-aware processor ─────────────────────────────────────────
    public async Task EditVideoAsync(
        string inputPath,
        string? watermarkPath,
        string? subtitlePath,
        string? concatListPath,
        VideoEditRequest request,
        string outputPath,
        IProgress<double> progress,
        CancellationToken cancellationToken)
    {
        double duration = 0;
        try { duration = await GetVideoDurationAsync(inputPath); } catch { /* thumbnail or concat may not need duration */ }

        var arguments = FFmpegUtils.BuildArguments(inputPath, watermarkPath, subtitlePath, concatListPath, request, outputPath);
        _logger.LogInformation($"FFmpeg arguments: {arguments}");
        await RunFFmpegAsync(arguments, duration, progress, cancellationToken);
    }

    // ── Shared FFmpeg runner with progress parsing ────────────────────────────
    private async Task RunFFmpegAsync(string arguments, double duration, IProgress<double> progress, CancellationToken cancellationToken)
    {
        var psi = new ProcessStartInfo
        {
            FileName = "ffmpeg",
            Arguments = arguments,
            UseShellExecute = false,
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            CreateNoWindow = true
        };

        using var process = Process.Start(psi)
            ?? throw new InvalidOperationException("Failed to start ffmpeg process");

        var outputTask = Task.Run(() =>
        {
            string? line;
            while ((line = process.StandardOutput.ReadLine()) != null)
            {
                if (line.StartsWith("out_time_ms=") && long.TryParse(line[12..], out var timeMs))
                {
                    var timeSecond = timeMs / 1_000_000.0;
                    if (duration > 0)
                    {
                        var pct = Math.Min(100, (timeSecond / duration) * 100);
                        progress.Report(pct);
                    }
                }
            }
        }, cancellationToken);

        var errorTask = Task.Run(async () =>
        {
            var err = await process.StandardError.ReadToEndAsync(cancellationToken);
            if (!string.IsNullOrWhiteSpace(err))
                _logger.LogInformation($"FFmpeg stderr: {err}");
        }, cancellationToken);

        await process.WaitForExitAsync(cancellationToken);
        await outputTask;
        await errorTask;

        if (process.ExitCode != 0)
            throw new InvalidOperationException($"FFmpeg exited with code {process.ExitCode}");

        progress.Report(100);
        _logger.LogInformation("FFmpeg processing completed successfully");
    }

    // ── Validation ────────────────────────────────────────────────────────────
    public async Task<bool> ValidateFFmpegInstalledAsync()
    {
        try
        {
            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = "-version",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                CreateNoWindow = true
            };
            using var process = Process.Start(psi);
            if (process == null) return false;
            await process.WaitForExitAsync();
            return process.ExitCode == 0;
        }
        catch { return false; }
    }
}
