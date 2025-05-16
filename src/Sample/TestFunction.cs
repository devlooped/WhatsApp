using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Caching.Hybrid;

namespace Devlooped.WhatsApp;

public class TestFunction(HybridCache cache)
{
    [Function("test")]
    public async Task<IActionResult> RunAsync([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var value = await cache.GetOrCreateAsync("test", entry => ValueTask.FromResult(Guid.NewGuid().ToString()));

        return new OkObjectResult("Running: " + value);
    }
}