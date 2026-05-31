namespace imageProcessing.Api.Dtos
{
    /// <summary>How many images a given pipeline is processing right now.</summary>
    public record PipelineStatusDto(string Pipeline, int ActiveImages);
}
