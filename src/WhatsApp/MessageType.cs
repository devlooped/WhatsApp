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
    /// The message is a flow endpoint data exchange.
    /// </summary>
    FlowData,
    /// <summary>
    /// Message contains a button or list selection reply.
    /// </summary>
    Interactive,
    /// <summary>
    /// Message contains the final reply after completing an interactive flow.
    /// </summary>
    InteractiveFlow,
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