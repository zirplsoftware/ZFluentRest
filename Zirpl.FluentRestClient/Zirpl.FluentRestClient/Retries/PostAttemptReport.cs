namespace Zirpl.FluentRestClient.Retries
{
    public class PostAttemptReport
    {
        public int AttemptNumber { get; set; }
        public int MaxAttempts { get; set; }
        public bool WasSuccessful { get; set; }
        public Exception? Exception { get; set; }
    }
}