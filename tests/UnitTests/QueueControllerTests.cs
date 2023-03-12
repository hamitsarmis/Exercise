using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Moq;
using PublicApi.Controllers;
using PublicApi.Helpers;
using PublicApi.Services;

namespace UnitTests
{
    public class QueueControllerTests
    {
        private readonly Mock<QueueService> _queueServiceMock;
        private readonly QueueController _controller;

        public QueueControllerTests()
        {
            var inMemorySettings = new List<KeyValuePair<string, string?>>
            {
                new KeyValuePair<string, string?>("PollingTime", "500")
            };
            IConfiguration configuration = new ConfigurationBuilder()
                .AddInMemoryCollection(inMemorySettings)
                .Build();
            _queueServiceMock = new Mock<QueueService>(configuration,
                Mock.Of<ILogger<QueueService>>());
            _controller = new QueueController(_queueServiceMock.Object);
        }

        [Fact]
        public void Enqueue_ReturnsOk()
        {
            int[] input = new int[] { 1, 10, 2, 9 };
            var result = _controller.Enqueue(input);

            Assert.IsType<OkObjectResult>(result);
            Assert.IsType<Guid>(((OkObjectResult)result).Value);
        }

        [Fact]
        public async void GetJobs_ReturnsOk()
        {
            var result = await _controller.GetJobs(new PaginationParams
            {
                PageNumber = 0,
                PageSize = 500
            });
            Assert.IsType<OkObjectResult>(result);
        }

        [Fact]
        public void GetJob_ReturnsNotFound()
        {
            var result = _controller.GetJob(Guid.NewGuid());
            Assert.IsType<NotFoundResult>(result);
        }

    }
}
