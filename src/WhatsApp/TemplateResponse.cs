namespace Devlooped.WhatsApp;

/// <summary>
/// Represents a response that sends a pre-defined template message to a recipient via a specified service.
/// </summary>
/// <remarks>This response is used to send a template message to a recipient's number using the specified service.
/// The template is identified by its name and code. The <see cref="SendCoreAsync"/> method handles the actual sending
/// of the template message.</remarks>
/// <param name="Number">The phone number of the recipient in international format.</param>
/// <param name="ServiceId">The identifier of the service handling the message.</param>
/// <param name="Context">The unique identifier of the message to which the reaction is being sent.</param>
/// <param name="Name">The template name</param>
/// <param name="Code">The template lang code</param>
public record TemplateResponse(string Number, string Service, string Context, string? ConversationId, string Name, string Code) : Response(Number, Service, Context, ConversationId)
{
    /// <inheritdoc/>
    protected override async Task<string?> SendCoreAsync(IWhatsAppClient client, CancellationToken cancellationToken = default)
    {
        await client.SendTemplateAsync(Service, Number, Name, Code, cancellationToken);

        return Ulid.NewUlid().ToString();
    }
}