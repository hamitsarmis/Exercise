namespace Client
{
    internal class AppSettings
    {
        public int ConcurrentWorkers { get; set; } = 1000;
        public int RequestsPerWorker { get; set; } = 100;
        public string RemoteAddress { get; set; } = "http://localhost:5189/";
    }
}
