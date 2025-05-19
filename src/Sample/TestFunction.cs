using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace Devlooped.WhatsApp;

public class TestFunction
{
    [Function("test")]
    public static IActionResult Run([HttpTrigger(AuthorizationLevel.Anonymous, "get")] HttpRequest req)
    {
        var value = Guid.NewGuid().ToString();

        return new OkObjectResult("Running: " + value);
    }
}