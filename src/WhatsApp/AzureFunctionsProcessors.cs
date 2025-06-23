using Azure.Messaging.EventGrid;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;

namespace Devlooped.WhatsApp;

class AzureFunctionsProcessors(PipelineRunner runner)
{
    [Function("whatsapp_dequeue")]
    public Task DequeueAsync([QueueTrigger("whatsappwebhook", Connection = "AzureWebJobsStorage")] string json)
        => runner.ProcessAsync(json);

    [Function("whatsapp_eventgrid")]
    public async Task<IActionResult> HandleEventGrid(
#if CI || RELEASE
        [EventGridTrigger] EventGridEvent e)
#else
        [HttpTrigger(AuthorizationLevel.Anonymous, "post")]
        [Microsoft.Azure.Functions.Worker.Http.FromBody] EventGridEvent e)
#endif
    {
        await runner.ProcessAsync(e.Data.ToString());
        return new OkResult();
    }
}
