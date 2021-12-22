namespace Zirpl.FluentRestClient
{
    public interface IHttpResponseParser<out TResponse>
    {
        TResponse ParseResponse(RestApiCallContext context);
    }
}