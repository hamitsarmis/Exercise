using System.Collections.Concurrent;
using PublicApi.Entities;
using PublicApi.Helpers;
using PublicApi.Interfaces;

namespace PublicApi.Services
{
    public class QueueService : IHostedService, IQueueService
    {
        private readonly ConcurrentQueue<Job> _queue;
        private readonly ConcurrentDictionary<Guid, Job> _allJobs;
        private readonly ILogger<QueueService> _logger;
        private readonly int _pollingTime = 500;
        private CancellationTokenSource _cancellationTokenSource;

        public QueueService(IConfiguration configuration, ILogger<QueueService> logger)
        {
            _queue = new();
            _allJobs = new();
            _pollingTime = configuration.GetValue("PollingTime", defaultValue: 500);
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

        public async Task<PagedList<Job>> GetJobs(PaginationParams paginationParams)
        {
            return await Task.FromResult(PagedList<Job>.Create(_allJobs.Values, 
                paginationParams.PageNumber, paginationParams.PageSize));
        }

        public Job GetJob(Guid jobId)
        {
            _allJobs.TryGetValue(jobId, out Job result);
            return result;
        }

        private void SortArray(Job job)
        {
            var newJob = job.Clone();
            newJob.Output = new int[newJob.Input.Length];
            Array.Copy(newJob.Input, newJob.Output, newJob.Input.Length);
            Array.Sort(newJob.Output);
            newJob.Duration = DateTime.UtcNow - newJob.EnqueuedAt;
            newJob.Status = JobState.Completed;
            _allJobs.AddOrUpdate(newJob.Id, newJob, (x, y) => newJob);
            _logger.LogInformation("Job {job.Id} completion duration is {job.Duration}", job.Id, job.Duration);
        }

    }

}
