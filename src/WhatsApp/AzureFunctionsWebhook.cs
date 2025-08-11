using System.Text;
using System.Text.Json;
using Azure.Data.Tables;
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
class AzureFunctionsWebhook(
    TableServiceClient tableClient,
    IMessageProcessor messageProcessor,
    IWhatsAppClient whatsapp,
    IOptions<MetaOptions> metaOptions,
    IOptions<WhatsAppOptions> functionOptions,
    ILogger<AzureFunctionsWebhook> logger)
{
    readonly WhatsAppOptions functionOptions = functionOptions.Value;

    [Function("whatsapp_message")]
    public async Task<IActionResult> Message([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "whatsapp")] HttpRequest req)
    {
        using var reader = new StreamReader(req.Body, Encoding.UTF8);
        var json = await reader.ReadToEndAsync();
        logger.LogDebug("Received WhatsApp message: {Message}.", json);

        // Detect encrypted flow request setup for flows endpoints
        if (JsonSerializer.Deserialize<EncryptedFlowData>(json) is { Data.Length: > 0, IV.Length: > 0, Key.Length: > 0 } encrypted)
        {
            if (string.IsNullOrEmpty(metaOptions.Value.PrivateKey))
                return new StatusCodeResult(421);

            var crypto = new FlowCryptography(metaOptions.Value.PrivateKey);
            if (!crypto.TryDecrypt(encrypted, out var data) || data is null)
                return new StatusCodeResult(421);

            if (data.Data.TryGetProperty("action", out var action) &&
                action.ValueKind == JsonValueKind.String &&
                action.GetString() == "ping")
            {
                // This satisfies the flow publishing requirement that the endpoint is active.
                return new OkObjectResult(crypto.Encrypt(data.With(
                    new { data = new { status = "active" } })));
            }

            // TODO: else, how do we handle flow actions?
            return new OkObjectResult(crypto.Encrypt(data.With(
                new
                {
                    screen = "SUCCESS",
                    data = new
                    {
                        extension_message_response = new Dictionary<string, object>
                        {
                            ["params"] = new
                            {
                                flow_token = "unused"
                            }
                        }
                    }
                })));
        }

        if (await WhatsApp.Message.DeserializeAsync(json) is { } message)
        {
            // If we got a user message, we can send progress updates as configured. We ignore exceptions in the 
            // operation since it can be a notification for an old message or it may have been deleted by the user.
            if (message is UserMessage user)
                await user.SendProgress(whatsapp, functionOptions.ReadOnMessage is true, functionOptions.TypingOnMessage is true).Ignore();

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

            // Otherwise, enqueue the message processing
            await messageProcessor.EnqueueAsync(json);
        }
        else
        {
            logger.LogWarning("Unsupported message type received: \r\n{Payload}", json);
        }

        return new OkResult();
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
