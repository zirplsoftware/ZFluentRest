# ZFluentRestClient

```csharp
public class MyRestApiClient : RestApiClientBase
{
  protected override HttpClient CreateHttpClient()
  {
    return new HttpClient("https://restapiexample.com/");
  }
  
  public NewOrderResponse CreateExample(NewOrder order)
  {
    // put to /orders
    return CreateCallContext()
      .WithUrlSegment("orders")
      .WithUrlParameter("recurring", false)
      .WithJsonRequestContent(order)
      .PutAsync()
      .AssertSuccessfulStatusCode()
      .ParseJsonResponse<NewOrderResponse>();
  }
}
```
