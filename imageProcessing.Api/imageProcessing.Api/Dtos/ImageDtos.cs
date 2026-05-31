using imageProcessing.Api.Models;

namespace imageProcessing.Api.Dtos
{
    /// <summary>Summary of an image, used in the "all images" list for display.</summary>
    public record ImageListItemDto(
        Guid Id,
        string FileName,
        int Width,
        int Height,
        ImageStatus Status,
        string DownloadUrl);

    /// <summary>Full details of a single image.</summary>
    public record ImageDetailsDto(
        Guid Id,
        string FileName,
        string Extension,
        int Width,
        int Height,
        double FileSize,
        ImageStatus Status,
        IReadOnlyList<string> PipelineHistory);
}
