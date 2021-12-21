using Zirpl.FluentRestClient.Logging;

namespace Zirpl.FluentRestClient.Retries
{
    public abstract class RetrierBase
    {
        private readonly List<AttemptError> _errors;
        protected int CurrentAttempt { get; private set; }

        protected RetrierBase()
        {
            _errors = new List<AttemptError>();
            MaxAttempts = 2;
        }

        public Func<Exception, bool> ShouldRetryEvaluator { get; set; }
        public int MaxAttempts { get; set; }
        public Action<PostAttemptReport> PostAttemptAction { get; set; }

        protected object? DoExecute()
        {
            if (MaxAttempts <= 0) throw new InvalidOperationException($"Invalid MaxAttempts value: {MaxAttempts}");

            var succeeded = false;
            var shouldRetry = true;

            while (!succeeded
                   && shouldRetry
                   && MaxAttempts > CurrentAttempt)
            {
                CurrentAttempt++;
                if (CurrentAttempt > 1)
                {
                    this.GetLog().Warn($"Retrying attempt # {CurrentAttempt} of {MaxAttempts} max attempts...");
                }
                try
                {
                    var returnValue = InvokeAction();
                    succeeded = true;
                    return returnValue;
                }
                catch (Exception e)
                {
                    _errors.Add(new AttemptError {AttemptNumber = CurrentAttempt, Exception = e});
                    shouldRetry = ShouldRetryEvaluator == null
                                  || ShouldRetryEvaluator(e);
                }
                finally
                {
                    PostAttemptAction?.Invoke(new PostAttemptReport { AttemptNumber = CurrentAttempt, MaxAttempts = MaxAttempts, WasSuccessful = succeeded, Exception = succeeded ? null : _errors.Last().Exception });
                }
            }
            throw new RetrierException($"MaxAttempts {MaxAttempts} exhausted without a successful run") { Errors = _errors.ToArray() };
        }

        protected abstract object? InvokeAction();
    }
}