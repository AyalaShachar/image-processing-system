namespace imageProcessing.Api.Models
{
    /// <summary>
    /// Represents a single uploaded image and the state of its processing.
    /// Stored in-memory by the <see cref="Services.IImageRepository"/>.
    /// </summary>
    public class ImageItem
    {
        /// <summary>Unique identifier for the image.</summary>
        public Guid Id { get; set; } = Guid.NewGuid();

        /// <summary>Original file name as uploaded by the client.</summary>
        public string FileName { get; set; } = string.Empty;

        /// <summary>File extension (e.g. ".jpg", ".png").</summary>
        public string Extension { get; set; } = string.Empty;

        /// <summary>Path of the file on disk.</summary>
        public string FilePath { get; set; } = string.Empty;

        /// <summary>Current (possibly modified) width of the image in pixels.</summary>
        public int Width { get; set; }

        /// <summary>Current (possibly modified) height of the image in pixels.</summary>
        public int Height { get; set; }

        /// <summary>Size of the file in megabytes (MB).</summary>
        public double FileSize { get; set; }

        /// <summary>Current processing status of the image.</summary>
        public ImageStatus Status { get; set; } = ImageStatus.InProcess;

        /// <summary>Ordered history of the pipelines this image has passed through.</summary>
        public List<string> PipelineHistory { get; set; } = new();
    }
}
