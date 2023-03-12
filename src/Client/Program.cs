using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

int totalRequests = 0, requestsFailedToSend = 0, otherExceptions = 0,
    requestsFailedWithErrorStatus = 0;

int concurrentWorkers = int.TryParse(config["ConcurrentWorkers"],
    out concurrentWorkers) ? concurrentWorkers : 1000;
int requestsPerWorker = int.TryParse(config["RequestsPerWorker"],
    out requestsPerWorker) ? requestsPerWorker : 100;
string remoteAddress = config["RemoteAddress"] ?? "https://localhost:7190/";

Console.WriteLine($"Creating and sending {concurrentWorkers * requestsPerWorker:N0}" +
    $" requests to {remoteAddress}");

var taskList = new List<Task>();

for (int i = 0; i < concurrentWorkers; i++)
{
    taskList.Add(Task.Run(async () =>
    {

        for (int j = 0; j < requestsPerWorker; j++)
        {
            Interlocked.Increment(ref totalRequests);

            Random random = new();
            int arrayLength = random.Next(1, 10000);
            int[] array = Enumerable.Range(0, arrayLength).
                Select(i => random.Next(100)).ToArray();
            try
            {
                using var client = new HttpClient()
                {
                    Timeout = TimeSpan.FromHours(1),
                };
                client.BaseAddress = new Uri(remoteAddress);
                var result = await client.PostAsync("/Queue/enqueue",
                    new StringContent(JsonSerializer.Serialize(array), Encoding.UTF8, "application/json"));
                if (!result.IsSuccessStatusCode)
                    Interlocked.Increment(ref requestsFailedWithErrorStatus);

            }
            catch (HttpRequestException)
            {
                Interlocked.Increment(ref requestsFailedToSend);
            }
            catch
            {
                Interlocked.Increment(ref otherExceptions);
            }
        }

    }));
}

await Task.WhenAll(taskList);

Console.WriteLine($"Done with {totalRequests} total requests, {requestsFailedToSend} could not be sent, " +
    $"{otherExceptions} had other exceptions and {requestsFailedWithErrorStatus} failed on the server");

Console.ReadLine();