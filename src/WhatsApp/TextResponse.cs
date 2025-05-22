namespace Devlooped.WhatsApp;

/// <summary>
/// A simple text response to a user message.
/// </summary>
/// <param name="Message">The message this reaction applies to.</param>
/// <param name="Text">The text of the response.</param>
public record TextResponse(Message Message, string Text, Button? Button1 = default, Button? Button2 = default) : Response
{
    /// <inheritdoc/>
    internal override Task SendAsync(IWhatsAppClient client, CancellationToken cancellationToken = default)
    {
        if (Button1 != null)
        {
            return Button2 == null ?
                client.ReplyAsync(Message, Text, Button1) :
                client.ReplyAsync(Message, Text, Button1, Button2);

        }

        return client.ReplyAsync(Message, Text);
    }
}