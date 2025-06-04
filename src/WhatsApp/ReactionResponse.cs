namespace Devlooped.WhatsApp;

/// <summary>
/// Represents a response that sends a reaction (emoji) to a specific message in a conversation.    
/// </summary>
/// <remarks>This response is used to react to a message by sending an emoji. The reaction is associated with a
/// specific  message identified by the <see cref="Context"/> property in the context of a conversation.</remarks>
/// <param name="Number">The phone number of the recipient in international format.</param>
/// <param name="Service">The identifier of the service handling the message.</param>
/// <param name="Context">The unique identifier of the message to which the reaction is being sent.</param>
/// <param name="Emoji">The emoji representing the reaction to the message.</param>
public record ReactionResponse(string Number, string Service, string Context, string? ConversationId, string Emoji) : Response(Number, Service, Context, ConversationId)
{
    /// <inheritdoc/>
    protected override async Task<string?> SendCoreAsync(IWhatsAppClient client, CancellationToken cancellationToken = default)
    {
        await client.ReactAsync(Service, Number, Context, Emoji);

        return Ulid.NewUlid().ToString();
    }
}