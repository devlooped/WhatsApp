using System.Diagnostics;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Devlooped.WhatsApp.Flows;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.Primitives;

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
    IMessageProcessor messageProcessor,
    IWhatsAppClient whatsapp,
    Func<IWhatsAppHandler> handler,
    IOptions<MetaOptions> metaOptions,
    IOptions<WhatsAppOptions> functionOptions,
    IHostEnvironment hosting,
    ILogger<ServiceHandler> logger)
{
    static readonly JsonSerializerOptions options = new(JsonSerializerDefaults.Web)
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    readonly WhatsAppOptions functionOptions = functionOptions.Value;

    [Function("whatsapp_message")]
    public async Task<IActionResult> Message([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "whatsapp")] HttpRequest req)
        => await MessageCoreAsync(req, accountId: null);

    [Function("whatsapp_message_account")]
    public async Task<IActionResult> MessageWithAccount([HttpTrigger(AuthorizationLevel.Anonymous, "post", Route = "whatsapp/{accountId}")] HttpRequest req, string accountId)
        => await MessageCoreAsync(req, accountId);

    async Task<IActionResult> MessageCoreAsync(HttpRequest req, string? accountId)
    {
        using var reader = new StreamReader(req.Body, Encoding.UTF8);
        var json = await reader.ReadToEndAsync();

        logger.LogDebug("Received WhatsApp message: {Message}.",
            hosting.IsProduction() ? json :
            JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(json), options));

        // Detect encrypted flow request setup for flows endpoints
        if (JsonSerializer.Deserialize<EncryptedFlowData>(json) is { Data.Length: > 0, IV.Length: > 0, Key.Length: > 0 } encrypted)
        {
            return await ProcessFlowDataAsync(json, encrypted, accountId);
        }

        if (await WhatsApp.Message.DeserializeAsync(json) is { } message)
        {
            // If we got a user message, we can send progress updates as configured. We ignore exceptions in the 
            // operation since it can be a notification for an old message or it may have been deleted by the user.
            if (message is UserMessage user)
                await user.SendProgress(whatsapp, functionOptions.ReadOnMessage is true, functionOptions.TypingOnMessage is true).Ignore();

            if (functionOptions.ReactOnMessage != null && message.Type == MessageType.Content)
                await message.React(functionOptions.ReactOnMessage).SendAsync(whatsapp).Ignore();

            await messageProcessor.EnqueueAsync(json);
        }
        else
        {
            logger.LogWarning("Unsupported message type received: \r\n{Payload}", json);
        }

        return new OkResult();
    }


    async Task<IActionResult> ProcessFlowDataAsync(string json, EncryptedFlowData encrypted, string? accountId)
    {
        FlowCryptography? crypto = null;
        FlowData? data = null;

        if (accountId is not null)
        {
            var key = metaOptions.Value.GetPrivateKey(accountId);
            if (key is null)
                return new StatusCodeResult(421);

            crypto = new FlowCryptography(key);
            if (!crypto.TryDecrypt(encrypted, out data) || data is null)
                return new StatusCodeResult(421);
        }
        else
        {
            foreach (var key in metaOptions.Value.GetPrivateKeys())
            {
                var candidate = new FlowCryptography(key);
                if (candidate.TryDecrypt(encrypted, out data) && data is not null)
                {
                    crypto = candidate;
                    break;
                }
                candidate.Dispose();
            }

            if (crypto is null || data is null)
                return new StatusCodeResult(421);
        }

        if (data.Data.TryGetProperty("action", out var action) &&
            action.ValueKind == JsonValueKind.String &&
            action.GetString() == "ping")
        {
            // This satisfies the flow publishing requirement that the endpoint is active.
            return new OkObjectResult(crypto.Encrypt(data.With(
                new { data = new { status = "active" } })));
        }

        if (!data.Data.TryGetProperty("flow_token", out var t) ||
            t.ValueKind != JsonValueKind.String || t.GetString() is not { Length: > 0 } value ||
            !FlowToken.TryDecode(value, out var token))
        {
            logger.LogWarning("Received flow request without a valid flow_token.");
            return new BadRequestObjectResult("Missing or invalid flow_token.");
        }

        var node = JsonObject.Create(data.Data);
        Debug.Assert(node != null, "Node should not be null after decryption.");

        node.Add("service", token.ServiceId);
        node.Add("user", token.UserId);

        var flow = JsonSerializer.Deserialize<FlowDataRequest>(node, JsonContext.DefaultOptions);
        if (flow?.Flow is null)
        {
            logger.LogWarning("Failed to deserialize flow message from: {Json}", json);
            return new BadRequestObjectResult("Invalid flow message format.");
        }

        FlowDataResponse? flowResponse = default;

        await foreach (var response in handler().HandleAsync([flow]))
        {
            if (response is FlowDataResponse fdr)
            {
                if (flowResponse is not null)
                {
                    logger.LogWarning("At most one flow data response can be provided for {Token}", token.RawToken);
                    return new ConflictObjectResult("Multiple flow data responses are not allowed.");
                }
                else
                {
                    flowResponse = fdr;
                }
            }
        }

        if (flowResponse is null)
        {
            logger.LogWarning("No flow data response provided for {Token}", token.RawToken);
            return new NotFoundObjectResult("No flow data response provided.");
        }

        return new OkObjectResult(crypto.Encrypt(data.With(
            new
            {
                screen = flowResponse.Screen,
                data = flowResponse.Data
            })));
    }

    [Function("whatsapp_register")]
    public IActionResult Register([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "whatsapp")] HttpRequest req)
        => RegisterCore(req);

    [Function("whatsapp_register_account")]
    public IActionResult RegisterWithAccount([HttpTrigger(AuthorizationLevel.Anonymous, "get", Route = "whatsapp/{accountId}")] HttpRequest req, string accountId)
        => RegisterCore(req, accountId);

    IActionResult RegisterCore(HttpRequest req, string? accountId = null)
    {
        StringValues token = default;
        if (req.Query.TryGetValue("hub.mode", out var mode) && mode == "subscribe" &&
            req.Query.TryGetValue("hub.verify_token", out token) &&
            token.ToString() is { Length: > 0 } receivedToken &&
            req.Query.TryGetValue("hub.challenge", out var values) &&
            values.ToString() is { } challenge)
        {
            var valid = accountId is not null
                ? metaOptions.Value.GetVerifyToken(accountId) == receivedToken
                : metaOptions.Value.FindAccountByVerifyToken(receivedToken) is not null;

            if (valid)
            {
                logger.LogInformation("Registering webhook callback.");
                return new ContentResult
                {
                    Content = challenge,
                    ContentType = "text/plain",
                    StatusCode = 200
                };
            }
        }

        logger.LogError("Received token {ACTUAL} but no matching account verify token found.", token.ToString());
        return new BadRequestObjectResult("Received verification token doesn't match any configured account.");
    }
}
