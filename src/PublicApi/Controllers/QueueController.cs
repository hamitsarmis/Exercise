using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PublicApi.Helpers;
using PublicApi.Services;

namespace PublicApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QueueController : ControllerBase
    {
        private readonly QueueService _queueService;

        public QueueController(QueueService queueService)
        {
            _queueService = queueService;
        }

        [HttpPost("enqueue")]
        [Authorize]
        public IActionResult Enqueue(int[] item)
        {
            var result = _queueService.Enqueue(item);
            return Ok(result);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("get-jobs")]
        public async Task<IActionResult> GetJobs([FromQuery]
            PaginationParams paginationParams)
        {
            var result = await _queueService.GetJobs(paginationParams);
            return Ok(result);
        }

        [HttpGet("get-job")]
        public IActionResult GetJob(Guid jobId)
        {
            var result = _queueService.GetJob(jobId);
            return result == null ? NotFound() : Ok(result);
        }
    }
}
