namespace Zirpl.FluentRestClient
{
    public abstract class RestApiClientBase : IDisposable
    {
        private HttpClient? _httpClient;

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Handles disposing logic
        /// </summary>
        /// <param name="disposing">True if invoked from public void Dispose()</param>
        protected virtual void Dispose(bool disposing)
        {
            try
            {
                if (disposing
                    && _httpClient != null)
                {
                    _httpClient.Dispose();
                }
            }
            catch
            {
                // ignored
            }
        }

        protected HttpClient? HttpClient
        {
            get
            {
                if (_httpClient == null)
                {
                    _httpClient = CreateHttpClient();
                }

                return _httpClient;
            }
        }

        protected abstract HttpClient? CreateHttpClient();

        protected virtual RestApiCallContext CreateCallContext()
        {
            return new RestApiCallContext(HttpClient);
        }
    }
}