using System.Text.Json;
using System.Text.Json.Serialization;

namespace Devlooped.WhatsApp;

/// <summary>
/// An <see cref="Message"/> that notifies of an unsupported message received by 
/// the WhatsApp for Business service.
/// </summary>
/// <param name="Id">The message identifier.</param>
/// <param name="Service">The service that received the message from the Cloud API.</param>
/// <param name="User">The user that sent the message.</param>
/// <param name="Timestamp">Timestamp of the message.</param>
/// <param name="Raw">JSON data.</param>
public record UnsupportedMessage(string Id, Service Service, User User, long Timestamp, JsonElement Raw) : SystemMessage(Id, Service, User, Timestamp)
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override MessageType Type => MessageType.Unsupported;
}