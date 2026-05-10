using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PublicApi.Helpers;
using PublicApi.Interfaces;

namespace PublicApi.Controllers
{
    [ApiController]
    [Route("[controller]")]
    public class QueueController : ControllerBase
    {
        private const int DefaultMaxInputLength = 1_000_000;
        private readonly IQueueService _queueService;
        private readonly int _maxInputLength;

        public QueueController(IQueueService queueService, IConfiguration configuration)
        {
            _queueService = queueService;
            _maxInputLength = configuration.GetValue("MaxInputLength", DefaultMaxInputLength);
        }

        [HttpPost("enqueue")]
        [Authorize]
        public IActionResult Enqueue(int[] item)
        {
            if (item is null)
                return BadRequest("Input array is required.");
            if (item.Length > _maxInputLength)
                return BadRequest($"Input exceeds maximum length of {_maxInputLength}.");
            var result = _queueService.Enqueue(item);
            return Ok(result);
        }

        [Authorize(Policy = "RequireAdminRole")]
        [HttpGet("get-jobs")]
        public IActionResult GetJobs([FromQuery] PaginationParams paginationParams)
        {
            var result = _queueService.GetJobs(paginationParams);
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
