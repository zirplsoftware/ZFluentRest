namespace Zirpl.FluentRestClient.Retries
{
    internal class PostAttemptReport
    {
        public PostAttemptReport(int attemptNumber, int maxAttempts, bool wasSuccessful, Exception? exception)
        {
            AttemptNumber = attemptNumber;
            MaxAttempts = maxAttempts;
            WasSuccessful = wasSuccessful;
            Exception = exception;
        }

        public int AttemptNumber { get; }
        public int MaxAttempts { get; }
        public bool WasSuccessful { get; }
        public Exception? Exception { get; }
    }
}