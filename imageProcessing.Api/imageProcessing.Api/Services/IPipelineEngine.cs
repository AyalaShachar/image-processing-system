namespace imageProcessing.Api.Services
{
    /// <summary>
    /// Runs the image-processing pipeline (rules + Square/Circle/Slow/Star)
    /// for a single image, asynchronously.
    /// </summary>
    public interface IPipelineEngine
    {
        /// <summary>Processes the image with the given id through the pipelines.</summary>
        Task RunAsync(Guid imageId, CancellationToken cancellationToken = default);
    }
}
