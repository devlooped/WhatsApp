using static System.Net.Mime.MediaTypeNames;

namespace Devlooped.WhatsApp;

/// <summary>
/// Represents a response containing a template message to be sent via a WhatsApp client.
/// </summary>
/// <remarks>This response encapsulates the details required to send a template message, including the recipient,
/// sender, template name, and template code. It is used in conjunction with a WhatsApp client to facilitate the
/// delivery of template-based messages.</remarks>
/// <param name="Message">The message details, including sender and recipient information.</param>
/// <param name="Name">The name of the template to be sent. This must match a pre-configured template in the WhatsApp system.</param>
/// <param name="Code">The code associated with the template, used to identify the specific template version or configuration.</param>
public record TemplateResponse(Message Message, string Name, string Code) : Response(Message)
{
    /// <inheritdoc/>
    internal override Task SendAsync(IWhatsAppClient client, CancellationToken cancellationToken = default)
        => client.SendTemplateAsync(Message.To.Id, Message.From.Number, Name, Code, cancellationToken);

    /// <inheritdoc/>
    protected override string GetResponseText() => "Template: " + Name;
}