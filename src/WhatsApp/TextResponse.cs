namespace Devlooped.WhatsApp;

/// <summary>
/// Represents a response containing text and optional interactive buttons,  which can be sent as a reply to a message. 
/// </summary>
/// <remarks>This response type allows sending a text message with up to two optional buttons  for user
/// interaction. If no buttons are provided, the response will consist of  only the text message.</remarks>
/// <param name="Number">The phone number of the recipient in international format.</param>
/// <param name="ServiceId">The identifier of the service handling the message.</param>
/// <param name="Context">The unique identifier of the message to which the reaction is being sent.</param>
/// <param name="Text">The text content of the response message.</param>
/// <param name="Button1">An optional button to include in the response for user interaction.</param>
/// <param name="Button2">An optional second button to include in the response for user interaction.</param>
public record TextResponse(string Number, string Service, string Context, string? ConversationId, string Text, Button? Button1 = default, Button? Button2 = default) : Response(Number, Service, Context, ConversationId)
{
    /// <inheritdoc/>
    protected override Task<string?> SendCoreAsync(IWhatsAppClient client, CancellationToken cancellationToken = default)
    {
        if (Button1 != null)
        {
            return (Button2 == null ?
                client.ReplyAsync(Number, Service, Context, Text, Button1) :
                client.ReplyAsync(Number, Service, Context, Text, Button1, Button2));
        }
        else
        {
            return client.ReplyAsync(Number, Service, Context, Text);
        }
    }
}