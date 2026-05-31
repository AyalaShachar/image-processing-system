namespace imageProcessing.Api.Services
{
    /// <summary>
    /// Hosted background service that consumes queued image ids and runs them
    /// through the pipeline engine. Each image is processed concurrently so the
    /// system can have several pipelines active at once.
    /// </summary>
    public class PipelineBackgroundService : BackgroundService
    {
        private readonly IPipelineQueue _queue;
        private readonly IPipelineEngine _engine;
        private readonly ILogger<PipelineBackgroundService> _logger;

        public PipelineBackgroundService(
            IPipelineQueue queue,
            IPipelineEngine engine,
            ILogger<PipelineBackgroundService> logger)
        {
            _queue = queue;
            _engine = engine;
            _logger = logger;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            _logger.LogInformation("Pipeline background service started.");

            await foreach (var imageId in _queue.DequeueAllAsync(stoppingToken))
            {
                // Fire-and-forget so multiple images process in parallel.
                _ = ProcessSafelyAsync(imageId, stoppingToken);
            }
        }

        private async Task ProcessSafelyAsync(Guid imageId, CancellationToken ct)
        {
            try
            {
                await _engine.RunAsync(imageId, ct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Pipeline processing failed for image {Id}.", imageId);
            }
        }
    }
}
