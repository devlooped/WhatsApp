namespace Devlooped.WhatsApp;

/// <summary>
/// A template response to a user message.
/// </summary>
/// <param name="Message">The message this reaction applies to.</param>
public record TemplateResponse(Message Message, string Name, string Code) : Response
{
    /// <inheritdoc/>
    internal override Task SendAsync(IWhatsAppClient client, CancellationToken cancellationToken = default)
        => client.SendTemplateAsync(Message.To.Id, Message.From.Number, Name, Code, cancellationToken);
}