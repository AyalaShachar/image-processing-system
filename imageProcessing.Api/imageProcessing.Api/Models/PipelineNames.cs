namespace imageProcessing.Api.Models
{
    /// <summary>
    /// Canonical pipeline names, used both for the image's processing history
    /// and as keys for the live pipeline monitor.
    /// </summary>
    public static class PipelineNames
    {
        public const string Image = "ImagePipeline";
        public const string Square = "SquarePipeline";
        public const string Circle = "CirclePipeline";
        public const string Slow = "SlowPipeline";
        public const string Star = "StarPipeline";

        /// <summary>The timed (work) pipelines tracked by the monitor.</summary>
        public static readonly string[] Timed = { Square, Circle, Slow, Star };
    }
}
