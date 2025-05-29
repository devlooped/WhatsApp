using System.Text.Json.Serialization;

namespace Devlooped.WhatsApp;

public record ResponseContentMessage(string Id, Service To, User From, Content Content) : Message(Id, To, From, int.MinValue)
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override MessageType Type => MessageType.Content;
}