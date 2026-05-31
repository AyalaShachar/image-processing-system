using imageProcessing.Api.Dtos;
using imageProcessing.Api.Models;
using imageProcessing.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace imageProcessing.Api.Controllers
{
    [ApiController]
    [Route("[controller]")] // -> /images
    public class ImagesController : ControllerBase
    {
        private readonly IImageRepository _repository;
        private readonly IImageStorageService _storage;
        private readonly ILogger<ImagesController> _logger;

        public ImagesController(
            IImageRepository repository,
            IImageStorageService storage,
            ILogger<ImagesController> logger)
        {
            _repository = repository;
            _storage = storage;
            _logger = logger;
        }

        /// <summary>
        /// POST /images — uploads a new image, stores it and starts processing.
        /// (The asynchronous pipeline trigger is wired in a later step.)
        /// </summary>
        [HttpPost]
        public async Task<ActionResult<ImageDetailsDto>> Upload(IFormFile file, CancellationToken ct)
        {
            if (file is null || file.Length == 0)
                return BadRequest("No file was uploaded.");

            ImageItem image;
            try
            {
                image = await _storage.SaveAsync(file, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Failed to store uploaded image '{FileName}'.", file.FileName);
                return BadRequest("The uploaded file could not be read as an image.");
            }

            _repository.Add(image);
            _logger.LogInformation("Stored image {Id} ({Width}x{Height}).", image.Id, image.Width, image.Height);

            // TODO (step 3): kick off ImagePipeline asynchronously here.

            return CreatedAtAction(nameof(GetById), new { id = image.Id }, ToDetails(image));
        }

        /// <summary>
        /// GET /images — list of images to display (Finished or ProcessError only).
        /// </summary>
        [HttpGet]
        public ActionResult<IEnumerable<ImageListItemDto>> GetAll()
        {
            var items = _repository.GetAll()
                .Where(i => i.Status is ImageStatus.Finished or ImageStatus.ProcessError)
                .Select(ToListItem);

            return Ok(items);
        }

        /// <summary>GET /images/{id} — full details including pipeline history.</summary>
        [HttpGet("{id:guid}")]
        public ActionResult<ImageDetailsDto> GetById(Guid id)
        {
            var image = _repository.GetById(id);
            if (image is null)
                return NotFound();

            return Ok(ToDetails(image));
        }

        /// <summary>GET /images/{id}/download — downloads the stored file.</summary>
        [HttpGet("{id:guid}/download")]
        public ActionResult Download(Guid id)
        {
            var image = _repository.GetById(id);
            if (image is null)
                return NotFound();

            var path = _storage.GetPhysicalPath(image);
            if (path is null)
                return NotFound("The image file is missing from disk.");

            var contentType = GetContentType(image.Extension);
            return PhysicalFile(path, contentType, image.FileName);
        }

        // --- mapping helpers ---

        private ImageListItemDto ToListItem(ImageItem i) => new(
            i.Id, i.FileName, i.Width, i.Height, i.Status,
            Url.Action(nameof(Download), "Images", new { id = i.Id }) ?? string.Empty);

        private static ImageDetailsDto ToDetails(ImageItem i) => new(
            i.Id, i.FileName, i.Extension, i.Width, i.Height, i.FileSize, i.Status, i.PipelineHistory);

        private static string GetContentType(string extension) => extension switch
        {
            ".jpg" or ".jpeg" => "image/jpeg",
            ".png" => "image/png",
            ".gif" => "image/gif",
            ".bmp" => "image/bmp",
            ".webp" => "image/webp",
            _ => "application/octet-stream"
        };
    }
}
