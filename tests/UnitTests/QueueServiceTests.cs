using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using PublicApi.Services;

namespace UnitTests;

public class QueueServiceTests
{
    [Fact]
    public async void TestEnqueueAndDequeue()
    {
        var configurationMock = new Mock<IConfiguration>();

        var queueService = new QueueService(configurationMock.Object, new NullLogger<QueueService>());

        var cancellationTokenSource = new CancellationTokenSource();
        await queueService.StartAsync(cancellationTokenSource.Token);

        int[] array = new int[] { 10, 0, 9, 1, 8, 2, 7, 3, 6, 4, 5 };

        var id = queueService.Enqueue(array);
        var job = queueService.GetJob(id);

        await Task.Delay(1000);
        await queueService.StopAsync(CancellationToken.None);

        Assert.NotNull(job);
        Assert.True(job.Status == PublicApi.JobState.Completed);
        Assert.True(string.Join("", job.Output) == "012345678910");
    }
}