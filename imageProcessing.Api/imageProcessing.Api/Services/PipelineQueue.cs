using System.Threading.Channels;

namespace imageProcessing.Api.Services
{
    /// <summary>
    /// <see cref="IPipelineQueue"/> backed by an unbounded
    /// <see cref="Channel{T}"/> — a producer/consumer queue safe for concurrent use.
    /// </summary>
    public class PipelineQueue : IPipelineQueue
    {
        private readonly Channel<Guid> _channel = Channel.CreateUnbounded<Guid>();

        public void Enqueue(Guid imageId)
        {
            // Unbounded channel: writing always succeeds synchronously.
            _channel.Writer.TryWrite(imageId);
        }

        public IAsyncEnumerable<Guid> DequeueAllAsync(CancellationToken cancellationToken) =>
            _channel.Reader.ReadAllAsync(cancellationToken);
    }
}
