namespace Zirpl.FluentRestClient.Retries
{
    public class AttemptError
    {
        public AttemptError(int attemptNumber, Exception exception)
        {
            AttemptNumber = attemptNumber;
            Exception = exception;
        }

        public int AttemptNumber { get; }
        public Exception Exception { get; }

        public override string ToString()
        {
            return $"Attempt Number: {AttemptNumber}, Exception: {Exception}";
        }
    }
}