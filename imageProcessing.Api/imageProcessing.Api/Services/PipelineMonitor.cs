using System.Collections.Concurrent;
using imageProcessing.Api.Models;

namespace imageProcessing.Api.Services
{
    /// <summary>
    /// Thread-safe implementation of <see cref="IPipelineMonitor"/> backed by a
    /// <see cref="ConcurrentDictionary{TKey,TValue}"/> of pipeline -> active count.
    /// </summary>
    public class PipelineMonitor : IPipelineMonitor
    {
        private readonly ConcurrentDictionary<string, int> _activeCounts;

        public PipelineMonitor()
        {
            // Seed all timed pipelines at 0 so they always appear in the snapshot.
            _activeCounts = new ConcurrentDictionary<string, int>(
                PipelineNames.Timed.ToDictionary(name => name, _ => 0));
        }

        public void Enter(string pipeline) =>
            _activeCounts.AddOrUpdate(pipeline, 1, (_, current) => current + 1);

        public void Exit(string pipeline) =>
            _activeCounts.AddOrUpdate(pipeline, 0, (_, current) => Math.Max(0, current - 1));

        public IReadOnlyDictionary<string, int> GetActiveCounts() =>
            new Dictionary<string, int>(_activeCounts);
    }
}
