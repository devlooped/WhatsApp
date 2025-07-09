namespace Devlooped.WhatsApp;

/// <summary>
/// Represents a typing status update that can be sent in response to a user message.
/// </summary>
/// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/typing-indicators"/>
public record TypingResponse(string ServiceId, string UserNumber, string Context) : Response(ServiceId, UserNumber, Context)
{
    readonly CompositeService? service;

    internal TypingResponse(Service service, string userNumber, string context)
        : this(service.Id, userNumber, context)
        => this.service = service as CompositeService;

    protected override async Task<string?> SendCoreAsync(IWhatsAppClient client, CancellationToken cancellation = default)
    {
        if (service != null)
            await client.SendTyping(service.Secondary.Id, Context!, cancellation);

        await client.SendTyping(ServiceId, Context!, cancellation);

        // These types of messages don't actually have an ID.
        return Ulid.NewUlid().ToString();
    }
}
