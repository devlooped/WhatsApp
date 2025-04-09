using System.Text.Json.Serialization;

namespace Devlooped.WhatsApp;

/// <summary>
/// A <see cref="Message"/> containing an <see cref="Error"/>.
/// </summary>
/// <param name="Id">The message identifier.</param>
/// <param name="To">The service that received the message from the Cloud API.</param>
/// <param name="From">The user that sent the message.</param>
/// <param name="Timestamp">Timestamp of the message.</param>
/// <param name="Error">The error.</param>
public record ErrorMessage(string Id, Service To, User From, long Timestamp, Error Error) : Message(Id, To, From, Timestamp)
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override MessageType Type => MessageType.Error;
}

/// <summary>
/// The error in an <see cref="ErrorMessage"/>.
/// </summary>
/// <param name="Code">The error code.</param>
/// <param name="Message">The error message.</param>
public record Error(int Code, string Message);

