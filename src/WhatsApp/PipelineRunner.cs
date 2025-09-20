using Azure.Data.Tables;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Devlooped.WhatsApp;

class PipelineRunner(
    Idempotency idempotency,
    IWhatsAppClient whatsapp,
    Func<IWhatsAppHandler> handler,
    IOptions<WhatsAppOptions> functionOptions,
    ILogger<PipelineRunner> logger)
{
    readonly WhatsAppOptions functionOptions = functionOptions.Value;

    public async Task ProcessAsync(string json)
    {
        logger.LogDebug("Processing WhatsApp message: {Message}", json);

        if (await Message.DeserializeAsync(json) is { } message)
        {
            if (await idempotency.IsProcessedAsync(message, json))
            {
                logger.LogInformation("Skipping already handled message {Id}", message.Id);
                return;
            }

            // If we got a user message, we can send progress updates as configured. We ignore exceptions in the 
            // operation since it can be a notification for an old message or it may have been deleted by the user.
            if (message is UserMessage user)
                await user.SendProgress(whatsapp, functionOptions.ReadOnProcess is true, functionOptions.TypingOnProcess is true).Ignore();

            // Ensure idempotent processing at dequeue time, since we might have been called 
            // multiple times for the same message by WhatsApp (Message method) while processing was still 
            // happening (and therefore we didn't save the entity yet).
            logger.LogInformation("Processing work item: {Id}", message.Id);
            var etag = await idempotency.TrySetProcessedAsync(message, json);
            if (etag == null)
            {
                logger.LogInformation("Skipping already handled message {Id}", message.Id);
                return;
            }

            try
            {
                // Await all responses
                // No action needed, just make sure all items are processed
                await handler().HandleAsync([message]).ToArrayAsync();
                logger.LogInformation($"Completed work item: {message.Id}");
            }
            catch (Exception e)
            {
                logger.LogError(e, "Failed to process message {Id}", message.Id);
                await idempotency.ResetProcessedAsync(message, json, etag.Value);
            }
        }
        else
        {
            logger.LogWarning("Failed to deserialize message. {Message}", json);
        }
    }
}
