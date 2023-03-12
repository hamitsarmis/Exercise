using System.Collections.Concurrent;
using PublicApi.Entities;
using PublicApi.Helpers;

namespace PublicApi.Services
{
    public class QueueService : IHostedService
    {
        private readonly ConcurrentQueue<Job> _queue;
        private readonly ConcurrentDictionary<Guid, Job> _allJobs;
        private readonly ILogger<QueueService> _logger;
        private readonly int _pollingTime = 1000;
        private CancellationTokenSource _cancellationTokenSource;

        public QueueService(IConfiguration configuration, ILogger<QueueService> logger)
        {
            _queue = new();
            _allJobs = new();
            _ = int.TryParse(configuration["PollingTime"], out _pollingTime);
            _logger = logger;
        }

        public async Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Queue Service has started.");
            _cancellationTokenSource = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);

            await Task.Factory.StartNew(async () =>
            {
                while (!_cancellationTokenSource.IsCancellationRequested)
                {
                    if (_queue.IsEmpty)
                    {
                        await Task.Delay(_pollingTime, _cancellationTokenSource.Token);
                        continue;
                    }
                    try
                    {
                        if (_queue.TryDequeue(out var item))
                        {
                            _logger.LogInformation("Job {item.Id} is dequeued for sorting", item.Id);
                            await Task.Run(() => SortArray(item), _cancellationTokenSource.Token);
                        }
                    }
                    catch (OperationCanceledException)
                    {
                        break;
                    }
                }
                _logger.LogInformation("Queue Service has stopped.");
            }, cancellationToken, TaskCreationOptions.LongRunning, TaskScheduler.Default);
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _cancellationTokenSource?.Cancel();
            await Task.CompletedTask;
        }

        public Guid Enqueue(int[] item)
        {
            var job = new Job
            {
                Id = Guid.NewGuid(),
                EnqueuedAt = DateTime.UtcNow,
                Input = item
            };
            _queue.Enqueue(job);
            _allJobs.TryAdd(job.Id, job);
            _logger.LogInformation("Job {job.Id} is enqueued at: {job.EnqueuedAt}", job.Id, job.EnqueuedAt);
            return job.Id;
        }

        public IEnumerable<Job> GetJobs(PaginationParams paginationParams)
        {
            return PagedList<Job>.Create(_allJobs.Values, paginationParams.PageNumber, paginationParams.PageSize);
        }

        public Job GetJob(Guid jobId)
        {
            _allJobs.TryGetValue(jobId, out Job result);
            return result;
        }

        private void SortArray(Job job)
        {
            job.Output = new int[job.Input.Length];
            Array.Copy(job.Input, job.Output, job.Input.Length);
            Array.Sort(job.Output);
            job.Duration = DateTime.UtcNow - job.EnqueuedAt;
            job.Status = JobState.Completed;
            _logger.LogInformation("Job {job.Id} completion duration is {job.Duration}", job.Id, job.Duration);
        }

    }

}
