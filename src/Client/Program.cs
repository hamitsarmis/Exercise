using Microsoft.Extensions.Configuration;
using System.Text;
using System.Text.Json;

var config = new ConfigurationBuilder()
    .AddJsonFile("appsettings.json", optional: false)
    .Build();

int totalRequests = 0, requestsFailedToSend = 0, otherExceptions = 0,
    requestsFailedWithErrorStatus = 0;

var appSettings = config.Get<Client.AppSettings>();

int concurrentWorkers = appSettings?.ConcurrentWorkers ?? 1000;
int requestsPerWorker = appSettings?.RequestsPerWorker ?? 100;
string remoteAddress = appSettings?.RemoteAddress ?? "https://localhost:7190/";

Console.WriteLine($"Creating and sending {concurrentWorkers * requestsPerWorker:N0}" +
    $" requests to {remoteAddress}");

using var httpClient = new HttpClient
{
    BaseAddress = new Uri(remoteAddress),
    Timeout = TimeSpan.FromHours(1),
};

var taskList = new List<Task>();

for (int i = 0; i < concurrentWorkers; i++)
{
    taskList.Add(Task.Run(async () =>
    {
        for (int j = 0; j < requestsPerWorker; j++)
        {
            Interlocked.Increment(ref totalRequests);

            int arrayLength = Random.Shared.Next(1, 10000);
            int[] array = new int[arrayLength];
            for (int k = 0; k < arrayLength; k++)
                array[k] = Random.Shared.Next(100);
            try
            {
                var result = await httpClient.PostAsync("/Queue/enqueue",
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

if (!Console.IsInputRedirected)
    Console.ReadLine();