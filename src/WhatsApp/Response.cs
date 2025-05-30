namespace Devlooped.WhatsApp;

/// <summary>
/// Represents a response sent via WhatsApp, containing the associated message and response metadata.
/// </summary>
/// <remarks>This abstract record serves as a base type for specific response implementations. It encapsulates the
/// message being sent and provides functionality for sending the response asynchronously using a WhatsApp
/// client.</remarks>
/// <param name="Message"></param>
public abstract partial record Response(Message Message) : IMessage
{
    /// <inheritdoc/>
    public string Id { get; set; } = string.Empty;

    /// <inheritdoc/>
    public string Number => Message.From.Number;

    /// <summary>
    /// Sends a request asynchronously using the specified WhatsApp client.
    /// </summary>
    /// <remarks>This method is abstract and must be implemented by a derived class to define the specific
    /// behavior for sending a request.</remarks>
    /// <param name="client">The <see cref="IWhatsAppClient"/> instance used to send the request. This parameter cannot be <see
    /// langword="null"/>.</param>
    /// <param name="cancellation">An optional <see cref="CancellationToken"/> to observe while waiting for the task to complete. Defaults to <see
    /// cref="CancellationToken.None"/>.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous operation.</returns>
    internal abstract Task SendAsync(IWhatsAppClient client, CancellationToken cancellation = default);
}