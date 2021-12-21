namespace Zirpl.FluentRestClient
{
    public interface IHttpResponseProcessor<out TResponse> : IUnsuccessfulHttpStatusCodeHandler
    {
        TResponse ProcessResponse(IHttpCallContext context);
    }
}