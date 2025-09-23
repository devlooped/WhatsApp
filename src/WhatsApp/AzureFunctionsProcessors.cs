using System.Text;
using System.Text.RegularExpressions;
using Azure.Messaging.EventGrid;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;

namespace Devlooped.WhatsApp;

class AzureFunctionsProcessors(Func<PipelineRunner> runner, IOptions<WhatsAppOptions> options)
{
    readonly WhatsAppOptions options = options.Value;

    [Function("whatsapp_dequeue")]
    public Task DequeueAsync([QueueTrigger("whatsappwebhook", Connection = "AzureWebJobsStorage")] string json)
        => runner().ProcessAsync(json);

#if CI || RELEASE
    [Function("whatsapp_eventgrid")]
    public async Task<IActionResult> HandleEventGrid(
        [EventGridTrigger] EventGridEvent e)
    {
        await runner().ProcessAsync(Regex.Unescape(e.Data.ToString()).Trim('"'));
        return new OkResult();
    }
#else
    [Function("whatsapp_eventgrid")]
    public async Task<IActionResult> HandleEventGrid(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "whatsapp/eventgrid")] HttpRequest request)
    {
        using var sr = new StreamReader(request.Body);
        var json = await sr.ReadToEndAsync();
        var events = System.Text.Json.JsonSerializer.Deserialize<System.Text.Json.Nodes.JsonObject[]>(json);

        // Validation handshake?
        if (request.Headers.TryGetValue("aeg-event-type", out var aeg) && aeg.ToString() == "SubscriptionValidation" &&
            events?[0]?["data"]?["validationCode"]?.ToString() is string code)
        {
            return new OkObjectResult(new { validationResponse = code }); // 200 with the code
        }

        // Normal events here...
        var data = System.Text.Json.JsonSerializer.Deserialize<EventGridEvent[]>(json);
        if (data == null)
            return new OkResult();

        foreach (var item in data)
        {
            await runner().ProcessAsync(Regex.Unescape(item.Data.ToString()).Trim('"'));
        }

        return new OkResult();
    }
#endif

    [Function("whatsapp_process")]
    public async Task<IActionResult> ProcessAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "whatsapp/process")] HttpRequest req)
    {
        if (string.IsNullOrEmpty(options.ProcessSecret) ||
            !req.Headers.TryGetValue("X-WHATSAPP-SECRET", out var values) ||
            !options.ProcessSecret.Equals(values.ToString(), StringComparison.Ordinal))
            return new UnauthorizedResult();

        using var reader = new StreamReader(req.Body, Encoding.UTF8);
        var json = await reader.ReadToEndAsync();

        await runner().ProcessAsync(json);
        return new OkResult();
    }
}
