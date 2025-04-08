using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Devlooped.WhatsApp;

/// <summary>
/// Provides the integration with Azure Functions.
/// </summary>
/// <param name="queueClient">Queue used to process asynchronously the webhook callbacks from WhatsApp for Business.</param>
/// <param name="tableClient">Table used to store successfully processed messages for idempotency.</param>
/// <param name="whatsapp">The <see cref="IWhatsAppClient"/> client to signal message processing state.</param>
/// <param name="handler">The message handler that will process incoming messages.</param>
/// <param name="logger">The logger.</param>
public class AzureFunctions(
    QueueServiceClient queueClient,
    TableServiceClient tableClient,
    IWhatsAppClient whatsapp,
    IWhatsAppHandler handler,
    IOptions<MetaOptions> options,
    ILogger<AzureFunctions> logger)
{
    [Function("whatsapp_register")]
    public HttpResponseMessage Register([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "whatsapp")] HttpRequest req)
    {
        if (req.Query.TryGetValue("hub.mode", out var mode) && mode == "subscribe" &&
            req.Query.TryGetValue("hub.verify_token", out var token) && token == options.Value.VerifyToken &&
            req.Query.TryGetValue("hub.challenge", out var values) &&
            values.ToString() is { } challenge)
        {
            logger.LogInformation("Registering webhook callback.");
            return new HttpResponseMessage(System.Net.HttpStatusCode.OK)
            {
                Content = new StringContent(challenge, Encoding.UTF8, "text/plain")
            };
        }

        return new HttpResponseMessage(System.Net.HttpStatusCode.BadRequest)
        {
            Content = new StringContent("Received verification token doesn't match the configured one.", Encoding.UTF8, "text/plain")
        };
    }

    [Function("whatsapp_message")]
    public async Task<HttpResponseMessage> Message([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "whatsapp")] HttpRequest req)
    {
        using var reader = new StreamReader(req.Body, Encoding.UTF8);
        var json = await reader.ReadToEndAsync();
        logger.LogDebug("Received WhatsApp message: {Message}.", json);

        if (await WhatsApp.Message.DeserializeAsync(json) is { } message)
        {
            var queue = queueClient.GetQueueClient("whatsapp");
            await queue.CreateIfNotExistsAsync();
            await queue.SendMessageAsync(json);
            if (message.Type == MessageType.Content)
                await whatsapp.MarkReadAsync(message.To.Id, message.Id);
        }
        else
        {
            logger.LogWarning("Unsupported message type received: \r\n{Payload}", json);
        }

        return new HttpResponseMessage(System.Net.HttpStatusCode.OK);
    }

    [Function("whatsapp_process")]
    public async Task Process([QueueTrigger("whatsapp", Connection = "AzureWebJobsStorage")] string json)
    {
        logger.LogDebug("Processing WhatsApp message: {Message}", json);

        if (await WhatsApp.Message.DeserializeAsync(json) is { } message)
        {
            // Ensure idempotent processing
            var table = tableClient.GetTableClient("whatsapp");
            await table.CreateIfNotExistsAsync();
            if (await table.GetEntityIfExistsAsync<TableEntity>(message.From.Number, message.Id) is { HasValue: true } existing)
            {
                logger.LogInformation("Skipping already handled message {Id}", message.Id);
                return;
            }

            await handler.HandleAsync(message);
            await table.UpsertEntityAsync(new TableEntity(message.From.Number, message.Id));
            logger.LogInformation($"Completed work item: {message.Id}");
        }
        else
        {
            logger.LogWarning("Failed to deserialize message.");
        }
    }
}
