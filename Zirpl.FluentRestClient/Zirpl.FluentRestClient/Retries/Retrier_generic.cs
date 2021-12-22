using Zirpl.FluentRestClient.Logging;

namespace Zirpl.FluentRestClient.Retries;

public class Retrier<T>
{
    private readonly List<AttemptError> _errors;
    private int _currentAttempt;

    public Retrier(int? maxAttempts = 2)
    {
        _errors = new List<AttemptError>();
        MaxAttempts = maxAttempts ?? 1;
    }

    public Func<int, T> Action { get; set; }
    public Func<Exception, bool>? ShouldRetryEvaluator { get; set; }
    public int MaxAttempts { get; set; }
    public Action<PostAttemptReport>? PostAttemptAction { get; set; }

    public T Execute()
    {
        if (MaxAttempts <= 0) throw new InvalidOperationException($"Invalid MaxAttempts value: {MaxAttempts}");
        if (Action == null) throw new InvalidOperationException("Action is required");

        var succeeded = false;
        var shouldRetry = true;

        while (!succeeded
               && shouldRetry
               && MaxAttempts > _currentAttempt)
        {
            _currentAttempt++;
            if (_currentAttempt > 1)
            {
                this.GetLog().Warn($"Retrying attempt # {_currentAttempt} of {MaxAttempts} max attempts...");
            }
            try
            {
                var returnValue = Action(_currentAttempt);
                succeeded = true;
                return returnValue;
            }
            catch (Exception e)
            {
                _errors.Add(new AttemptError(_currentAttempt, e));
                shouldRetry = ShouldRetryEvaluator == null
                              || ShouldRetryEvaluator(e);
            }
            finally
            {
                PostAttemptAction?.Invoke(new PostAttemptReport(_currentAttempt, MaxAttempts, succeeded, succeeded ? null : _errors.Last().Exception));
            }
        }

        throw new RetrierException(_errors.ToArray(),
            $"MaxAttempts {MaxAttempts} exhausted without a successful run");
    }
}