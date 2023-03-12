﻿using PublicApi.Entities;
using PublicApi.Helpers;

namespace PublicApi.Interfaces
{
    public interface IQueueService
    {
        Guid Enqueue(int[] item);

        IEnumerable<Job> GetJobs(PaginationParams paginationParams);

        Job GetJob(Guid jobId);
    }
}
