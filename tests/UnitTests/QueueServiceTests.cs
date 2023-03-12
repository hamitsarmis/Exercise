using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PublicApi.Services;

namespace UnitTests;

public class QueueServiceTests
{

    private readonly QueueService _queueService;

    public QueueServiceTests()
    {
        var configurationMock = new Mock<IConfiguration>();
        _queueService = new QueueService(configurationMock.Object, new NullLogger<QueueService>());
    }

    [Fact]
    public async void Enqueues_And_Dequeues()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        await _queueService.StartAsync(cancellationTokenSource.Token);

        int[] array = new int[] { 10, 0, 9, 1, 8, 2, 7, 3, 6, 4, 5 };

        var id = _queueService.Enqueue(array);
        var job = _queueService.GetJob(id);

        await Task.Delay(1000);
        await _queueService.StopAsync(CancellationToken.None);

        Assert.NotNull(job);
        Assert.True(job.Status == PublicApi.JobState.Completed);
        Assert.True(string.Join("", job.Output) == "012345678910");
    }

    [Fact]
    public async void GetJobs_ReturnsList()
    {
        var cancellationTokenSource = new CancellationTokenSource();
        await _queueService.StartAsync(cancellationTokenSource.Token);

        await Task.Delay(1000);
        await _queueService.StopAsync(CancellationToken.None);

        var allJobs = _queueService.GetJobs(new PublicApi.Helpers.PaginationParams
        {
            PageSize = 10,
            PageNumber = 0
        });

        Assert.NotNull(allJobs);
    }
}