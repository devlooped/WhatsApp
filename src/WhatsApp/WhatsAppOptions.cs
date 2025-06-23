namespace Devlooped.WhatsApp;

/// <summary>
/// Allows configuring behaviors on the core processing.
/// </summary>
public class WhatsAppOptions
{
    /// <summary>
    /// Mark messages as read when received in the WhatsApp webhook endpoint.
    /// </summary>
    public bool? ReadOnMessage { get; set; }

    /// <summary>
    /// Mark messages as read when processing is started.
    /// </summary>
    public bool? ReadOnProcess { get; set; }

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
}
