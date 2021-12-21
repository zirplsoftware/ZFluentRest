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

        public RetrierException()
        {
        }

        public RetrierException(string message) : base(message)
        {
        }

        public RetrierException(string message, Exception inner) : base(message, inner)
        {
        }

        protected RetrierException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public AttemptError[] Errors { get; set; }

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