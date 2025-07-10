namespace Devlooped.WhatsApp;

/// <summary>
/// A response that uses a function to send the message.
/// </summary>
/// <param name="ServiceId">The identifier of the service to use to send the response through.</param>
/// <param name="UserNumber">The phone number of the recipient in international format.</param>
/// <param name="Sender">The function that implements the response sending behavior.</param>
/// <param name="Context">Optional identifier of the message to which this response may be a reply to.</param>
public record AnonymousResponse(string ServiceId, string UserNumber, Func<IWhatsAppClient, CancellationToken, Task<string?>> Sender, string? Context = null) : Response(ServiceId, UserNumber, Context)
{
    /// <summary>
    /// Initializes a new instance of the <see cref="AnonymousResponse"/> class using an existing message and a sender function.
    /// </summary>
    public AnonymousResponse(IMessage message, Func<IWhatsAppClient, CancellationToken, Task<string?>> sender)
        : this(message.ServiceId, message.UserNumber, sender) { }

    protected override Task<string?> SendCoreAsync(IWhatsAppClient client, CancellationToken cancellation = default) => Sender(client, cancellation);
}
