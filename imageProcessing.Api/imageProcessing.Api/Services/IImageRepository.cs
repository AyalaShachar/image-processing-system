using imageProcessing.Api.Models;

namespace imageProcessing.Api.Services
{
    /// <summary>
    /// In-memory store for uploaded images. Registered as a Singleton and
    /// implemented in a thread-safe manner so it can be accessed concurrently
    /// from background pipeline tasks and incoming API requests.
    /// </summary>
    public interface IImageRepository
    {
        /// <summary>Adds a new image to the store.</summary>
        void Add(ImageItem image);

        /// <summary>Returns the image with the given id, or null if not found.</summary>
        ImageItem? GetById(Guid id);

        /// <summary>Returns all images currently in the store.</summary>
        IReadOnlyCollection<ImageItem> GetAll();
    }
}
