
namespace Devlooped.WhatsApp;

/// <summary>
/// Represents a typing status update that can be sent in response to a user message.
/// </summary>
/// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/typing-indicators"/>
public record TypingResponse(string ServiceId, string UserNumber, string Context, string? ConversationId) : Response(ServiceId, UserNumber, Context, ConversationId)
{
    protected override async Task<string?> SendCoreAsync(IWhatsAppClient client, CancellationToken cancellation = default)
    {
        await client.SendTyping(ServiceId, Context, cancellation);
        // These types of messages don't actually have an ID.
        return Ulid.NewUlid().ToString();
    }
}
