using System.Text;
using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
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
    IOptions<MetaOptions> metaOptions,
    IOptions<WhatsAppOptions> functionOptions,
    ILogger<AzureFunctions> logger,
    IHostEnvironment environment)
{
    readonly WhatsAppOptions functionOptions = functionOptions.Value;

    [Function("whatsapp_message")]
    public async Task<IActionResult> Message([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "whatsapp")] HttpRequest req)
    {
        using var reader = new StreamReader(req.Body, Encoding.UTF8);
        var json = await reader.ReadToEndAsync();
        logger.LogDebug("Received WhatsApp message: {Message}.", json);

        if (await WhatsApp.Message.DeserializeAsync(json) is { } message)
        {
            if (functionOptions.ReadOnMessage is true && message.Type == MessageType.Content)
                // Ignored since this can be an old, deleted message, for example
                await whatsapp.MarkReadAsync(message.Service.Id, message.Id).Ignore();

            // Ensure idempotent processing
            var table = tableClient.GetTableClient("WhatsAppWebhook");
            await table.CreateIfNotExistsAsync();
            if (await table.GetEntityIfExistsAsync<TableEntity>(message.User.Number, message.NotificationId) is { HasValue: true } existing)
            {
                logger.LogInformation("Skipping already handled message {Id}", message.Id);
                return new OkResult();
            }

            if (functionOptions.ReactOnMessage != null && message.Type == MessageType.Content)
                await message.React(functionOptions.ReactOnMessage).SendAsync(whatsapp).Ignore();

            // Otherwise, queue the new message
            var queue = queueClient.GetQueueClient("whatsappwebhook");
            await queue.CreateIfNotExistsAsync();
            await queue.SendMessageAsync(json);
        }
        else
        {
            logger.LogWarning("Unsupported message type received: \r\n{Payload}", json);
        }

        return new OkResult();
    }

    [Function("whatsapp_process")]
    public async Task Process([QueueTrigger("whatsappwebhook", Connection = "AzureWebJobsStorage")] string json)
    {
        logger.LogDebug("Processing WhatsApp message: {Message}", json);

        if (await WhatsApp.Message.DeserializeAsync(json) is { } message)
        {
            if (functionOptions.ReadOnProcess is true && message.Type == MessageType.Content)
                // Ignored since this can be an old, deleted message, for example
                await whatsapp.MarkReadAsync(message.Service.Id, message.Id).Ignore();

            // Ensure idempotent processing at dequeue time, since we might have been called 
            // multiple times for the same message by WhatsApp (Message method) while processing was still 
            // happening (and therefore we didn't save the entity yet).
            var table = tableClient.GetTableClient("WhatsAppWebhook");
            await table.CreateIfNotExistsAsync();
            if (await table.GetEntityIfExistsAsync<TableEntity>(message.User.Number, message.NotificationId) is { HasValue: true } existing)
            {
                logger.LogInformation("Skipping already handled message {Id}", message.Id);
                return;
            }

            // Await all responses
            // No action needed, just make sure all items are processed
            await handler.HandleAsync([message]).ToArrayAsync();

            await table.UpsertEntityAsync(new TableEntity(message.User.Number, message.Id));
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
            req.Query.TryGetValue("hub.verify_token", out var token) && token == metaOptions.Value.VerifyToken &&
            req.Query.TryGetValue("hub.challenge", out var values) &&
            values.ToString() is { } challenge)
        {
            logger.LogInformation("Registering webhook callback.");

            return new OkObjectResult(challenge);
        }

        return new BadRequestObjectResult("Received verification token doesn't match the configured one.");
    }
}
