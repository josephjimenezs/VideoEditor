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

public class ProcessingResponse
{
    public required string FileId { get; set; }
    public required string DownloadUrl { get; set; }
    public required string Filename { get; set; }
    public required long FileSizeBytes { get; set; }
}
