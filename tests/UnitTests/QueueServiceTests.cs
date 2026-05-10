using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging.Abstractions;
using PublicApi;
using PublicApi.Entities;
using PublicApi.Helpers;
using PublicApi.Services;

namespace UnitTests;

public class QueueServiceTests
{
    private static readonly TimeSpan WaitTimeout = TimeSpan.FromSeconds(5);

    private readonly QueueService _queueService;

    public QueueServiceTests()
    {
        IConfiguration configuration = new ConfigurationBuilder().Build();
        _queueService = new QueueService(configuration, new NullLogger<QueueService>());
    }

    [Fact]
    public async Task Enqueues_And_Dequeues()
    {
        await _queueService.StartAsync(CancellationToken.None);
        try
        {
            int[] array = new int[] { 10, 0, 9, 1, 8, 2, 7, 3, 6, 4, 5 };
            var id = _queueService.Enqueue(array);

            var job = await WaitForTerminalAsync(id);

            Assert.Equal(JobState.Completed, job.Status);
            Assert.Equal("012345678910", string.Join("", job.Output));
        }
        finally
        {
            await _queueService.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task GetJobs_ReturnsList()
    {
        await _queueService.StartAsync(CancellationToken.None);
        try
        {
            var id = _queueService.Enqueue(new[] { 1 });
            await WaitForTerminalAsync(id);

            var allJobs = _queueService.GetJobs(new PaginationParams
            {
                PageSize = 10,
                PageNumber = 0
            });

            Assert.NotNull(allJobs);
            Assert.IsType<PagedList<Job>>(allJobs);
            Assert.Equal(1, allJobs.TotalCount);
        }
        finally
        {
            await _queueService.StopAsync(CancellationToken.None);
        }
    }

    [Fact]
    public async Task Faulting_Job_Marks_Failed_And_Keeps_Consumer_Alive()
    {
        await _queueService.StartAsync(CancellationToken.None);
        try
        {
            // Input is null at the service layer (controller normally rejects this);
            // SortArray will NRE on job.Input.Length.
            var failingId = _queueService.Enqueue(null!);
            var failed = await WaitForTerminalAsync(failingId);

            Assert.Equal(JobState.Failed, failed.Status);
            Assert.False(string.IsNullOrEmpty(failed.Error));

            // Consumer must still process the next job after a failure.
            var goodId = _queueService.Enqueue(new[] { 3, 1, 2 });
            var good = await WaitForTerminalAsync(goodId);

            Assert.Equal(JobState.Completed, good.Status);
            Assert.Equal(new[] { 1, 2, 3 }, good.Output);
        }
        finally
        {
            await _queueService.StopAsync(CancellationToken.None);
        }
    }

    private async Task<Job> WaitForTerminalAsync(Guid jobId)
    {
        var deadline = DateTime.UtcNow + WaitTimeout;
        while (DateTime.UtcNow < deadline)
        {
            var job = _queueService.GetJob(jobId);
            if (job is not null && (job.Status == JobState.Completed || job.Status == JobState.Failed))
                return job;
            await Task.Delay(20);
        }
        throw new TimeoutException($"Job {jobId} did not reach a terminal state within {WaitTimeout}.");
    }
}
