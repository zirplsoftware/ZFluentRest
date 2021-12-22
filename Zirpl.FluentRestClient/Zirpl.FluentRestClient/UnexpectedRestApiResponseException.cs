using System.Runtime.Serialization;

namespace Zirpl.FluentRestClient
{
    [Serializable]
    public class UnexpectedRestApiResponseException : RestApiException
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public UnexpectedRestApiResponseException(RestApiCallContext callContext) : base(callContext)
        {
        }

        public UnexpectedRestApiResponseException(RestApiCallContext callContext, string message) : base(callContext, message)
        {
        }

        public UnexpectedRestApiResponseException(RestApiCallContext callContext, string message, Exception inner) : base(callContext, message, inner)
        {
        }

        protected UnexpectedRestApiResponseException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }
    }
}