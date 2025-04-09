using System.Text.Json;
using System.Text.Json.Serialization;

namespace Devlooped.WhatsApp;

/// <summary>
/// An <see cref="Message"/> that notifies of an unsupported message received by 
/// the WhatsApp for Business service.
/// </summary>
/// <param name="Id">The message identifier.</param>
/// <param name="To">The service that received the message from the Cloud API.</param>
/// <param name="From">The user that sent the message.</param>
/// <param name="Timestamp">Timestamp of the message.</param>
/// <param name="Raw">JSON data.</param>
public record UnsupportedMessage(string Id, Service To, User From, long Timestamp, JsonElement Raw) : Message(Id, To, From, Timestamp)
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override MessageType Type => MessageType.Unsupported;
}