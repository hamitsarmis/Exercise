using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Configuration;
using Moq;
using PublicApi.Controllers;
using PublicApi.Entities;
using PublicApi.Helpers;
using PublicApi.Interfaces;

namespace UnitTests
{
    public class QueueControllerTests
    {
        private readonly Mock<IQueueService> _queueServiceMock;
        private readonly QueueController _controller;

        public QueueControllerTests()
        {
            _queueServiceMock = new Mock<IQueueService>();
            IConfiguration configuration = new ConfigurationBuilder().Build();
            _controller = new QueueController(_queueServiceMock.Object, configuration);
        }

        [Fact]
        public void Enqueue_ReturnsOk()
        {
            var expectedId = Guid.NewGuid();
            _queueServiceMock.Setup(x => x.Enqueue(It.IsAny<int[]>())).Returns(expectedId);

            int[] input = new int[] { 1, 10, 2, 9 };
            var result = _controller.Enqueue(input);

            Assert.IsType<OkObjectResult>(result);
            Assert.Equal(expectedId, ((OkObjectResult)result).Value);
        }

        [Fact]
        public void Enqueue_NullInput_ReturnsBadRequest()
        {
            var result = _controller.Enqueue(null!);

            Assert.IsType<BadRequestObjectResult>(result);
        }

        [Fact]
        public void GetJobs_ReturnsOk()
        {
            _queueServiceMock
                .Setup(x => x.GetJobs(It.IsAny<PaginationParams>()))
                .Returns(new PagedList<Job>(Array.Empty<Job>(), 0, 0, 10));

            var result = _controller.GetJobs(new PaginationParams
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
