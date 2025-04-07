using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Devlooped.WhatsApp;

class MockHttpClientFactory : IHttpClientFactory
{
    public static IHttpClientFactory Default { get; } = new MockHttpClientFactory();

    public HttpClient CreateClient(string name) => new HttpClient();
}
