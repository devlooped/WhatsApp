using System.ComponentModel;
using System.Diagnostics;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.RegularExpressions;
using Azure.Messaging.EventGrid;
using Devlooped.WhatsApp;
using Devlooped.WhatsApp.Flows;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Microsoft.AspNetCore.Builder;

/// <summary>
/// Provides extension methods for mapping WhatsApp webhook endpoints in ASP.NET Core.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WhatsAppEndpointRouteBuilderExtensions
{
    static readonly JsonSerializerOptions jsonOptions = new(JsonSerializerDefaults.Web)
    {
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
        WriteIndented = true
    };

    /// <summary>
    /// Maps WhatsApp webhook endpoints to the application.
    /// </summary>
    /// <remarks>
    /// The following endpoints are mapped:
    /// <list type="bullet">
    /// <item><description>POST /whatsapp - Main webhook endpoint for receiving messages</description></item>
    /// <item><description>GET /whatsapp - Webhook verification endpoint</description></item>
    /// <item><description>POST /whatsapp/process - Direct message processing endpoint (requires X-WHATSAPP-SECRET header)</description></item>
    /// <item><description>POST /whatsapp/eventgrid - Azure Event Grid event processing endpoint</description></item>
    /// <item><description>POST/GET /whatsapp/cli - Development console endpoint</description></item>
    /// </list>
    /// </remarks>
    /// <param name="endpoints">The endpoint route builder.</param>
    /// <returns>The endpoint route builder for chaining.</returns>
    public static IEndpointRouteBuilder UseWhatsApp(this IEndpointRouteBuilder endpoints)
    {
        Throw.IfNull(endpoints);

        // POST /whatsapp - Main webhook endpoint for receiving messages
        endpoints.MapPost("/whatsapp", HandleMessageAsync);

        // GET /whatsapp - Webhook verification endpoint
        endpoints.MapGet("/whatsapp", HandleRegisterAsync);

        // POST /whatsapp/process - Direct message processing endpoint
        endpoints.MapPost("/whatsapp/process", HandleProcessAsync);

        // POST /whatsapp/eventgrid - Azure Event Grid event processing endpoint
        endpoints.MapPost("/whatsapp/eventgrid", HandleEventGridAsync);

        // POST/GET /whatsapp/cli - Development console endpoint
        endpoints.MapMethods("/whatsapp/cli", ["POST", "GET"], HandleConsoleAsync);

        // Legacy console endpoint redirect
        endpoints.MapMethods("/whatsappcli", ["POST", "GET"], (HttpRequest request) =>
        {
            var newPath = request.Path.Value?.Replace("whatsappcli", "whatsapp/cli") ?? "/whatsapp/cli";
            return Results.Redirect(newPath, true, true);
        });

        return endpoints;
    }

    static async Task<IResult> HandleMessageAsync(
        [FromBody] JsonElement body,
        IMessageProcessor messageProcessor,
        IWhatsAppClient whatsapp,
        IOptions<MetaOptions> metaOptions,
        IOptions<WhatsAppOptions> whatsappOptions,
        IHostEnvironment hosting,
        Func<IWhatsAppHandler> handlerFactory,
        ILogger<WhatsAppEndpoints> logger)
    {
        var json = body.GetRawText();

        logger.LogDebug("Received WhatsApp message: {Message}.",
            hosting.IsProduction() ? json :
            JsonSerializer.Serialize(body, jsonOptions));

        // Detect encrypted flow request setup for flows endpoints
        if (JsonSerializer.Deserialize<EncryptedFlowData>(json) is { Data.Length: > 0, IV.Length: > 0, Key.Length: > 0 } encrypted)
        {
            return await ProcessFlowDataAsync(json, encrypted, metaOptions.Value, handlerFactory(), logger);
        }

        if (await Message.DeserializeAsync(json) is { } message)
        {
            // If we got a user message, we can send progress updates as configured. We ignore exceptions in the 
            // operation since it can be a notification for an old message or it may have been deleted by the user.
            if (message is UserMessage user)
                await user.SendProgress(whatsapp, whatsappOptions.Value.ReadOnMessage is true, whatsappOptions.Value.TypingOnMessage is true).Ignore();

            if (whatsappOptions.Value.ReactOnMessage != null && message.Type == MessageType.Content)
                await message.React(whatsappOptions.Value.ReactOnMessage).SendAsync(whatsapp).Ignore();

            await messageProcessor.EnqueueAsync(json);
        }
        else
        {
            logger.LogWarning("Unsupported message type received: \r\n{Payload}", json);
        }

        return Results.Ok();
    }

    static async Task<IResult> ProcessFlowDataAsync(string json, EncryptedFlowData encrypted, MetaOptions metaOptions, IWhatsAppHandler handler, ILogger logger)
    {
        if (string.IsNullOrEmpty(metaOptions.PrivateKey))
            return Results.StatusCode(421);

        var crypto = new FlowCryptography(metaOptions.PrivateKey);
        if (!crypto.TryDecrypt(encrypted, out var data) || data is null)
            return Results.StatusCode(421);

        if (data.Data.TryGetProperty("action", out var action) &&
            action.ValueKind == JsonValueKind.String &&
            action.GetString() == "ping")
        {
            // This satisfies the flow publishing requirement that the endpoint is active.
            return Results.Ok(crypto.Encrypt(data.With(
                new { data = new { status = "active" } })));
        }

        if (!data.Data.TryGetProperty("flow_token", out var t) ||
            t.ValueKind != JsonValueKind.String || t.GetString() is not { Length: > 0 } value ||
            !FlowToken.TryDecode(value, out var token))
        {
            logger.LogWarning("Received flow request without a valid flow_token.");
            return Results.BadRequest("Missing or invalid flow_token.");
        }

        var node = JsonObject.Create(data.Data);
        Debug.Assert(node != null, "Node should not be null after decryption.");

        node.Add("service", token.ServiceId);
        node.Add("user", token.UserNumber);

        var flow = JsonSerializer.Deserialize<FlowDataRequest>(node, JsonContext.DefaultOptions);
        if (flow?.Flow is null)
        {
            logger.LogWarning("Failed to deserialize flow message from: {Json}", json);
            return Results.BadRequest("Invalid flow message format.");
        }

        FlowDataResponse? flowResponse = default;

        await foreach (var response in handler.HandleAsync([flow]))
        {
            if (response is FlowDataResponse fdr)
            {
                if (flowResponse is not null)
                {
                    logger.LogWarning("At most one flow data response can be provided for {Token}", token.RawToken);
                    return Results.Conflict("Multiple flow data responses are not allowed.");
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
            return Results.NotFound("No flow data response provided.");
        }

        return Results.Ok(crypto.Encrypt(data.With(
            new
            {
                screen = flowResponse.Screen,
                data = flowResponse.Data
            })));
    }

    static IResult HandleRegisterAsync(
        [FromQuery(Name = "hub.mode")] string? mode,
        [FromQuery(Name = "hub.verify_token")] string? verifyToken,
        [FromQuery(Name = "hub.challenge")] string? challenge,
        IOptions<MetaOptions> metaOptions,
        ILogger<WhatsAppEndpoints> logger)
    {
        if (mode == "subscribe" && verifyToken == metaOptions.Value.VerifyToken && challenge is { Length: > 0 })
        {
            logger.LogInformation("Registering webhook callback.");
            return Results.Ok(challenge);
        }

        logger.LogError("Received token {ACTUAL} but expected {EXPECTED}.", verifyToken, metaOptions.Value.VerifyToken);
        return Results.BadRequest("Received verification token doesn't match the configured one.");
    }

    static async Task<IResult> HandleProcessAsync(
        [FromBody] JsonElement body,
        [FromHeader(Name = "X-WHATSAPP-SECRET")] string? secret,
        IOptions<WhatsAppOptions> options,
        Func<PipelineRunner> runnerFactory)
    {
        if (string.IsNullOrEmpty(options.Value.ProcessSecret) ||
            !options.Value.ProcessSecret.Equals(secret, StringComparison.Ordinal))
            return Results.Unauthorized();

        var json = body.GetRawText();
        await runnerFactory().ProcessAsync(json);
        return Results.Ok();
    }

    static async Task<IResult> HandleEventGridAsync(
        HttpRequest request,
        [FromBody] JsonElement body,
        Func<PipelineRunner> runnerFactory)
    {
        var json = body.GetRawText();
        var events = JsonSerializer.Deserialize<JsonObject[]>(json);

        // Validation handshake?
        if (request.Headers.TryGetValue("aeg-event-type", out var aeg) && aeg.ToString() == "SubscriptionValidation" &&
            events?[0]?["data"]?["validationCode"]?.ToString() is string code)
        {
            return Results.Ok(new { validationResponse = code });
        }

        // Normal events here...
        var data = JsonSerializer.Deserialize<EventGridEvent[]>(json);
        if (data == null)
            return Results.Ok();

        foreach (var item in data)
        {
            await runnerFactory().ProcessAsync(Regex.Unescape(item.Data.ToString()).Trim('"'));
        }

        return Results.Ok();
    }

    static async Task<IResult> HandleConsoleAsync(
        HttpRequest request,
        IWhatsAppClient client,
        Func<IWhatsAppHandler> handlerFactory,
        IHostEnvironment environment,
        ILogger<WhatsAppEndpoints> logger)
    {
        // This endpoint is only available in development environments, since it allows sending messages from the debug console.
        if (environment.IsProduction())
            return Results.Unauthorized();

        if (request.Method.Equals("GET", StringComparison.OrdinalIgnoreCase))
        {
            // Return a simple HTML page so we can verify from the console that the service endpoint URL is reachable
            return Results.Content(
                """
                <html>
                <body>
                <h1>WhatsApp CLI Console</h1>
                <p>Use the <a href="http://nuget.org/packages/dotnet-whatsapp">dotnet-whatsapp</a> client to send messages.</p>
                </body>
                </html>
                """,
                "text/html");
        }

        using var reader = new StreamReader(request.Body);
        var json = await reader.ReadToEndAsync();
        logger.LogDebug("Received WhatsApp message: {Message}.",
            environment.IsProduction() ? json :
            JsonSerializer.Serialize(JsonSerializer.Deserialize<JsonElement>(json), jsonOptions));

        // Try to deserialize the message sent by the console
        if (JsonSerializer.Deserialize(json, JsonContext.Default.Message) is Message message)
        {
            if (message is UserMessage user)
                await user.SendProgress(client, true, true).Ignore();

            message = message.With(x => x["FromConsole"] = true);

            // Await all responses
            // No action needed, just make sure all items are processed
            _ = Task.Run(() => handlerFactory().HandleAsync([message]).ToArrayAsync().AsTask()).Ignore();
        }
        else
        {
            logger.LogWarning("Unsupported message type received: \r\n{Payload}", json);
        }

        return Results.Ok();
    }

    /// <summary>
    /// Marker class for logging from WhatsApp endpoints.
    /// </summary>
    class WhatsAppEndpoints { }
}
