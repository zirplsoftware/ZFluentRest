using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Xml;
using System.Xml.Serialization;
using Zirpl.FluentRestClient.Retries;

namespace Zirpl.FluentRestClient
{
    public class RestApiCallContext
    {
        private readonly StringBuilder _urlBuilder;
        private readonly IDictionary<string, string[]> _requestHeaders;
        private HttpClient? _httpClient;
        private bool _hasAddedUrlParameter;
        private HttpContent? _requestContent = null;
        private string? _stringRequestContent = null;
        private object? _jsonRequestContent = null;
        private object? _xmlRequestContent = null;
        private bool _hasRequestContent = false;
        private int _retryCount = 0;
        private CancellationToken _cancellationToken;
        private IRestApiCallLogger? _logger;
        private AuthenticationHeaderValue _authenticationHeaderValue = null;

        public RestApiCallContext(HttpClient httpClient)
        {
            _httpClient = httpClient;
            _urlBuilder = new StringBuilder();
            _requestHeaders = new Dictionary<string, string[]>();
        }

        public string Url => $"{_httpClient?.BaseAddress}{_urlBuilder}";
        public string? HttpRequestBody { get; private set; }
        public HttpResponseMessage? HttpResponseMessage { get; private set; }
        public HttpStatusCode? HttpResponseStatusCode => HttpResponseMessage?.StatusCode;
        public string? HttpResponseBody { get; private set; }

        public RestApiCallContext WithHttpClient(HttpClient client)
        {
            _httpClient = client;
            return this;
        }
        
        public RestApiCallContext WithUrlSegment<T>(T relativeUrl)
        {
            if (relativeUrl == null)
            {
                throw new ArgumentNullException(nameof(relativeUrl));
            }
            if (_hasAddedUrlParameter)
            {
                throw new InvalidOperationException("Must be called before AddUrlParameter");
            }

            if (_urlBuilder.Length != 0
                && _urlBuilder[_urlBuilder.Length - 1] != '/')
            {
                _urlBuilder.Append('/');
            }
            _urlBuilder.Append(WebUtility.UrlEncode(relativeUrl.ToString()));

            return this;
        }

        public RestApiCallContext WithUrlParameter<T>(string name, T value)
        {
            if (name == null)
            {
                throw new ArgumentNullException(nameof(name));
            }
            _urlBuilder.Append(!_hasAddedUrlParameter ? "?" : "&");
            _urlBuilder.Append($"{WebUtility.UrlEncode(name)}={(value == null ? string.Empty : WebUtility.UrlEncode(value.ToString()))}");
            _hasAddedUrlParameter = true;
            return this;
        }

        public RestApiCallContext WithRequestContent(HttpContent content)
        {
            if (_hasRequestContent)
            {
                throw new InvalidOperationException("Already has request content");
            }

            _requestContent = content;
            _hasRequestContent = true;
            return this;
        }

        public RestApiCallContext WithRequestContent(string content)
        {
            if (_hasRequestContent)
            {
                throw new InvalidOperationException("Already has request content");
            }

            _stringRequestContent = content;
            _hasRequestContent = true;
            return this;
        }

        public RestApiCallContext WithJsonRequestContent(object content)
        {
            if (_hasRequestContent)
            {
                throw new InvalidOperationException("Already has request content");
            }

            _jsonRequestContent = content;
            _hasRequestContent = true;
            return this;
        }

        public RestApiCallContext WithXmlRequestContent(object content)
        {
            if (_hasRequestContent)
            {
                throw new InvalidOperationException("Already has request content");
            }

            _xmlRequestContent = content;
            _hasRequestContent = true;
            return this;
        }

        public RestApiCallContext WithRetries(int retryCount)
        {
            _retryCount = retryCount;
            return this;
        }

        public RestApiCallContext WithLogger(IRestApiCallLogger logger)
        {
            _logger = logger;
            return this;
        }

        public RestApiCallContext WithRequestHeader(string name, params string[] values)
        {
            _requestHeaders.Add(name, values);
            return this;
        }

        public RestApiCallContext WithBasicAuthentication(string username, string password)
        {
            var basicAuthenticationHeaderValue = Convert.ToBase64String(System.Text.Encoding.ASCII.GetBytes($"{username}:{password}"));
            _authenticationHeaderValue = new AuthenticationHeaderValue("Basic", basicAuthenticationHeaderValue);
            return this;
        }

        public RestApiCallContext WithCancellationToken(CancellationToken cancellationToken)
        {
            _cancellationToken = cancellationToken;
            return this;
        }

        public async Task<RestApiCallContext> GetAsync()
        {
            if (_hasRequestContent)
            {
                throw new InvalidOperationException("Cannot call Get with content");
            }

            if (HttpResponseMessage != null)
            {
                throw new InvalidOperationException("Cannot call more than one Http method");
            }

            try
            {
                var url = _urlBuilder.ToString();
                _logger?.Log($"Calling GET on {url}");

                Func<int, Task>? action = null;
                if (_httpClient != null)
                {
                    _httpClient.DefaultRequestHeaders.Authorization = _authenticationHeaderValue;
                    action = async i =>
                    {
                        HttpResponseMessage = await _httpClient.GetAsync(url, _cancellationToken);
                    };
                }
                else
                {
                    action = async i =>
                    {
                        using (var httpClient = new HttpClient())
                        {
                            httpClient.DefaultRequestHeaders.Authorization = _authenticationHeaderValue;
                            HttpResponseMessage = await httpClient.GetAsync(url, _cancellationToken);
                        }
                    };
                }

                if (_retryCount > 0)
                {
                    await new AsyncRetrier(_retryCount + 1)
                    {
                        Action = action
                    }.ExecuteAsync();
                }
                else
                {
                    await action(1);
                }
            }
            catch (RestApiException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new RestApiException(this, "Unexpected error during Get operation", e);
            }

            return this;
        }

        public async Task<RestApiCallContext> PostAsync()
        {
            if (HttpResponseMessage != null)
            {
                throw new InvalidOperationException("Cannot call more than one Http method");
            }

            try
            {
                var action = new Func<int, Task>(async i =>
                {
                    var url = _urlBuilder.ToString();
                    _logger?.Log($"Calling POST on {url}");
                    var requestContent = GetRequestContent();
                    HttpRequestBody = await requestContent.ReadAsStringAsync(_cancellationToken);
                    _logger?.Log($"HTTPRequestBody: {HttpRequestBody}");
                    HttpResponseMessage = await _httpClient.PostAsync(url, requestContent, _cancellationToken);
                    HttpResponseBody = await HttpResponseMessage.Content.ReadAsStringAsync(_cancellationToken);
                    _logger?.Log($"HTTPResponseBody: {HttpResponseBody}");
                    AssertSuccessfulStatusCode();
                });

                if (_retryCount > 0)
                {
                    await new AsyncRetrier(_retryCount + 1)
                    {
                        Action = action
                    }.ExecuteAsync();
                }
                else
                {
                    await action(1);
                }
            }
            catch (RestApiException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new RestApiException(this, "Unexpected error during Post operation", e);
            }

            return this;
        }
        
        public async Task<RestApiCallContext> PutAsync()
        {
            if (HttpResponseMessage != null)
            {
                throw new InvalidOperationException("Cannot call more than one Http method");
            }

            try
            {
                var action = new Func<int, Task>(async i =>
                {
                    var url = _urlBuilder.ToString();
                    _logger?.Log($"Calling PUT on {url}");
                    var requestContent = GetRequestContent();
                    HttpRequestBody = await requestContent.ReadAsStringAsync(_cancellationToken);
                    _logger?.Log($"HTTPRequestBody: {HttpRequestBody}");
                    HttpResponseMessage = await _httpClient.PutAsync(url, requestContent, _cancellationToken);
                    HttpResponseBody = await HttpResponseMessage.Content.ReadAsStringAsync(_cancellationToken);
                    _logger?.Log($"HTTPResponseBody: {HttpResponseBody}");
                    AssertSuccessfulStatusCode();
                });

                if (_retryCount > 0)
                {
                    await new AsyncRetrier(_retryCount + 1)
                    {
                        Action = action
                    }.ExecuteAsync();
                }
                else
                {
                    await action(1);
                }
            }
            catch (RestApiException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new RestApiException(this, "Unexpected error during Put operation", e);
            }

            return this;
        }

        public async Task<RestApiCallContext> DeleteAsync()
        {
            if (HttpResponseMessage != null)
            {
                throw new InvalidOperationException("Cannot call more than one Http method");
            }
            if (_hasRequestContent)
            {
                throw new InvalidOperationException("Cannot call Delete with content");
            }

            try
            {
                var action = new Func<int, Task>(async i =>
                {
                    var url = _urlBuilder.ToString();
                    _logger?.Log($"Calling DELETE on {url}");
                    HttpResponseMessage = await _httpClient.DeleteAsync(url, _cancellationToken);
                    HttpResponseBody = await HttpResponseMessage.Content.ReadAsStringAsync(_cancellationToken);
                    _logger?.Log($"HTTPResponseBody: {HttpResponseBody}");
                    AssertSuccessfulStatusCode();
                });

                if (_retryCount > 0)
                {
                    await new AsyncRetrier(_retryCount + 1)
                    {
                        Action = action
                    }.ExecuteAsync();
                }
                else
                {
                    await action(1);
                }
            }
            catch (RestApiException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new RestApiException(this, "Unexpected error during Delete operation", e);
            }

            return this;
        }

        private HttpContent GetRequestContent()
        {
            HttpContent requestContent = null;
            if (_hasRequestContent)
            {
                if (_requestContent != null)
                {
                    requestContent = _requestContent;
                }
                else if (_stringRequestContent != null)
                {
                    requestContent = new StringContent(_stringRequestContent);
                }
                else if (_jsonRequestContent != null)
                {
                    requestContent = new StringContent(System.Text.Json.JsonSerializer.Serialize(_jsonRequestContent));
                    requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/json");
                }
                else if (_xmlRequestContent != null)
                {
                    var serializer = new XmlSerializer(_xmlRequestContent.GetType());
                    string xml;

                    using (var stringWriter = new StringWriter())
                    {
                        using (var xmlWriter = XmlWriter.Create(stringWriter))
                        {
                            serializer.Serialize(xmlWriter, _xmlRequestContent);
                            xml = stringWriter.ToString(); // Your XML
                        }
                    }

                    requestContent = new StringContent(xml);
                    requestContent.Headers.ContentType = new MediaTypeHeaderValue("application/xml");
                }
            }

            if (requestContent == null
                && _requestHeaders.Any())
            {
                requestContent = new StringContent("");
            }

            foreach (var requestHeader in _requestHeaders)
            {
                requestContent.Headers.Add(requestHeader.Key, requestHeader.Value);
            }

            return requestContent;
        }
        
        public async Task<T> ParseJsonResponse<T>()
        {
            if (HttpResponseMessage == null)
            {
                throw new InvalidOperationException("Must execute call before parsing response");
            }

            try
            {
                HttpResponseBody = await HttpResponseMessage.Content.ReadAsStringAsync(_cancellationToken);
                _logger?.Log($"HTTPResponseBody: {HttpResponseBody}");
                return HttpResponseBody == null ? default(T) : System.Text.Json.JsonSerializer.Deserialize<T>(HttpResponseBody);
            }
            catch (RestApiException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new UnexpectedRestApiResponseException(this, $"Could not process response", e);
            }
        }

        public async Task<T> ParseXmlResponse<T>()
        {
            if (HttpResponseMessage == null)
            {
                throw new InvalidOperationException("Must execute call before parsing response");
            }

            try
            {
                HttpResponseBody = await HttpResponseMessage.Content.ReadAsStringAsync(_cancellationToken);
                _logger?.Log($"HTTPResponseBody: {HttpResponseBody}");
                if (HttpResponseBody == null)
                {
                    return default(T);
                }
                var serializer = new XmlSerializer(typeof(T));
                using (var responseBodyReader = new StringReader(HttpResponseBody))
                {
                    using (var reader = XmlReader.Create(responseBodyReader))
                    {
                        return (T)serializer.Deserialize(reader);
                    }
                }
            }
            catch (RestApiException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new UnexpectedRestApiResponseException(this, $"Could not process response", e);
            }
        }

        public async Task<T> ParseResponse<T>(IHttpResponseParser<T> parser)
        {
            if (HttpResponseMessage == null)
            {
                throw new InvalidOperationException("Must execute call before parsing response");
            }

            try
            {
                HttpResponseBody = await HttpResponseMessage.Content.ReadAsStringAsync(_cancellationToken);
                _logger?.Log($"HTTPResponseBody: {HttpResponseBody}");
                return parser.ParseResponse(this);
            }
            catch (RestApiException)
            {
                throw;
            }
            catch (Exception e)
            {
                throw new UnexpectedRestApiResponseException(this, $"Could not process response", e);
            }
        }

        public async Task<string> GetResponseText()
        {
            HttpResponseBody = await HttpResponseMessage.Content.ReadAsStringAsync(_cancellationToken);
            _logger?.Log($"HTTPResponseBody: {HttpResponseBody}");
            return HttpResponseBody;
        }

        public void AssertSuccessfulStatusCode(params HttpStatusCode[] codesToAllow)
        {
            if (!HttpResponseMessage.IsSuccessStatusCode
                && (codesToAllow == null
                    || !codesToAllow.Contains(HttpResponseMessage.StatusCode)))
            {
                throw new UnexpectedRestApiResponseException(this, $"HttpStatusCode indicates an unsuccessful response");
            }
        }
    }
}