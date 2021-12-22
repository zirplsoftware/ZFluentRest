using System.Runtime.Serialization;
using System.Text;

namespace Zirpl.FluentRestClient.Retries
{
    [Serializable]
    public class RetrierException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public RetrierException(AttemptError[] errors)
        {
            Errors = errors;
        }

        public RetrierException(AttemptError[] errors, string message) : base(message)
        {
            Errors = errors;
        }

        public RetrierException(AttemptError[] errors, string message, Exception inner) : base(message, inner)
        {
            Errors = errors;
        }

        protected RetrierException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public AttemptError[] Errors { get; }

        public override string ToString()
        {
            var stringBuilder = new StringBuilder();
            stringBuilder.Append(base.ToString());
            foreach (var error in Errors)
            {
                stringBuilder.AppendLine().Append(error);
            }
            return stringBuilder.ToString();
        }
    }
}