namespace Devlooped.WhatsApp;

/// <summary>
/// Usability extensions for creating responses for user messages.
/// </summary>
public static partial class MessageExtensions
{
    /// <summary>
    /// Creates a reaction response for the user message.
    /// </summary>
    public static ReactionResponse React(this UserMessage message, string emoji)
        => new ReactionResponse(message, emoji);

    /// <summary>
    /// Creates a reengagement response for the error message.
    /// </summary>
    public static TemplateResponse Reengage(this ErrorMessage message)
        => new TemplateResponse(message, "reengagement", "es_AR");

    /// <summary>
    /// Creates a text response for the message.
    /// </summary>
    public static TextResponse Text(this Message message, string text)
        => new TextResponse(message, text);

    /// <summary>
    /// Creates a text response with buttons for the message.
    /// </summary>
    public static TextResponse TextWithButtons(this Message message, string text, Button button1, Button? button2 = default)
        => new TextResponse(message, text, button1, button2);
}