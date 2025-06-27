namespace Devlooped.WhatsApp;

/// <summary>
/// Represents a response that sends a reaction (emoji) to a specific message in a conversation.    
/// </summary>
/// <remarks>This response is used to react to a message by sending an emoji. The reaction is associated with a
/// specific  message identified by the <see cref="Context"/> property in the context of a conversation.</remarks>
/// <param name="ServiceId">The identifier of the service handling the message.</param>
/// <param name="UserNumber">The phone number of the recipient in international format.</param>
/// <param name="Context">The unique identifier of the message to which the reaction is being sent.</param>
/// <param name="Emoji">The emoji representing the reaction to the message.</param>
public record ReactionResponse(string ServiceId, string UserNumber, string Context, string? ConversationId, string Emoji) : Response(ServiceId, UserNumber, Context, ConversationId)
{
    // If this variable is not null, it means the originating message came from WhatsApp
    // and there's an ongoing conversation with the CLI simultaneously.
    readonly CompositeService? service;

    internal ReactionResponse(Service service, string userNumber, string context, string? conversationId, string emoji)
        : this(service.Id, userNumber, context, conversationId, emoji)
        => this.service = service as CompositeService;

    /// <inheritdoc/>
    protected override async Task<string?> SendCoreAsync(IWhatsAppClient client, CancellationToken cancellationToken = default)
    {
        if (service != null)
            await client.ReactAsync(service.Secondary.Id, UserNumber, Context, this.ConsoleText ?? Emoji, cancellationToken);

        // If service is null, it's either a WhatsApp regular without CLI, or it's pure CLI.
        // In the former case, we don't want to send messages that are CLI-only if the service id 
        // is not actually a CLI service.
        if (this.ConsoleOnly == true && !ServiceId.IsCLI())
            return null;

        // It may not be CLI-only but still provide a CLI-enhanced text.
        await client.ReactAsync(ServiceId, UserNumber, Context,
            // Automatically pick the CLI version of the text if sending to the CLI
            ServiceId.IsCLI() ? this.ConsoleText ?? Emoji : Emoji);

        return Ulid.NewUlid().ToString();
    }
}