namespace Zirpl.FluentRestClient.Retries
{
    public class ActionRetrier : RetrierBase
    {
        public Action<int> Action { get; set; }

        public void Execute()
        {
            if (Action == null) throw new InvalidOperationException("Action is required");
            DoExecute();
        }

        protected override object? InvokeAction()
        {
            Action(CurrentAttempt);
            return null;
        }
    }
}