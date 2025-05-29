namespace Devlooped.WhatsApp;

/// <summary>
/// Represents a response sent via WhatsApp, containing the associated message and response metadata.
/// </summary>
/// <remarks>This abstract record serves as a base type for specific response implementations. It encapsulates the
/// message being sent and provides functionality for sending the response asynchronously using a WhatsApp
/// client.</remarks>
/// <param name="Message"></param>
public abstract partial record Response(Message Message)
{
    /// <summary>
    /// Gets the unique identifier for this instance.
    /// </summary>
    public string? Id { get; set; }

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

    /// <summary>
    /// Converts the current response content into a <see cref="ResponseContentMessage"/> object.
    /// </summary>
    public ResponseContentMessage? AsMessage() => Id != null ? new ResponseContentMessage(Id, Message.To, Message.From, new TextContent(GetResponseText())) : null;

    /// <summary>
    /// Retrieves the response text associated with the current response.
    /// </summary>
    /// <remarks>This method can be overridden in a derived class to provide a custom response text.</remarks>
    /// <returns>A <see cref="string"/> containing the response text. Returns an empty string if no response text is available.</returns>
    protected virtual string GetResponseText() => string.Empty;
}