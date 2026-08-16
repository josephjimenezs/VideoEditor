using VideoProcessor.Models;

namespace VideoProcessor.Utils;

public static class FFmpegUtils
{
    // ── Original conversion helper (kept for compatibility) ──────────────────
    public static string GetFFmpegArguments(string inputPath, string outputPath, string format)
    {
        return format.ToLower() switch
        {
            "mp4"  => $"-i \"{inputPath}\" -c:v libx264 -preset fast -crf 22 -c:a aac -b:a 128k -progress pipe:1 \"{outputPath}\"",
            "mkv"  => $"-i \"{inputPath}\" -c:v libx265 -preset fast -crf 22 -c:a aac -b:a 128k -progress pipe:1 \"{outputPath}\"",
            "webm" => $"-i \"{inputPath}\" -c:v libvpx-vp9 -b:v 500k -c:a libopus -progress pipe:1 \"{outputPath}\"",
            _ => throw new ArgumentException($"Unsupported format: {format}")
        };
    }

    // ── Operation-aware builder ───────────────────────────────────────────────
    public static string BuildArguments(string inputPath, string? watermarkPath, string? subtitlePath, string? concatListPath, VideoEditRequest req, string outputPath)
    {
        var filters = new List<string>();
        var extraInputs = "";
        var audioArgs = "-c:a aac -b:a 128k";
        var videoArgs = "";
        var seekArgs = "";
        var durationArgs = "";

        switch (req.Operation)
        {
            // ── Trim ──────────────────────────────────────────────────────────
            case VideoEditOperation.Trim:
            {
                if (!string.IsNullOrWhiteSpace(req.StartTime))
                    seekArgs = $"-ss {req.StartTime}";
                
                // Calculate duration from StartTime and EndTime
                if (!string.IsNullOrWhiteSpace(req.StartTime) && !string.IsNullOrWhiteSpace(req.EndTime))
                {
                    // Parse times and calculate duration
                    if (TimeSpan.TryParse(req.StartTime, out var start) && TimeSpan.TryParse(req.EndTime, out var end))
                    {
                        var duration = end - start;
                        if (duration.TotalSeconds > 0)
                            durationArgs = $"-t {duration.TotalSeconds:F2}";
                    }
                }
                else if (!string.IsNullOrWhiteSpace(req.EndTime))
                {
                    // Only end time specified - use it as duration limit
                    durationArgs = $"-to {req.EndTime}";
                }
                
                videoArgs = GetVideoEncodeArgs(req.OutputFormat);
                break;
            }

            // ── Crop ──────────────────────────────────────────────────────────
            case VideoEditOperation.Crop:
            {
                var w = req.CropWidth ?? 1280;
                var h = req.CropHeight ?? 720;
                var x = req.CropX ?? 0;
                var y = req.CropY ?? 0;
                filters.Add($"crop={w}:{h}:{x}:{y}");
                videoArgs = GetVideoEncodeArgs(req.OutputFormat);
                break;
            }

            // ── Resize ────────────────────────────────────────────────────────
            case VideoEditOperation.Resize:
            {
                var w = req.ScaleWidth > 0 ? req.ScaleWidth!.Value : -2;
                var h = req.ScaleHeight > 0 ? req.ScaleHeight!.Value : -2;
                filters.Add($"scale={w}:{h}");
                videoArgs = GetVideoEncodeArgs(req.OutputFormat);
                break;
            }

            // ── Speed ─────────────────────────────────────────────────────────
            case VideoEditOperation.Speed:
            {
                var speed = Math.Clamp(req.Speed ?? 1.0f, 0.25f, 4.0f);
                var pts = 1.0 / speed;
                filters.Add($"setpts={pts:F4}*PTS");
                // atempo only accepts 0.5–2.0, chain for values outside
                audioArgs = BuildAtempoChain(speed);
                videoArgs = GetVideoEncodeArgs(req.OutputFormat);
                break;
            }

            // ── Volume ────────────────────────────────────────────────────────
            case VideoEditOperation.Volume:
            {
                var vol = Math.Clamp(req.Volume ?? 1.0f, 0.0f, 5.0f);
                audioArgs = $"-af volume={vol:F2} -c:a aac -b:a 128k";
                videoArgs = "-c:v copy";
                break;
            }

            // ── Extract Audio ─────────────────────────────────────────────────
            case VideoEditOperation.ExtractAudio:
            {
                var codec = req.OutputFormat.ToLower() switch
                {
                    "mp3"  => "-c:a libmp3lame -q:a 2",
                    "ogg"  => "-c:a libvorbis -q:a 5",
                    "wav"  => "-c:a pcm_s16le",
                    "flac" => "-c:a flac",
                    _      => "-c:a aac -b:a 192k"
                };
                return $"-y -i \"{inputPath}\" -vn {codec} -progress pipe:1 \"{outputPath}\"";
            }

            // ── Video to GIF ──────────────────────────────────────────────────
            case VideoEditOperation.VideoToGif:
            {
                var fps   = req.GifFps   ?? 10;
                var width = req.GifWidth ?? 480;
                var loop  = req.GifLoop  ?? 0;
                filters.Add($"fps={fps},scale={width}:-1:flags=lanczos,split[s0][s1];[s0]palettegen[p];[s1][p]paletteuse");
                return $"-y -i \"{inputPath}\" -filter_complex \"{string.Join(",", filters)}\" -loop {loop} -progress pipe:1 \"{outputPath}\"";
            }

            // ── Watermark ─────────────────────────────────────────────────────
            case VideoEditOperation.Watermark when watermarkPath != null:
            {
                var pos = req.WatermarkPosition?.ToLower() ?? "bottomright";
                var ox  = req.WatermarkX ?? 10;
                var oy  = req.WatermarkY ?? 10;
                var (overlayX, overlayY) = pos switch
                {
                    "topleft"     => ($"{ox}", $"{oy}"),
                    "topright"    => ($"main_w-overlay_w-{ox}", $"{oy}"),
                    "bottomleft"  => ($"{ox}", $"main_h-overlay_h-{oy}"),
                    _             => ($"main_w-overlay_w-{ox}", $"main_h-overlay_h-{oy}") // bottomright
                };
                extraInputs = $"-i \"{watermarkPath}\"";
                return $"-y -i \"{inputPath}\" {extraInputs} -filter_complex \"[0:v][1:v]overlay={overlayX}:{overlayY}[outv]\" -map \"[outv]\" -map 0:a? {GetVideoEncodeArgs(req.OutputFormat)} -c:a aac -b:a 128k -progress pipe:1 \"{outputPath}\"";
            }

            // ── Equalizer (Brightness/Contrast/Saturation) ────────────────────
            case VideoEditOperation.Equalizer:
            {
                var brightness = Math.Clamp(req.Brightness ?? 0f, -1f, 1f);
                var contrast   = Math.Clamp(req.Contrast   ?? 1f, -1000f, 1000f);
                var saturation = Math.Clamp(req.Saturation ?? 1f, 0f, 3f);
                filters.Add($"eq=brightness={brightness:F2}:contrast={contrast:F2}:saturation={saturation:F2}");
                videoArgs = GetVideoEncodeArgs(req.OutputFormat);
                break;
            }

            // ── Rotate / Flip ─────────────────────────────────────────────────
            case VideoEditOperation.Rotate:
            {
                var transposeFilters = (req.Rotation ?? 0) switch
                {
                    90  => "transpose=1",
                    180 => "transpose=1,transpose=1",
                    270 => "transpose=2",
                    _   => ""
                };
                if (!string.IsNullOrEmpty(transposeFilters))
                    filters.Add(transposeFilters);
                if (req.FlipHorizontal == true)
                    filters.Add("hflip");
                if (req.FlipVertical == true)
                    filters.Add("vflip");
                videoArgs = GetVideoEncodeArgs(req.OutputFormat);
                break;
            }

            // ── Remove Audio ──────────────────────────────────────────────────
            case VideoEditOperation.RemoveAudio:
                return $"-y -i \"{inputPath}\" -an -c:v copy -progress pipe:1 \"{outputPath}\"";

            // ── Compress ──────────────────────────────────────────────────────
            case VideoEditOperation.Compress:
            {
                var crf = Math.Clamp(req.Crf ?? 28, 0, 51);
                videoArgs = req.OutputFormat.ToLower() switch
                {
                    "mkv"  => $"-c:v libx265 -preset slow -crf {crf}",
                    "webm" => $"-c:v libvpx-vp9 -crf {crf} -b:v 0",
                    _      => $"-c:v libx264 -preset slow -crf {crf}"
                };
                break;
            }

            // ── Subtitles ─────────────────────────────────────────────────────
            case VideoEditOperation.Subtitles when subtitlePath != null:
                filters.Add($"subtitles='{subtitlePath.Replace("\\", "/").Replace("'", "\\'")}'");
                videoArgs = GetVideoEncodeArgs(req.OutputFormat);
                break;

            // ── Concatenate ───────────────────────────────────────────────────
            case VideoEditOperation.Concatenate when concatListPath != null:
                return $"-y -f concat -safe 0 -i \"{concatListPath}\" -c:v libx264 -preset fast -crf 22 -c:a aac -b:a 128k -progress pipe:1 \"{outputPath}\"";

            // ── Thumbnail ─────────────────────────────────────────────────────
            case VideoEditOperation.Thumbnail:
            {
                var ts = req.ThumbnailTime ?? "00:00:01";
                return $"-y -ss {ts} -i \"{inputPath}\" -vframes 1 -q:v 2 \"{outputPath}\"";
            }

            // ── Convert (default) ─────────────────────────────────────────────
            default:
                return $"-y {GetFFmpegArguments(inputPath, outputPath, req.OutputFormat)}";
        }

        // Build compound command for filter-based operations
        var filterArg = filters.Count > 0 ? $"-vf \"{string.Join(",", filters)}\"" : "";
        return $"-y {seekArgs} -i \"{inputPath}\" {durationArgs} {filterArg} {videoArgs} {audioArgs} -progress pipe:1 \"{outputPath}\"".Replace("  ", " ").Trim();
    }

    // ── Helper: video encode arguments per container ──────────────────────────
    private static string GetVideoEncodeArgs(string format) => format.ToLower() switch
    {
        "mkv"  => "-c:v libx265 -preset fast -crf 22",
        "webm" => "-c:v libvpx-vp9 -b:v 500k",
        _      => "-c:v libx264 -preset fast -crf 22"
    };

    // ── Helper: chain atempo filters to support speeds outside 0.5–2.0 ───────
    private static string BuildAtempoChain(float speed)
    {
        var filters = new List<string>();
        var remaining = (double)speed;
        while (remaining > 2.0)
        {
            filters.Add("atempo=2.0");
            remaining /= 2.0;
        }
        while (remaining < 0.5)
        {
            filters.Add("atempo=0.5");
            remaining /= 0.5;
        }
        filters.Add($"atempo={remaining:F4}");
        return $"-af \"{string.Join(",", filters)}\" -c:a aac -b:a 128k";
    }

    // ── Validation helpers ────────────────────────────────────────────────────
    public static string GetValidOutputExtension(string format) => format.ToLower() switch
    {
        "mp4"  => ".mp4",
        "mkv"  => ".mkv",
        "webm" => ".webm",
        "mp3"  => ".mp3",
        "aac"  => ".aac",
        "ogg"  => ".ogg",
        "wav"  => ".wav",
        "flac" => ".flac",
        "gif"  => ".gif",
        "jpg"  => ".jpg",
        "jpeg" => ".jpeg",
        "png"  => ".png",
        _      => throw new ArgumentException($"Unsupported format: {format}")
    };

    public static bool IsValidFormat(string format)
    {
        var validFormats = new[]
        {
            "mp4", "mkv", "webm",
            "mp3", "aac", "ogg", "wav", "flac",
            "gif", "jpg", "jpeg", "png"
        };
        return validFormats.Contains(format.ToLower());
    }

    public static bool IsValidVideoFile(string filename)
    {
        var validExtensions = new[]
        {
            ".mp4", ".mkv", ".webm", ".avi", ".mov",
            ".flv", ".wmv", ".m4v", ".ts", ".mts",
            ".m2ts", ".3gp", ".ogv", ".vob"
        };
        var extension = Path.GetExtension(filename).ToLower();
        return validExtensions.Contains(extension);
    }

    public static bool IsValidImageFile(string filename)
    {
        var validExtensions = new[] { ".png", ".jpg", ".jpeg", ".webp", ".gif", ".bmp" };
        return validExtensions.Contains(Path.GetExtension(filename).ToLower());
    }

    public static bool IsValidSubtitleFile(string filename)
    {
        var validExtensions = new[] { ".srt", ".ass", ".vtt", ".sub" };
        return validExtensions.Contains(Path.GetExtension(filename).ToLower());
    }
}
