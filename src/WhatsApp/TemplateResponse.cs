namespace Devlooped.WhatsApp;

/// <summary>
/// Represents a response that sends a pre-defined template message to a recipient via a specified service.
/// </summary>
/// <remarks>This response is used to send a template message to a recipient's number using the specified service.
/// The template is identified by its name and code. The <see cref="SendCoreAsync"/> method handles the actual sending
/// of the template message.</remarks>
/// <param name="ServiceId">The identifier of the service handling the message.</param>
/// <param name="UserId">The phone number of the recipient in international format.</param>
/// <param name="Context">The unique identifier of the message to which the reaction is being sent.</param>
/// <param name="Template">The message template, components and parameters.</param>
public record TemplateResponse(string ServiceId, string UserId, string Context, MessageTemplate Template) : Response(ServiceId, UserId, Context)
{
    public TemplateResponse(string ServiceId, string UserId, string Context, string Name, string Language)
        : this(UserId, ServiceId, Context, new MessageTemplate(Name, Language))
    {
    }

    /// <inheritdoc/>
    protected override async Task<string?> SendCoreAsync(IWhatsAppClient client, CancellationToken cancellationToken = default)
    {
        await client.SendTemplateAsync(ServiceId, UserId, Template, cancellationToken);

        return Ulid.NewUlid().ToString();
    }
}