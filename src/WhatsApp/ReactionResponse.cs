namespace Devlooped.WhatsApp;

/// <summary>
/// A reaction response to a user message.
/// </summary>
/// <param name="UserMessage">The message this reaction applies to.</param>
/// <param name="Emoji">The emoji of the reaction.</param>
public record ReactionResponse(UserMessage UserMessage, string Emoji) : Response
{
    /// <inheritdoc/>
    internal override Task SendAsync(IWhatsAppClient client, CancellationToken cancellationToken = default)
        => client.ReactAsync(UserMessage, Emoji);
}