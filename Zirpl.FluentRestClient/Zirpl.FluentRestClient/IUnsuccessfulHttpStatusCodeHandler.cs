namespace Zirpl.FluentRestClient
{
    public interface IUnsuccessfulHttpStatusCodeHandler
    {
        void OnUnsuccessfulHttpStatusCode(IHttpCallContext context);
    }
}