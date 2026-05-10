using System.Collections.Concurrent;
using System.Threading.Channels;
using PublicApi.Entities;
using PublicApi.Helpers;
using PublicApi.Interfaces;

namespace PublicApi.Services
{
    public class QueueService : IHostedService, IQueueService
    {
        private readonly Channel<Job> _channel;
        private readonly ConcurrentDictionary<Guid, Job> _allJobs;
        private readonly ILogger<QueueService> _logger;
        private readonly int _consumerCount;
        private Task[] _consumers = Array.Empty<Task>();

        public QueueService(IConfiguration configuration, ILogger<QueueService> logger)
        {
            _channel = Channel.CreateUnbounded<Job>(new UnboundedChannelOptions
            {
                SingleReader = false,
                SingleWriter = false
            });
            _allJobs = new();
            _consumerCount = configuration.GetValue("ConsumerCount", Environment.ProcessorCount);
            _logger = logger;
        }

        public Task StartAsync(CancellationToken cancellationToken)
        {
            _logger.LogInformation("Queue Service starting with {ConsumerCount} consumers.", _consumerCount);
            _consumers = new Task[_consumerCount];
            for (int i = 0; i < _consumerCount; i++)
            {
                _consumers[i] = Task.Run(() => ConsumeAsync(cancellationToken), cancellationToken);
            }
            return Task.CompletedTask;
        }

        public async Task StopAsync(CancellationToken cancellationToken)
        {
            _channel.Writer.TryComplete();
            if (_consumers.Length == 0) return;
            try
            {
                await Task.WhenAll(_consumers).WaitAsync(cancellationToken);
            }
            catch (OperationCanceledException)
            {
            }
            _logger.LogInformation("Queue Service has stopped.");
        }

        public Guid Enqueue(int[] item)
        {
            var job = new Job
            {
                Id = Guid.NewGuid(),
                EnqueuedAt = DateTime.UtcNow,
                Input = item
            };
            _allJobs.TryAdd(job.Id, job);
            if (!_channel.Writer.TryWrite(job))
                throw new InvalidOperationException("Queue is no longer accepting jobs.");
            _logger.LogInformation("Job {JobId} is enqueued at: {EnqueuedAt}", job.Id, job.EnqueuedAt);
            return job.Id;
        }

        public PagedList<Job> GetJobs(PaginationParams paginationParams)
        {
            return PagedList<Job>.Create(_allJobs.Values,
                paginationParams.PageNumber, paginationParams.PageSize);
        }

        public Job? GetJob(Guid jobId)
        {
            _allJobs.TryGetValue(jobId, out var result);
            return result;
        }

        private async Task ConsumeAsync(CancellationToken cancellationToken)
        {
            try
            {
                await foreach (var item in _channel.Reader.ReadAllAsync(cancellationToken))
                {
                    ProcessJob(item);
                }
            }
            catch (OperationCanceledException)
            {
            }
        }

        private void ProcessJob(Job job)
        {
            _logger.LogInformation("Job {JobId} is dequeued for sorting", job.Id);
            job.Status = JobState.Running;
            try
            {
                SortArray(job);
                job.Duration = DateTime.UtcNow - job.EnqueuedAt;
                // Status is written last so a reader observing Completed sees Output and Duration.
                job.Status = JobState.Completed;
                _logger.LogInformation("Job {JobId} completion duration is {Duration}", job.Id, job.Duration);
            }
            catch (Exception ex)
            {
                job.Duration = DateTime.UtcNow - job.EnqueuedAt;
                job.Error = ex.Message;
                job.Status = JobState.Failed;
                _logger.LogError(ex, "Job {JobId} failed after {Duration}", job.Id, job.Duration);
            }
        }

        private static void SortArray(Job job)
        {
            var output = new int[job.Input.Length];
            Array.Copy(job.Input, output, job.Input.Length);
            Array.Sort(output);
            job.Output = output;
        }
    }
}
