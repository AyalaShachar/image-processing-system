namespace imageProcessing.Api.Models
{
    /// <summary>
    /// Possible processing states of an image in the system.
    /// </summary>
    public enum ImageStatus
    {
        /// <summary>The image is currently being processed by a pipeline.</summary>
        InProcess,

        /// <summary>Processing completed successfully.</summary>
        Finished,

        /// <summary>Processing ended with an error.</summary>
        ProcessError
    }
}
