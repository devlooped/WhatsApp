namespace Devlooped.WhatsApp;

/// <summary>
/// Represents an interactive call to action that can be sent in response to a user message.
/// </summary>
/// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/messages/interactive-cta-url-messages/"/>
/// <param name="ServiceId">The identifier of the service handling the message.</param>
/// <param name="UserNumber">The phone number of the recipient in international format.</param>
/// <param name="Text">The content of the message calling to action.</param>
/// <param name="Action">The action button text.</param>
/// <param name="Url">The URL to navigate to when the action button is clicked.</param>
/// /// <param name="ConversationId">The conversation id where this response was generated</param>
public record CallToActionResponse(string ServiceId, string UserNumber, string Text, string Action, string Url, string? ConversationId) : Response(ServiceId, UserNumber, null, ConversationId)
{
    readonly CompositeService? service;

    internal CallToActionResponse(Service service, string userNumber, string Text, string Action, string Url, string? conversationId)
        : this(service.Id, userNumber, Text, Action, Url, conversationId)
        => this.service = service as CompositeService;

    protected override Task<string?> SendCoreAsync(IWhatsAppClient client, CancellationToken cancellation = default)
    {
        if (service != null)
            return client.CallToActionAsync(service.Secondary.Id, UserNumber, Text, Action, Url, cancellation);

        return client.CallToActionAsync(ServiceId, UserNumber, Text, Action, Url, cancellation);
    }
}