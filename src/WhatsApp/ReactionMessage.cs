using System.Text.Json.Serialization;

namespace Devlooped.WhatsApp;

/// <summary>
/// A reaction to a message.
/// </summary>
/// <param name="Id">The identifier of the message this reaction applies to.</param>
/// <param name="Service">The service that received the message from the Cloud API.</param>
/// <param name="User">The user that sent the message.</param>
/// <param name="Timestamp">Timestamp of the message.</param>
/// <param name="Emoji">The emoji of the reaction.</param>
public record ReactionMessage(string Id, Service Service, User User, long Timestamp, string Emoji) : SystemMessage(Id, Service, User, Timestamp)
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override MessageType Type => MessageType.Reaction;
}