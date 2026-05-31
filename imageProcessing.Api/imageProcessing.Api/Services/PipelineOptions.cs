namespace imageProcessing.Api.Services
{
    /// <summary>
    /// Configurable pipeline delays. Bound from the "Pipeline" config section.
    /// Delays are expressed in seconds (per the spec) and multiplied by
    /// <see cref="SpeedFactor"/> so they can be shortened during development.
    /// </summary>
    public class PipelineOptions
    {
        /// <summary>
        /// Multiplier applied to every delay. 1.0 = real spec timings.
        /// e.g. 0.1 makes a 30s delay take 3s for development/testing.
        /// </summary>
        public double SpeedFactor { get; set; } = 1.0;

        public int SquareDelaySeconds { get; set; } = 30;
        public int SlowDelaySeconds { get; set; } = 15;
        public int StarDelaySeconds { get; set; } = 10;
        public int CircleMinDelaySeconds { get; set; } = 15;
        public int CircleMaxDelaySeconds { get; set; } = 30;

        /// <summary>The hour range (inclusive) considered "daytime" for rule 1.</summary>
        public int DaytimeStartHour { get; set; } = 8;
        public int DaytimeEndHour { get; set; } = 19;
    }
}
