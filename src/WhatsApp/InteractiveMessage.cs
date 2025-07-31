using System.Text.Json.Serialization;

namespace Devlooped.WhatsApp;

/// <summary>
/// A <see cref="Message"/> containing an interactive reply for either a button or list message.
/// </summary>
/// <param name="Id">The message identifier.</param>
/// <param name="Service">The service that received the message from the Cloud API.</param>
/// <param name="User">The user that sent the message.</param>
/// <param name="Timestamp">Timestamp of the message.</param>
/// <param name="Selection">The button or item selected by the user.</param>
public record InteractiveMessage(string Id, Service Service, User User, long Timestamp, Selection Selection) : UserMessage(Id, Service, User, Timestamp)
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override MessageType Type => MessageType.Interactive;
}

/// <summary>
/// Selection made by the user in an interactive message, such as a button or list item.
/// </summary>
/// <param name="Text">The selection text (i.e. button or list item text).</param>
/// <param name="Value">The value associated with the selection (i.e. button id or payload).</param>
public record Selection(string Text, string Value);