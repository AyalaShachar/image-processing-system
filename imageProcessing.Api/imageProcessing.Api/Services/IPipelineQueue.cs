namespace imageProcessing.Api.Services
{
    /// <summary>
    /// Asynchronous queue of image ids waiting to be processed. Allows the API
    /// to accept an upload and return immediately while processing runs in the
    /// background.
    /// </summary>
    public interface IPipelineQueue
    {
        /// <summary>Queues an image id for background processing.</summary>
        void Enqueue(Guid imageId);

        /// <summary>Streams queued image ids until cancellation.</summary>
        IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken cancellationToken);
    }
}
