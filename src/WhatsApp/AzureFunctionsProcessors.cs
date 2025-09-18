using System.Text;
using System.Text.RegularExpressions;
using Azure.Messaging.EventGrid;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Options;

namespace Devlooped.WhatsApp;

class AzureFunctionsProcessors(PipelineRunner runner, IOptions<WhatsAppOptions> options)
{
    readonly WhatsAppOptions options = options.Value;

    [Function("whatsapp_dequeue")]
    public Task DequeueAsync([QueueTrigger("whatsappwebhook", Connection = "AzureWebJobsStorage")] string json)
        => runner.ProcessAsync(json);

    [Function("whatsapp_eventgrid")]
    public async Task<IActionResult> HandleEventGrid(
#if CI || RELEASE
        [EventGridTrigger] EventGridEvent e)
#else
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "whatsapp/eventgrid")]
        [Microsoft.Azure.Functions.Worker.Http.FromBody] EventGridEvent e)
#endif
    {
        await runner.ProcessAsync(Regex.Unescape(e.Data.ToString()).Trim('"'));
        return new OkResult();
    }

    [Function("whatsapp_process")]
    public async Task<IActionResult> ProcessAsync(
        [HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "whatsapp/process")] HttpRequest req)
    {
        if (string.IsNullOrEmpty(options.Secret) ||
            !req.Headers.TryGetValue("X-WHATSAPP-SECRET", out var values) ||
            !options.Secret.Equals(values.ToString(), StringComparison.Ordinal))
            return new UnauthorizedResult();

        using var reader = new StreamReader(req.Body, Encoding.UTF8);
        var json = await reader.ReadToEndAsync();

        await runner.ProcessAsync(json);
        return new OkResult();
    }
}
