namespace VideoProcessor.Utils;

public static class FFmpegUtils
{
    public static string GetFFmpegArguments(string inputPath, string outputPath, string format)
    {
        return format.ToLower() switch
        {
            "mp4" => $"-i \"{inputPath}\" -c:v libx264 -preset fast -crf 22 -c:a aac -b:a 128k -progress pipe:1 \"{outputPath}\"",
            "mkv" => $"-i \"{inputPath}\" -c:v libx265 -preset fast -crf 22 -c:a aac -b:a 128k -progress pipe:1 \"{outputPath}\"",
            "webm" => $"-i \"{inputPath}\" -c:v libvpx-vp9 -b:v 500k -c:a libopus -progress pipe:1 \"{outputPath}\"",
            _ => throw new ArgumentException($"Unsupported format: {format}")
        };
    }

    public static string GetValidOutputExtension(string format)
    {
        return format.ToLower() switch
        {
            "mp4" => ".mp4",
            "mkv" => ".mkv",
            "webm" => ".webm",
            _ => throw new ArgumentException($"Unsupported format: {format}")
        };
    }

    public static bool IsValidFormat(string format)
    {
        var validFormats = new[] { "mp4", "mkv", "webm" };
        return validFormats.Contains(format.ToLower());
    }

    public static bool IsValidVideoFile(string filename)
    {
        var validExtensions = new[] { ".mp4", ".mkv", ".webm", ".avi", ".mov", ".flv", ".wmv", ".m4v" };
        var extension = Path.GetExtension(filename).ToLower();
        return validExtensions.Contains(extension);
    }
}
