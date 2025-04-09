using System.Text.Json.Serialization;

namespace Devlooped.WhatsApp;

/// <summary>
/// A <see cref="Message"/> containing <see cref="Content"/>.
/// </summary>
/// <param name="Id">The message identifier.</param>
/// <param name="To">The service that received the message from the Cloud API.</param>
/// <param name="From">The user that sent the message.</param>
/// <param name="Timestamp">Timestamp of the message.</param>
/// <param name="Content">Message content.</param>
public record ContentMessage(string Id, Service To, User From, long Timestamp, Content Content) : Message(Id, To, From, Timestamp)
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override MessageType Type => MessageType.Content;
}
