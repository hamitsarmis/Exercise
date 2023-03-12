using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using PublicApi.Helpers;
using PublicApi.Services;
using System.IO.Compression;

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
        public IActionResult GetJobs([FromQuery]
            PaginationParams paginationParams)
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
