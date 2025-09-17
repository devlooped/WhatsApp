namespace Devlooped.WhatsApp;

/// <summary>
/// Allows configuring behaviors on the core processing.
/// </summary>
public class WhatsAppOptions
{
    /// <summary>
    /// Configures the time window to consider for conversation messages. 
    /// Messages sent within this time frame will be grouped together as part of the same conversation.
    /// </summary>
    public int ConversationWindowSeconds { get; set; } = 5 * 60; // 5 minutes

    /// <summary>
    /// Mark messages as read when received in the WhatsApp webhook endpoint. 
    /// Defaults to <c>true</c>.
    /// </summary>
    public bool? ReadOnMessage { get; set; } = true;

    /// <summary>
    /// Send a typing indicator status when message is received in the 
    /// WhatsApp webhook endpoint.
    /// </summary>
    public bool? TypingOnMessage { get; set; } = true;

    /// <summary>
    /// Mark messages as read when processing is started.
    /// </summary>
    public bool? ReadOnProcess { get; set; }

    /// <summary>
    /// Send a typing indicator status when message processing begins.
    /// </summary>
    public bool? TypingOnProcess { get; set; } = true;

    /// <summary>
    /// An optional emoji to react with when a message is received 
    /// in the WhatsApp webhook endpoint.
    /// </summary>
    public string? ReactOnMessage { get; set; }

    /// <summary>
    /// An optional emoji to react with when message processing is started.
    /// </summary>
    public string? ReactOnProcess { get; set; }

    /// <summary>
    /// An optional emoji to react with when restoring conversation context.
    /// </summary>
    public string? ReactOnConversation { get; set; }

    /// <summary>
    /// Optional secret to enable direct POST processing of webhook-formatted 
    /// payloads without going through a queue. 
    /// </summary>
    /// <remarks>
    /// If used, the incoming POST request must have the X-WHATSAPP-SECRET 
    /// header set and it must match exactly the value of this option.
    /// </remarks>
    public string? Secret { get; set; }
}
