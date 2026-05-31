using imageProcessing.Api.Models;

namespace imageProcessing.Api.Services
{
    /// <summary>
    /// Handles persisting uploaded image files to disk and reading their
    /// metadata (dimensions, size, extension) into an <see cref="ImageItem"/>.
    /// </summary>
    public interface IImageStorageService
    {
        /// <summary>
        /// Saves the uploaded file to disk and builds an <see cref="ImageItem"/>
        /// populated with its real width/height, size (MB) and extension.
        /// The returned item starts in <see cref="ImageStatus.InProcess"/>.
        /// </summary>
        Task<ImageItem> SaveAsync(IFormFile file, CancellationToken cancellationToken = default);

        /// <summary>Returns the absolute path of the stored file, or null if missing.</summary>
        string? GetPhysicalPath(ImageItem image);
    }
}
