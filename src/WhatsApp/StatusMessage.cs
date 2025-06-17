using System.Text.Json.Serialization;

namespace Devlooped.WhatsApp;

/// <summary>
/// A status update about a message.
/// </summary>
/// <param name="Id">The identifier of the message this status update relates to.</param>
/// <param name="Service">The service that received the message from the Cloud API.</param>
/// <param name="User">The user that sent the message.</param>
/// <param name="Timestamp">Timestamp of the message.</param>
/// <param name="Status">The message status.</param>
public record StatusMessage(string Id, Service Service, User User, long Timestamp, Status Status) : SystemMessage(Id, Service, User, Timestamp)
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override MessageType Type => MessageType.Status;
}

/// <summary>
/// Known statuses for a message.
/// </summary>
public enum Status
{
    /// <summary>
    /// The message was sent.
    /// </summary>
    Sent,
    /// <summary>
    /// The message was delivered.
    /// </summary>
    Delivered,
    /// <summary>
    /// The message was read.
    /// </summary>
    Read,
}
