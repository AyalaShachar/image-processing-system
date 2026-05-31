using imageProcessing.Api.Models;
using SixLabors.ImageSharp;

namespace imageProcessing.Api.Services
{
    /// <summary>
    /// Stores uploaded files under a local "Uploads" folder and reads their
    /// dimensions using ImageSharp.
    /// </summary>
    public class ImageStorageService : IImageStorageService
    {
        private readonly string _uploadsRoot;

        public ImageStorageService(IWebHostEnvironment env)
        {
            _uploadsRoot = Path.Combine(env.ContentRootPath, "Uploads");
            Directory.CreateDirectory(_uploadsRoot);
        }

        public async Task<ImageItem> SaveAsync(IFormFile file, CancellationToken cancellationToken = default)
        {
            if (file is null || file.Length == 0)
                throw new ArgumentException("Uploaded file is empty.", nameof(file));

            var id = Guid.NewGuid();
            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();

            // Store on disk using the id as the file name to avoid collisions.
            var storedFileName = $"{id}{extension}";
            var fullPath = Path.Combine(_uploadsRoot, storedFileName);

            await using (var stream = new FileStream(fullPath, FileMode.Create))
            {
                await file.CopyToAsync(stream, cancellationToken);
            }

            // Read the real dimensions from the saved file (header only).
            var info = await Image.IdentifyAsync(fullPath, cancellationToken);

            return new ImageItem
            {
                Id = id,
                FileName = file.FileName,
                Extension = extension,
                FilePath = fullPath,
                Width = info.Width,
                Height = info.Height,
                FileSize = Math.Round(file.Length / (1024.0 * 1024.0), 3), // MB
                Status = ImageStatus.InProcess
            };
        }

        public string? GetPhysicalPath(ImageItem image)
        {
            if (image is null || string.IsNullOrEmpty(image.FilePath))
                return null;

            return File.Exists(image.FilePath) ? image.FilePath : null;
        }
    }
}
