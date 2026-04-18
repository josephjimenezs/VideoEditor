using System.Diagnostics;
using System.Text.RegularExpressions;
using VideoProcessor.Utils;

namespace VideoProcessor.Services;

public class FFmpegService : IFFmpegService
{
    private readonly ILogger<FFmpegService> _logger;

    public FFmpegService(ILogger<FFmpegService> logger)
    {
        _logger = logger;
    }

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

            using (var process = Process.Start(psi))
            {
                if (process == null)
                    throw new InvalidOperationException("Failed to start ffprobe process");

                var output = await process.StandardOutput.ReadToEndAsync();
                var error = await process.StandardError.ReadToEndAsync();
                await process.WaitForExitAsync();

                if (!string.IsNullOrWhiteSpace(error))
                {
                    _logger.LogError($"ffprobe error output: {error}");
                }

                var trimmedOutput = output.Trim();
                _logger.LogDebug($"ffprobe raw output: '{trimmedOutput}'");

                if (string.IsNullOrWhiteSpace(trimmedOutput))
                {
                    throw new InvalidOperationException($"ffprobe returned empty output for file: {inputPath}");
                }

                if (double.TryParse(trimmedOutput, System.Globalization.CultureInfo.InvariantCulture, out var duration))
                {
                    if (duration > 0)
                    {
                        _logger.LogInformation($"Video duration: {duration} seconds");
                        return duration;
                    }
                    throw new InvalidOperationException("Video duration is zero or negative");
                }

                throw new InvalidOperationException($"Could not parse duration from ffprobe output: '{trimmedOutput}'");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error getting video duration: {ex.Message}");
            throw;
        }
    }

    public async Task ProcessVideoAsync(string inputPath, string outputPath, string format, IProgress<double> progress, CancellationToken cancellationToken)
    {
        try
        {
            var duration = await GetVideoDurationAsync(inputPath);
            var arguments = FFmpegUtils.GetFFmpegArguments(inputPath, outputPath, format);

            var psi = new ProcessStartInfo
            {
                FileName = "ffmpeg",
                Arguments = $"-y {arguments}",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (var process = Process.Start(psi))
            {
                if (process == null)
                    throw new InvalidOperationException("Failed to start ffmpeg process");

                var outputTask = Task.Run(() =>
                {
                    string? line;
                    while ((line = process.StandardOutput.ReadLine()) != null)
                    {
                        if (line.StartsWith("out_time_ms="))
                        {
                            if (long.TryParse(line.Substring(12), out var timeMs))
                            {
                                var timeSeconds = timeMs / 1_000_000.0;
                                if (duration > 0)
                                {
                                    var progressPercent = Math.Min(100, (timeSeconds / duration) * 100);
                                    progress.Report(progressPercent);
                                    _logger.LogInformation($"Processing progress: {progressPercent:F1}%");
                                }
                            }
                        }
                    }
                });

                var errorTask = Task.Run(async () =>
                {
                    var error = await process.StandardError.ReadToEndAsync();
                    if (!string.IsNullOrWhiteSpace(error))
                    {
                        _logger.LogInformation($"FFmpeg output: {error}");
                    }
                });

                await process.WaitForExitAsync(cancellationToken);
                await outputTask;
                await errorTask;

                if (process.ExitCode != 0)
                {
                    throw new InvalidOperationException($"FFmpeg process exited with code {process.ExitCode}");
                }

                progress.Report(100);
                _logger.LogInformation("Video processing completed successfully");
            }
        }
        catch (Exception ex)
        {
            _logger.LogError($"Error processing video: {ex.Message}");
            throw;
        }
    }

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

            using (var process = Process.Start(psi))
            {
                if (process == null) return false;
                await process.WaitForExitAsync();
                return process.ExitCode == 0;
            }
        }
        catch
        {
            return false;
        }
    }
}
