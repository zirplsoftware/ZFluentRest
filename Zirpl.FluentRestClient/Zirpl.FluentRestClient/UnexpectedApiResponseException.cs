using System.Runtime.Serialization;

namespace Zirpl.FluentRestClient
{
    [Serializable]
    public class UnexpectedApiResponseException : ApiException
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public UnexpectedApiResponseException(IHttpCallContext callContext) : base(callContext)
        {
        }

        public UnexpectedApiResponseException(IHttpCallContext callContext, string message) : base(callContext, message)
        {
        }

        public UnexpectedApiResponseException(IHttpCallContext callContext, string message, Exception inner) : base(callContext, message, inner)
        {
        }

        protected UnexpectedApiResponseException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}