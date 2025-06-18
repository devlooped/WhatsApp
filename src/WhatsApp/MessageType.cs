namespace Devlooped.WhatsApp;

/// <summary>
/// Type of message.
/// </summary>
public enum MessageType
{
    /// <summary>
    /// Message contains user content.
    /// </summary>
    Content,
    /// <summary>
    /// Message contains an error.
    /// </summary>
    Error,
    /// <summary>
    /// Message contains a button reply.
    /// </summary>
    Interactive,
    /// <summary>
    /// Message contains a reaction to a message.
    /// </summary>
    Reaction,
    /// <summary>
    /// Message contains a status update.
    /// </summary>
    Status,
    /// <summary>
    /// Message is a response from the service, rather than an incoming message.
    /// </summary>
    Response,
    /// <summary>
    /// Message type is not supported by the WhatsApp for Business service.
    /// </summary>
    Unsupported,
}