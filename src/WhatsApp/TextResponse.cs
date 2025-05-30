namespace Devlooped.WhatsApp;

/// <summary>
/// Represents a response containing text and optional interactive buttons,  which can be sent as a reply to a message. 
/// </summary>
/// <remarks>This response type allows sending a text message with up to two optional buttons  for user
/// interaction. If no buttons are provided, the response will consist of  only the text message.</remarks>
/// <param name="Message">The message to which this response is a reply.</param>
/// <param name="Text">The text content of the response message.</param>
/// <param name="Button1">An optional button to include in the response for user interaction.</param>
/// <param name="Button2">An optional second button to include in the response for user interaction.</param>
public record TextResponse(Message Message, string Text, Button? Button1 = default, Button? Button2 = default) : Response(Message)
{
    /// <inheritdoc/>
    internal async override Task SendAsync(IWhatsAppClient client, CancellationToken cancellationToken = default)
    {
        if (Button1 != null)
        {
            Id = await (Button2 == null ?
                client.ReplyAsync(Message, Text, Button1) :
                client.ReplyAsync(Message, Text, Button1, Button2)) ?? string.Empty;
        }
        else
        {
            Id = await client.ReplyAsync(Message, Text) ?? string.Empty;
        }
    }
}