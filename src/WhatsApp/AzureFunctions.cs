using System.Text;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
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
    [Function("whatsapp_message")]
    public async Task<IActionResult> Message([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "whatsapp")] HttpRequest req)
    {
        using var reader = new StreamReader(req.Body, Encoding.UTF8);
        var json = await reader.ReadToEndAsync();
        logger.LogDebug("Received WhatsApp message: {Message}.", json);

        if (await WhatsApp.Message.DeserializeAsync(json) is { } message)
        {
            // Ensure idempotent processing
            var table = tableClient.GetTableClient("whatsapp");
            await table.CreateIfNotExistsAsync();
            if (await table.GetEntityIfExistsAsync<TableEntity>(message.From.Number, message.NotificationId) is { HasValue: true } existing)
            {
                logger.LogInformation("Skipping already handled message {Id}", message.Id);
                return new OkResult();
            }

            // Otherwise, queue the new message
            var queue = queueClient.GetQueueClient("whatsapp");
            await queue.CreateIfNotExistsAsync();
            await queue.SendMessageAsync(json);

            // Mark read these two types of messages we want to explicitly acknowledge from users.
            if (message.Type == MessageType.Content ||
                message.Type == MessageType.Interactive)
            {
                try
                {
                    // Best-effort to mark as read. This might be an old message callback, 
                    // or the message might have been deleted.
                    await whatsapp.MarkReadAsync(message.To.Id, message.Id);
                }
                catch (HttpRequestException e)
                {
                    logger.LogWarning("Failed to mark message as read: {Id}\r\n{Payload}", message.Id, e.Message);
                }
            }
        }
        else
        {
            logger.LogWarning("Unsupported message type received: \r\n{Payload}", json);
        }

        return new OkResult();
    }

    [Function("whatsapp_process")]
    public async Task Process([QueueTrigger("whatsapp", Connection = "AzureWebJobsStorage")] string json)
    {
        logger.LogDebug("Processing WhatsApp message: {Message}", json);

        if (await WhatsApp.Message.DeserializeAsync(json) is { } message)
        {
            // Ensure idempotent processing at dequeue time, since we might have been called 
            // multiple times for the same message by WhatsApp (Message method) while processing was still 
            // happening (and therefore we didn't save the entity yet).
            var table = tableClient.GetTableClient("whatsapp");
            await table.CreateIfNotExistsAsync();
            if (await table.GetEntityIfExistsAsync<TableEntity>(message.From.Number, message.NotificationId) is { HasValue: true } existing)
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

    [Function("whatsapp_register")]
    public IActionResult Register([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "whatsapp")] HttpRequest req)
    {
        if (req.Query.TryGetValue("hub.mode", out var mode) && mode == "subscribe" &&
            req.Query.TryGetValue("hub.verify_token", out var token) && token == options.Value.VerifyToken &&
            req.Query.TryGetValue("hub.challenge", out var values) &&
            values.ToString() is { } challenge)
        {
            logger.LogInformation("Registering webhook callback.");

            return new OkObjectResult(challenge);
        }

        return new BadRequestObjectResult("Received verification token doesn't match the configured one.");
    }
}
