namespace Devlooped.WhatsApp;

/// <summary>
/// Represents a response that sends a pre-defined template message to a recipient via a specified service.
/// </summary>
/// <remarks>This response is used to send a template message to a recipient's number using the specified service.
/// The template is identified by its name and code. The <see cref="SendCoreAsync"/> method handles the actual sending
/// of the template message.</remarks>
/// <param name="Number">The phone number of the recipient in international format.</param>
/// <param name="Service">The identifier of the service handling the message.</param>
/// <param name="Context">The unique identifier of the message to which the reaction is being sent.</param>
/// <param name="Name">The template name</param>
/// <param name="Language">The template language code (i.e. 'es_AR')</param>
/// <see cref="https://developers.facebook.com/docs/whatsapp/api/messages/message-templates#supported-languages"/>
/// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages/#template-object"/>
public record TemplateResponse(string Number, string Service, string Context, string? ConversationId, object Template) : Response(Number, Service, Context, ConversationId)
{
    public TemplateResponse(string Number, string Service, string Context, string? ConversationId, string Name, string Language)
        : this(Number, Service, Context, ConversationId, new { name = Name, language = new { code = Language } })
    {
    }

    /// <inheritdoc/>
    protected override async Task<string?> SendCoreAsync(IWhatsAppClient client, CancellationToken cancellationToken = default)
    {
        await client.SendTemplateAsync(ServiceId, Number, Template, cancellationToken);

        return Ulid.NewUlid().ToString();
    }
}