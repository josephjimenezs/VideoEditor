namespace VideoProcessor.Models;

public class VideoProcessRequest
{
    public required string OutputFormat { get; set; }
    public required string OutputName { get; set; }
}

public enum VideoFormat
{
    Mp4,
    Mkv,
    Webm
}

public enum VideoEditOperation
{
    Convert,
    Trim,
    Crop,
    Resize,
    Speed,
    Volume,
    ExtractAudio,
    VideoToGif,
    Watermark,
    Equalizer,
    Rotate,
    RemoveAudio,
    Compress,
    Subtitles,
    Concatenate,
    Thumbnail
}

public class VideoEditRequest
{
    public required string OutputFormat { get; set; }
    public required string OutputName { get; set; }
    public VideoEditOperation Operation { get; set; } = VideoEditOperation.Convert;

    // Trim
    public string? StartTime { get; set; }
    public string? EndTime { get; set; }

    // Crop
    public int? CropWidth { get; set; }
    public int? CropHeight { get; set; }
    public int? CropX { get; set; }
    public int? CropY { get; set; }

    // Resize/Scale
    public int? ScaleWidth { get; set; }
    public int? ScaleHeight { get; set; }

    // Speed
    public float? Speed { get; set; }

    // Volume
    public float? Volume { get; set; }

    // GIF
    public int? GifFps { get; set; }
    public int? GifWidth { get; set; }
    public int? GifLoop { get; set; }

    // Watermark
    public string? WatermarkPosition { get; set; }
    public int? WatermarkX { get; set; }
    public int? WatermarkY { get; set; }

    // Equalizer
    public float? Brightness { get; set; }
    public float? Contrast { get; set; }
    public float? Saturation { get; set; }

    // Rotate / Flip
    public int? Rotation { get; set; }
    public bool? FlipHorizontal { get; set; }
    public bool? FlipVertical { get; set; }

    // Compress
    public int? Crf { get; set; }

    // Thumbnail
    public string? ThumbnailTime { get; set; }
}

public class ProcessingResponse
{
    public required string FileId { get; set; }
    public required string DownloadUrl { get; set; }
    public required string Filename { get; set; }
    public required long FileSizeBytes { get; set; }
}

public class VideoMetadata
{
    public double Duration { get; set; }
    public int Width { get; set; }
    public int Height { get; set; }
    public double Fps { get; set; }
    public string VideoCodec { get; set; } = "";
    public string AudioCodec { get; set; } = "";
    public long FileSizeBytes { get; set; }
    public string AudioBitrate { get; set; } = "";
}
