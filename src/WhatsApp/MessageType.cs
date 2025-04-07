namespace Devlooped.WhatsApp;

/// <summary>
/// Type of message received by the service.
/// </summary>
public enum MessageType
{
    /// <summary>
    /// Message contains an error.
    /// </summary>
    Error,
    /// <summary>
    /// Message contains user content.
    /// </summary>
    Content
}