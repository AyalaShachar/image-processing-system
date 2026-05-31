using System.Collections.Concurrent;
using imageProcessing.Api.Models;

namespace imageProcessing.Api.Services
{
    /// <summary>
    /// Thread-safe, in-memory implementation of <see cref="IImageRepository"/>
    /// backed by a <see cref="ConcurrentDictionary{TKey,TValue}"/>.
    /// </summary>
    public class ImageRepository : IImageRepository
    {
        private readonly ConcurrentDictionary<Guid, ImageItem> _images = new();

        public void Add(ImageItem image)
        {
            ArgumentNullException.ThrowIfNull(image);
            _images[image.Id] = image;
        }

        public ImageItem? GetById(Guid id)
        {
            _images.TryGetValue(id, out var image);
            return image;
        }

        public IReadOnlyCollection<ImageItem> GetAll()
        {
            // Snapshot of the current values; safe to enumerate by the caller.
            return _images.Values.ToList();
        }
    }
}
