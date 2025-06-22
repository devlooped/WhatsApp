using System.Text;
using System.Text.Json;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace Devlooped.WhatsApp;

/// <summary>
/// Azure functions used in development environments to allow the WhatsApp CLI to connect 
/// and exercise the WhatsApp API without requiring a full WhatsApp for Business account.
/// </summary>
public class AzureFunctionsConsole(
    IWhatsAppHandler handler,
    ILogger<AzureFunctions> logger,
    IHostEnvironment environment)
{
    [Function("whatsapp_console")]
    public async Task<IActionResult> MessageConsole([HttpTrigger(AuthorizationLevel.Anonymous, ["post", "get"], Route = "whatsappcli")] HttpRequest req)
    {
        // This endpoint is only available in development environments, since it allows sending messages from the debug console.
        if (environment.IsProduction())
            return new UnauthorizedResult();

        if (req.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
        {
            // Return a simple HTML page so we can verify from the console that the service endpoint URL is reachable
            return new ContentResult
            {
                ContentType = "text/html",
                StatusCode = 200,
                Content =
                    """
                    <html>
                    <body>
                    <h1>WhatsApp CLI Console</h1>
                    <p>Use the <a href="http://nuget.org/packages/dotnet-whatsapp">dotnet-whatsapp</a> client to send messages.</p>
                    </body>
                    </html>
                    """,
            };
        }

        using var reader = new StreamReader(req.Body, Encoding.UTF8);
        var json = await reader.ReadToEndAsync();
        logger.LogDebug("Received WhatsApp message: {Message}.", json);

        // Try to deserialize the message sent by the console
        if (JsonSerializer.Deserialize(json, JsonContext.Default.Message) is Message message)
        {
            message.FromConsole = true;
            // Await all responses
            // No action needed, just make sure all items are processed
            await handler.HandleAsync([message]).ToArrayAsync();
        }
        else
        {
            logger.LogWarning("Unsupported message type received: \r\n{Payload}", json);
        }

        return new OkResult();
    }
}
