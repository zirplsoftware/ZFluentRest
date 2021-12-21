using System.Net;

namespace Zirpl.FluentRestClient
{
    public interface IHttpCallContext
    {
        string Url { get; }
        HttpResponseMessage HttpResponseMessage { get; }
        HttpStatusCode? HttpResponseStatusCode { get; }
        string HttpResponseBody { get; }
        string HttpRequestBody { get; }
        bool LogRequestContent { get; }
        IHttpCallContext AddUrlSegment<T>(T relativeUrl);
        IHttpCallContext AddUrlParameter(string name, object value);

        void Get(int? attempts = null, IUnsuccessfulHttpStatusCodeHandler? handler = null);
        TResponse Get<TResponse>(IHttpResponseProcessor<TResponse>? processor, int? attempts = null);
        TResponse GetAndParseJson<TResponse>(int? attempts = null, IUnsuccessfulHttpStatusCodeHandler? handler = null);

        void Post(HttpContent? requestContent = null, int? attempts = null,
            bool logRequestContent = false, IUnsuccessfulHttpStatusCodeHandler? handler = null);
        TResponse Post<TResponse>(HttpContent? requestContent, IHttpResponseProcessor<TResponse>? processor,
            int? attempts = null,
            bool logRequestContent = false);
        TResponse Post<TResponse>(IHttpResponseProcessor<TResponse>? processor,
            int? attempts = null, bool logRequestContent = false);
        TResponse PostAndParseJson<TResponse>(HttpContent? requestContent = null,
            int? attempts = null, bool logRequestContent = false,
            IUnsuccessfulHttpStatusCodeHandler? handler = null);

        void PostJson(object request, int? attempts = null, bool logRequestContent = false,
            IUnsuccessfulHttpStatusCodeHandler? handler = null);
        TResponse PostJson<TResponse>(object request, IHttpResponseProcessor<TResponse>? processor,
            int? attempts = null, bool logRequestContent = false);
        TResponse PostJson<TResponse>(object request, int? attempts = null,
            bool logRequestContent = false, IUnsuccessfulHttpStatusCodeHandler? handler = null);
    }
}