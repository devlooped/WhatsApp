using System.Text.Json.Serialization;

namespace Devlooped.WhatsApp;

/// <summary>
/// A <see cref="Message"/> containing <see cref="Content"/>.
/// </summary>
/// <param name="Id">The message identifier.</param>
/// <param name="Service">The service that received the message from the Cloud API.</param>
/// <param name="User">The user that sent the message.</param>
/// <param name="Timestamp">Timestamp of the message.</param>
/// <param name="Content">Message content.</param>
public record ContentMessage(string Id, Service Service, User User, long Timestamp, Content Content) : UserMessage(Id, Service, User, Timestamp)
{
    /// <inheritdoc/>
    [JsonIgnore]
    public override MessageType Type => MessageType.Content;

    /// <summary>
    /// Creates a simple text message with the given service ID, user number, and text content.
    /// </summary>
    public static ContentMessage Create(string serviceId, string userNumber, string text) => new ContentMessage(
        Ulid.NewUlid().ToString(),
        new Service(serviceId, serviceId),
        new User(userNumber, userNumber),
        DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
        new TextContent(text));
}
