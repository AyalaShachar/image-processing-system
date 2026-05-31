using imageProcessing.Api.Models;
using Microsoft.Extensions.Options;

namespace imageProcessing.Api.Services
{
    /// <summary>
    /// Implements the rule-based routing engine and the four processing
    /// pipelines. Per the spec, dimensions are only changed as references
    /// (we never resize the file on disk).
    /// </summary>
    public class PipelineEngine : IPipelineEngine
    {
        private readonly IImageRepository _repository;
        private readonly IPipelineMonitor _monitor;
        private readonly PipelineOptions _options;
        private readonly ILogger<PipelineEngine> _logger;

        public PipelineEngine(
            IImageRepository repository,
            IPipelineMonitor monitor,
            IOptions<PipelineOptions> options,
            ILogger<PipelineEngine> logger)
        {
            _repository = repository;
            _monitor = monitor;
            _options = options.Value;
            _logger = logger;
        }

        /// <summary>The action the rule engine decided to take for an image.</summary>
        private enum RuleResult { Square, Circle, Slow, Star, Error, None }

        public async Task RunAsync(Guid imageId, CancellationToken cancellationToken = default)
        {
            var image = _repository.GetById(imageId);
            if (image is null)
            {
                _logger.LogWarning("Image {Id} not found; nothing to process.", imageId);
                return;
            }

            try
            {
                await RunImagePipelineAsync(image, cancellationToken);
            }
            catch (OperationCanceledException)
            {
                _logger.LogInformation("Processing of image {Id} was cancelled.", imageId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error while processing image {Id}.", imageId);
                image.Status = ImageStatus.ProcessError;
            }
        }

        // --- ImagePipeline: evaluate the rules and route to the first match ---

        private async Task RunImagePipelineAsync(ImageItem image, CancellationToken ct)
        {
            AppendHistory(image, PipelineNames.Image);

            switch (Evaluate(image))
            {
                case RuleResult.Square:
                    await RunSquareAsync(image, ct);
                    break;
                case RuleResult.Circle:
                    await RunCircleAsync(image, ct);
                    break;
                case RuleResult.Slow:
                    await RunSlowAsync(image, ct);
                    break;
                case RuleResult.Star:
                    await RunStarAsync(image, ct);
                    break;
                case RuleResult.Error:
                    image.Status = ImageStatus.ProcessError;
                    _logger.LogInformation("Image {Id} ended with ProcessError (rule 5).", image.Id);
                    break;
                default: // None — no rule matched, processing finished successfully.
                    image.Status = ImageStatus.Finished;
                    _logger.LogInformation("Image {Id} finished ({W}x{H}).", image.Id, image.Width, image.Height);
                    break;
            }
        }

        /// <summary>
        /// Evaluates the rules in order; the first satisfied rule wins.
        /// </summary>
        private RuleResult Evaluate(ImageItem image)
        {
            // Rule 1: width == height AND processed during daytime -> Square.
            if (image.Width == image.Height && IsDaytime())
                return RuleResult.Square;

            // Rule 2: width > height -> Circle.
            if (image.Width > image.Height)
                return RuleResult.Circle;

            // Rule 3: file > 3MB AND width < height -> Slow.
            if (image.FileSize > 3 && image.Width < image.Height)
                return RuleResult.Slow;

            // Rule 4: file type is JPG -> Star.
            if (image.Extension is ".jpg" or ".jpeg")
                return RuleResult.Star;

            // Rule 5: width < 20 -> ProcessError.
            if (image.Width < 20)
                return RuleResult.Error;

            // No rule matched -> finished successfully.
            return RuleResult.None;
        }

        // --- The four work pipelines ---

        /// <summary>SquarePipeline: delay 30s, then finish processing.</summary>
        private async Task RunSquareAsync(ImageItem image, CancellationToken ct)
        {
            await RunTimedAsync(image, PipelineNames.Square, _options.SquareDelaySeconds, ct);
            image.Status = ImageStatus.Finished;
            _logger.LogInformation("Image {Id} finished via SquarePipeline ({W}x{H}).", image.Id, image.Width, image.Height);
        }

        /// <summary>SlowPipeline: delay 15s, double the width, re-run ImagePipeline.</summary>
        private async Task RunSlowAsync(ImageItem image, CancellationToken ct)
        {
            await RunTimedAsync(image, PipelineNames.Slow, _options.SlowDelaySeconds, ct);
            image.Width *= 2;
            await RunImagePipelineAsync(image, ct);
        }

        /// <summary>CirclePipeline: random 15-30s delay, reduce width by 10px, re-run ImagePipeline.</summary>
        private async Task RunCircleAsync(ImageItem image, CancellationToken ct)
        {
            var seconds = Random.Shared.Next(_options.CircleMinDelaySeconds, _options.CircleMaxDelaySeconds + 1);
            await RunTimedAsync(image, PipelineNames.Circle, seconds, ct);
            image.Width -= 10;
            await RunImagePipelineAsync(image, ct);
        }

        /// <summary>StarPipeline: delay 10s, double the width, then run CirclePipeline.</summary>
        private async Task RunStarAsync(ImageItem image, CancellationToken ct)
        {
            await RunTimedAsync(image, PipelineNames.Star, _options.StarDelaySeconds, ct);
            image.Width *= 2;
            await RunCircleAsync(image, ct);
        }

        // --- helpers ---

        /// <summary>
        /// Records the pipeline in history, marks the image as active in the
        /// monitor for the duration of the delay, then clears it.
        /// </summary>
        private async Task RunTimedAsync(ImageItem image, string pipeline, int seconds, CancellationToken ct)
        {
            AppendHistory(image, pipeline);
            _monitor.Enter(pipeline);
            try
            {
                await Task.Delay(TimeSpan.FromSeconds(seconds * _options.SpeedFactor), ct);
            }
            finally
            {
                _monitor.Exit(pipeline);
            }
        }

        private bool IsDaytime()
        {
            var hour = DateTime.Now.Hour;
            return hour >= _options.DaytimeStartHour && hour <= _options.DaytimeEndHour;
        }

        /// <summary>
        /// Appends a history entry by swapping in a new list, so concurrent
        /// readers (the API) never observe a half-mutated collection.
        /// </summary>
        private static void AppendHistory(ImageItem image, string entry) =>
            image.PipelineHistory = new List<string>(image.PipelineHistory) { entry };
    }
}
