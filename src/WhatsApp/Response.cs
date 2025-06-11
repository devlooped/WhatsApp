namespace Devlooped.WhatsApp;

/// <summary>
/// Represents a response message or command that can be sent using a WhatsApp client.
/// </summary>
/// <remarks>This abstract record serves as a base type for defining specific response messages or commands that
/// can be sent to a WhatsApp client. It provides common properties such as <see cref="UserNumber"/>, <see cref="ServiceId"/>,
/// <see cref="Context"/>, and <see cref="ConversationId"/>, as well as methods for sending the response
/// asynchronously.</remarks>
/// <param name="Number">The phone number of the recipient in international format.</param>
/// <param name="ServiceId">The identifier of the service handling the message.</param>
/// <param name="Context">The unique identifier of the message to which the reaction is being sent.</param>
/// <param name="ConversationId">The conversation id where this response was generated</param>
public abstract partial record Response(string UserNumber, string ServiceId, string Context, string? ConversationId) : IMessage
{
    /// <inheritdoc/>
    public string Id { get; init; } = string.Empty;

    /// <inheritdoc/>
    public long Timestamp { get; init; }

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
    internal async Task<Response> SendAsync(IWhatsAppClient client, CancellationToken cancellation = default)
    {
        if (!string.IsNullOrEmpty(Id) || Timestamp != 0)
        {
            throw new InvalidOperationException("The response was already sent");
        }

        return this with
        {
            Id = await SendCoreAsync(client, cancellation) ?? string.Empty,
            Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds()
        };
    }

    /// <summary>
    /// Sends a message or command to the specified WhatsApp client asynchronously.
    /// </summary>
    /// <remarks>This method is intended to be implemented by derived classes to define the specific behavior
    /// for sending messages or commands  to a WhatsApp client. The implementation should handle any necessary
    /// serialization, communication, and response processing.</remarks>
    /// <param name="client">The <see cref="IWhatsAppClient"/> instance to which the message or command will be sent. Cannot be <see
    /// langword="null"/>.</param>
    /// <param name="cancellation">An optional <see cref="CancellationToken"/> to observe while waiting for the operation to complete.</param>
    /// <returns>A task that represents the asynchronous operation. The task result is a <see cref="string"/> containing the
    /// generated message id or null if it could not be generated</returns>
    protected abstract Task<string?> SendCoreAsync(IWhatsAppClient client, CancellationToken cancellation = default);
}