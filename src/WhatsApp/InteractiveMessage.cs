using System.Text.Json.Serialization;

namespace Devlooped.WhatsApp;

/// <summary>
/// A <see cref="Message"/> containing an interactive button reply.
/// </summary>
/// <param name="Id">The message identifier.</param>
/// <param name="To">The service that received the message from the Cloud API.</param>
/// <param name="From">The user that sent the message.</param>
/// <param name="Timestamp">Timestamp of the message.</param>
/// <param name="Button">The button selected by the user.</param>
public record InteractiveMessage(string Id, Service To, User From, long Timestamp, Button Button) : Message(Id, To, From, Timestamp)
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override MessageType Type => MessageType.Interactive;
}

public record Button(string Id, string Title);