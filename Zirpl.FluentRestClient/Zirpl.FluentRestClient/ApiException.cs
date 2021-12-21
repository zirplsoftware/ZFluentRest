using System.Runtime.Serialization;
using Zirpl.FluentRestClient.Logging;

namespace Zirpl.FluentRestClient
{
    [Serializable]
    public class ApiException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public ApiException(IHttpCallContext callContext)
        {
            CallContext = callContext;
        }

        public ApiException(IHttpCallContext callContext, string message) : base(message)
        {
            CallContext = callContext;
        }

        public ApiException(IHttpCallContext callContext, string message, Exception inner) : base(message, inner)
        {
            CallContext = callContext;
        }

        protected ApiException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public IHttpCallContext? CallContext { get; private set; }

        public override string Message
        {
            get
            {
                if (CallContext == null)
                {
                    return base.Message;
                }
                var messageData = new
                {
                    Url = CallContext.HttpResponseMessage?.RequestMessage?.RequestUri?.ToString() ?? CallContext.Url,
                    RequestBody = CallContext.LogRequestContent ? CallContext.HttpRequestBody : "[NOT LOGGED]",
                    HttpStatusCode = CallContext.HttpResponseStatusCode,
                    ResponseBody = CallContext.HttpResponseBody
                };
                return $"{base.Message}- CallContext: {messageData.ToLoggableJson()}";
            }
        }
    }
}