using System.Text.Json.Serialization;

namespace Devlooped.WhatsApp;

/// <summary>
/// A reaction to a message.
/// </summary>
/// <param name="Id">The identifier of the message this reaction applies to.</param>
/// <param name="To">The service that received the message from the Cloud API.</param>
/// <param name="From">The user that sent the message.</param>
/// <param name="Timestamp">Timestamp of the message.</param>
/// <param name="Emoji">The emoji of the reaction.</param>
public record ReactionMessage(string Id, Service To, User From, long Timestamp, string Emoji) : SystemMessage(Id, To, From, Timestamp)
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override MessageType Type => MessageType.Reaction;
}