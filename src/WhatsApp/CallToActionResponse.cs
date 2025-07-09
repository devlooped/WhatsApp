namespace Devlooped.WhatsApp;

/// <summary>
/// Represents an interactive call to action that can be sent in response to a user message.
/// </summary>
/// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/messages/interactive-cta-url-messages/"/>
public record CallToActionResponse(string ServiceId, string UserNumber, string? Context, string Text, string ButtonText, string Url, string? ConversationId) : Response(ServiceId, UserNumber, Context, ConversationId)
{
    readonly CompositeService? service;

    internal CallToActionResponse(Service service, string userNumber, string? context, string Text, string ButtonText, string Url, string? conversationId)
        : this(service.Id, userNumber, context, Text, ButtonText, Url, conversationId)
        => this.service = service as CompositeService;

    protected override Task<string?> SendCoreAsync(IWhatsAppClient client, CancellationToken cancellation = default)
    {
        if (service != null)
            return client.SendCallToActionAsync(service.Secondary.Id, UserNumber, Text, ButtonText, Url, cancellation);

        return client.SendCallToActionAsync(ServiceId, UserNumber, Text, ButtonText, Url, cancellation);
    }
}