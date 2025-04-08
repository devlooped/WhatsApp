namespace Devlooped.WhatsApp;

/// <summary>
/// Type of message received by the service.
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
    /// Message contains a status update.
    /// </summary>
    Status,
    /// <summary>
    /// Message contains a button reply.
    /// </summary>
    Interactive,
}