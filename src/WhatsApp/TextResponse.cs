namespace Devlooped.WhatsApp;

/// <summary>
/// Represents a response containing text and optional interactive buttons,  which can be sent as a reply to a message. 
/// </summary>
/// <remarks>This response type allows sending a text message with up to two optional buttons  for user
/// interaction. If no buttons are provided, the response will consist of  only the text message.</remarks>
/// <param name="ServiceId">The identifier of the service handling the message.</param>
/// <param name="UserNumber">The phone number of the recipient in international format.</param>
/// <param name="Context">The unique identifier of the message to which this response is a reply to .</param>
/// <param name="Text">The text content of the response message.</param>
/// <param name="Button1">An optional button to include in the response for user interaction.</param>
/// <param name="Button2">An optional second button to include in the response for user interaction.</param>
public record TextResponse(string ServiceId, string UserNumber, string Context, string? ConversationId, string Text, Button? Button1 = default, Button? Button2 = default) : Response(ServiceId, UserNumber, Context, ConversationId)
{
    // If this variable is not null, it means the originating message came from WhatsApp
    // and there's an ongoing conversation with the CLI simultaneously.
    readonly CompositeService? service;

    internal TextResponse(Service service, string userNumber, string context, string? conversationId, string text, Button? button1 = default, Button? button2 = default)
        : this(service.Id, userNumber, context, conversationId, text, button1, button2)
        => this.service = service as CompositeService;

    /// <inheritdoc/>
    protected override async Task<string?> SendCoreAsync(IWhatsAppClient client, CancellationToken cancellation = default)
    {
        string? id = null;
        if (service != null)
            id = await SendReplyAsync(client, service.Secondary.Id, this.ConsoleText ?? Text, cancellation);

        // If service is null, it's either a WhatsApp regular without CLI, or it's pure CLI.
        // In both cases, we need to send the reaction to the WhatsApp service. 
        // The other case is if we did get a composite service, but the message is not intended for console only.
        if (service == null || this.ConsoleOnly != true)
            return await SendReplyAsync(client, ServiceId,
                // Automatically pick the CLI version of the text if sending to the CLI
                ServiceId.IsCLI() ? this.ConsoleText ?? Text : Text, cancellation);

        return id;
    }

    Task<string?> SendReplyAsync(IWhatsAppClient client, string serviceId, string text, CancellationToken cancellation)
    {
        if (Button1 != null)
        {
            if (Button2 == null)
                return client.ReplyAsync(serviceId, UserNumber, Context, text, Button1, cancellation);
            else
                return client.ReplyAsync(serviceId, UserNumber, Context, text, Button1, Button2, cancellation);
        }
        else
        {
            return client.ReplyAsync(serviceId, UserNumber, Context, text, cancellation);
        }
    }
}