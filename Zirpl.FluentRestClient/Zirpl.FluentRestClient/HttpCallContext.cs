using System.Net;
using System.Net.Http.Headers;
using System.Text;
using Newtonsoft.Json;
using Zirpl.FluentRestClient.Logging;
using Zirpl.FluentRestClient.Retries;

namespace Zirpl.FluentRestClient
{
    public class HttpCallContext : IHttpCallContext
    {
        private readonly HttpClient _httpClient;
        private readonly StringBuilder _urlBuilder;
        private bool _hasAddedUrlParameter;
        
        public HttpCallContext(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _urlBuilder = new StringBuilder();
        }

        public string Url => $"{_httpClient.BaseAddress}{_urlBuilder}";
        public string HttpRequestBody { get; private set; }
        public bool LogRequestContent { get; private set; }
        public HttpResponseMessage HttpResponseMessage { get; private set; }
        public HttpStatusCode? HttpResponseStatusCode => HttpResponseMessage?.StatusCode;
        public string HttpResponseBody { get; private set; }


        public void Get(int? attempts = null, IUnsuccessfulHttpStatusCodeHandler? handler = null)
        {
            DoGet(attempts, handler);
        }
        
        public TResponse Get<TResponse>(IHttpResponseProcessor<TResponse>? processor, int ? attempts = null)
        {
            DoGet(attempts, processor);

            return ProcessResponse(processor);
        }
        
        public TResponse GetAndParseJson<TResponse>(int? attempts = null,
            IUnsuccessfulHttpStatusCodeHandler? handler = null)
        {
            DoGet(attempts, handler);

            return ProcessJsonResponse<TResponse>();
        }

        private void DoGet(int? attempts, IUnsuccessfulHttpStatusCodeHandler? handler)
        {
            try
            {
                var action = new Action<int>(i =>
                {
                    var url = _urlBuilder.ToString();
                    this.GetLog().Debug($"Calling GET to API:{url}");
                    HttpResponseMessage = _httpClient.GetAsync(url).Result;
                    HttpResponseBody = HttpResponseMessage.Content.ReadAsStringAsync().Result;
                    this.GetLog().Trace("HttpResponseBody: " + HttpResponseBody);
                    AssertSuccessfulStatusCode(handler);
                });

                if (attempts != null)
                {
                    new ActionRetrier
                    {
                        Action = action,
                        MaxAttempts = attempts.Value
                    }.Execute();
                }
                else
                {
                    action(1);
                }
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ApiException(this, "Unexpected error during Get operation", e);
            }
        }

        public void Post(HttpContent? requestContent = null, int? attempts = null, bool logRequestContent = false,
            IUnsuccessfulHttpStatusCodeHandler? handler = null)
        {
            DoPost(requestContent, logRequestContent, attempts, handler);
        }

        public TResponse Post<TResponse>(HttpContent? requestContent, IHttpResponseProcessor<TResponse>? processor,
            int? attempts = null, bool logRequestContent = false)
        {
            DoPost(requestContent, logRequestContent, attempts, processor);

            return ProcessResponse(processor);
        }

        public TResponse Post<TResponse>(IHttpResponseProcessor<TResponse>? processor, int? attempts = null,
            bool logRequestContent = false)
        {
            DoPost(null, logRequestContent, attempts, processor);

            return ProcessResponse(processor);
        }

        public TResponse PostAndParseJson<TResponse>(HttpContent? requestContent = null, int? attempts = null,
            bool logRequestContent = false, IUnsuccessfulHttpStatusCodeHandler? handler = null)
        {
            DoPost(requestContent, logRequestContent, attempts, handler);

            return ProcessJsonResponse<TResponse>();
        }




        public void PostJson(object request, int? attempts = null, bool logRequestContent = false,
            IUnsuccessfulHttpStatusCodeHandler? handler = null)
        {
            var requestJson = JsonConvert.SerializeObject(request);
            var requestContent = new StringContent(requestJson);
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            DoPost(requestContent, logRequestContent, attempts, handler);
        }

        public TResponse PostJson<TResponse>(object request, IHttpResponseProcessor<TResponse>? processor,
            int? attempts = null, bool logRequestContent = false)
        {
            var requestJson = JsonConvert.SerializeObject(request);
            var requestContent = new StringContent(requestJson);
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            DoPost(requestContent, logRequestContent, attempts, processor);

            return ProcessResponse(processor);
        }

        public TResponse PostJson<TResponse>(object request, int? attempts = null,
            bool logRequestContent = false, IUnsuccessfulHttpStatusCodeHandler? handler = null)
        {
            var requestJson = JsonConvert.SerializeObject(request);
            var requestContent = new StringContent(requestJson);
            requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");

            DoPost(requestContent, logRequestContent, attempts, handler);

            return ProcessJsonResponse<TResponse>();
        }


        private async Task DoPost(HttpContent requestContent, bool logRequestContent, int? attempts, IUnsuccessfulHttpStatusCodeHandler? handler)
        {
            LogRequestContent = logRequestContent;
            try
            {
                var action = new Action<int>(async i =>
                {
                    var url = _urlBuilder.ToString();
                    this.GetLog().Debug($"Calling POST to API:{url}");
                    HttpRequestBody = await requestContent.ReadAsStringAsync();
                    if (logRequestContent)
                    {
                        this.GetLog().Trace("HttpRequestBody: " + HttpRequestBody);
                    }
                    HttpResponseMessage = _httpClient.PostAsync(url, requestContent).Result;
                    HttpResponseBody = HttpResponseMessage.Content.ReadAsStringAsync().Result;
                    this.GetLog().Trace("HttpResponseBody: " + HttpResponseBody);
                    AssertSuccessfulStatusCode(handler);
                });

                if (attempts != null)
                {
                    new ActionRetrier
                    {
                        Action = action,
                        MaxAttempts = attempts.Value
                    }.Execute();
                }
                else
                {
                    action(1);
                }
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new ApiException(this, "Unexpected error during Post operation", e);
            }
        }

        private void DoAddUrlSegment(string relativeUrl)
        {
            if (_hasAddedUrlParameter)
            {
                throw new InvalidOperationException("Must be called before AddUrlParameter");
            }

            if (_urlBuilder.Length != 0
                && _urlBuilder[_urlBuilder.Length - 1] != '/')
            {
                _urlBuilder.Append('/');
            }
            _urlBuilder.Append(WebUtility.UrlEncode(relativeUrl));
        }

        public IHttpCallContext AddUrlSegment<T>(T relativeUrl)
        {
            DoAddUrlSegment(relativeUrl.ToString());
            return this;
        }

        public IHttpCallContext AddUrlParameter(string name, object value)
        {
            _urlBuilder.Append(!_hasAddedUrlParameter ? "?" : "&");
            _urlBuilder.Append($"{WebUtility.UrlEncode(name)}={(value == null ? string.Empty : WebUtility.UrlEncode(value.ToString()))}");
            _hasAddedUrlParameter = true;
            return this;
        }

        private TResponse ProcessResponse<TResponse>(IHttpResponseProcessor<TResponse>? processor)
        {
            try
            {
                return processor.ProcessResponse(this);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new UnexpectedApiResponseException(this, $"Could not process response", e);
            }
        }

        private TResponse ProcessJsonResponse<TResponse>()
        {
            try
            {
                return JsonConvert.DeserializeObject<TResponse>(HttpResponseBody);
            }
            catch (ApiException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new UnexpectedApiResponseException(this, $"Could not process response", e);
            }
        }

        private void AssertSuccessfulStatusCode(IUnsuccessfulHttpStatusCodeHandler? handler)
        {
            try
            {
                HttpResponseMessage.EnsureSuccessStatusCode();
            }
            catch (Exception e)
            {
                handler?.OnUnsuccessfulHttpStatusCode(this);
                throw new UnexpectedApiResponseException(this, $"HttpStatusCode indicates an unsuccessful response", e);
            }
        }
    }
}