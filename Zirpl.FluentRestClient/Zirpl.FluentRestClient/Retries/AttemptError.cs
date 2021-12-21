namespace Zirpl.FluentRestClient.Retries
{
    public class AttemptError
    { 
        public int AttemptNumber { get; set; }
        public Exception Exception { get; set; }

        public override string ToString()
        {
            return $"Attempt Number: {AttemptNumber}, Exception: {Exception}";
        }
    }
}