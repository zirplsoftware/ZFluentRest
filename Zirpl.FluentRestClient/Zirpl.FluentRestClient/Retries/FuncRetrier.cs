namespace Zirpl.FluentRestClient.Retries
{
    public class FuncRetrier<T> : RetrierBase
    {
        public Func<int, T> Action { get; set; }

        public T Execute()
        {
            if (Action == null) throw new InvalidOperationException("Action is required");
            return (T)DoExecute();
        }

        protected override object InvokeAction()
        {
            return Action(CurrentAttempt);
        }
    }
}