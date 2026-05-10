using System.Text.Json.Serialization;
using PublicApi.Helpers;

namespace PublicApi.Entities
{
    public class Job
    {

        public Guid Id { get; set; }
        public DateTime EnqueuedAt { get; set; }
        [JsonConverter(typeof(TimeSpanConverter))]
        public TimeSpan Duration { get; set; }
        [JsonConverter(typeof(JsonStringEnumConverter))]
        public JobState Status { get; set; }
        public required int[] Input { get; set; }
        public int[]? Output { get; set; }
        public string? Error { get; set; }
    }
}
