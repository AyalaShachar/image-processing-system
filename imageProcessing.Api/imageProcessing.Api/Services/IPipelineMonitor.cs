namespace imageProcessing.Api.Services
{
    /// <summary>
    /// Tracks, in real time, how many images are currently being processed by
    /// each pipeline. Thread-safe singleton powering the GET /pipelines endpoint.
    /// </summary>
    public interface IPipelineMonitor
    {
        /// <summary>An image entered the given pipeline (increments its count).</summary>
        void Enter(string pipeline);

        /// <summary>An image left the given pipeline (decrements its count).</summary>
        void Exit(string pipeline);

        /// <summary>Snapshot of the active image count per pipeline.</summary>
        IReadOnlyDictionary<string, int> GetActiveCounts();
    }
}
