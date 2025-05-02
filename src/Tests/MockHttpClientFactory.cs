namespace Devlooped.WhatsApp;

class MockHttpClientFactory : IHttpClientFactory
{
    public static IHttpClientFactory Default { get; } = new MockHttpClientFactory();

    public HttpClient CreateClient(string name) => new HttpClient();
}
