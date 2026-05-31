using imageProcessing.Api.Dtos;
using imageProcessing.Api.Services;
using Microsoft.AspNetCore.Mvc;

namespace imageProcessing.Api.Controllers
{
    [ApiController]
    [Route("[controller]")] // -> /pipelines
    public class PipelinesController : ControllerBase
    {
        private readonly IPipelineMonitor _monitor;

        public PipelinesController(IPipelineMonitor monitor)
        {
            _monitor = monitor;
        }

        /// <summary>
        /// GET /pipelines — the currently active pipelines and how many images
        /// each one is processing right now.
        /// </summary>
        [HttpGet]
        public ActionResult<IEnumerable<PipelineStatusDto>> GetActive()
        {
            var result = _monitor.GetActiveCounts()
                .Select(kvp => new PipelineStatusDto(kvp.Key, kvp.Value))
                .OrderBy(p => p.Pipeline);

            return Ok(result);
        }
    }
}
