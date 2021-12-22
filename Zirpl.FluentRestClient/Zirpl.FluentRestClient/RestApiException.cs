using System.Net;
using System.Runtime.Serialization;

namespace Zirpl.FluentRestClient
{
    [Serializable]
    public class RestApiException : Exception
    {
        //
        // For guidelines regarding the creation of new exception types, see
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/cpgenref/html/cpconerrorraisinghandlingguidelines.asp
        // and
        //    http://msdn.microsoft.com/library/default.asp?url=/library/en-us/dncscol/html/csharp07192001.asp
        //

        public RestApiException(RestApiCallContext callContext)
        {
            CallContext = callContext;
            Url = callContext.HttpResponseMessage?.RequestMessage?.RequestUri?.ToString() ?? callContext.Url;
            HttpStatusCode = callContext.HttpResponseStatusCode;
            RequestBody = callContext.HttpRequestBody;
            ResponseBody = callContext.HttpResponseBody;
        }

        public RestApiException(RestApiCallContext callContext, string message) : base(message)
        {
            CallContext = callContext;
            Url = callContext.HttpResponseMessage?.RequestMessage?.RequestUri?.ToString() ?? callContext.Url;
            HttpStatusCode = callContext.HttpResponseStatusCode;
            RequestBody = callContext.HttpRequestBody;
            ResponseBody = callContext.HttpResponseBody;
        }

        public RestApiException(RestApiCallContext callContext, string message, Exception inner) : base(message, inner)
        {
            CallContext = callContext;
            Url = callContext.HttpResponseMessage?.RequestMessage?.RequestUri?.ToString() ?? callContext.Url;
            HttpStatusCode = callContext.HttpResponseStatusCode;
            RequestBody = callContext.HttpRequestBody;
            ResponseBody = callContext.HttpResponseBody;
        }

        protected RestApiException(
            SerializationInfo info,
            StreamingContext context) : base(info, context)
        {
        }

        public RestApiCallContext? CallContext { get; private set; }
        public string Url { get; private set; }
        public HttpStatusCode? HttpStatusCode { get; private set; }
        public string RequestBody { get; private set; }
        public string ResponseBody { get; private set; }

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
                    Url,
                    RequestBody,
                    HttpStatusCode,
                    ResponseBody
                };
                var json = System.Text.Json.JsonSerializer.Serialize(messageData);
                return $"{base.Message}- CallContext: {json}";
            }
        }
    }
}