namespace Devlooped.WhatsApp;

/// <summary>
/// A <see cref="Message"/> containing a status update.
/// </summary>
/// <param name="Id">The message identifier.</param>
/// <param name="To">The service that received the message from the Cloud API.</param>
/// <param name="From">The user that sent the message.</param>
/// <param name="Timestamp">Timestamp of the message.</param>
/// <param name="Status">The message status.</param>
public record StatusMessage(string Id, Service To, User From, long Timestamp, Status Status) : Message(Id, To, From, Timestamp)
{
    /// <summary>
    /// A JQ query that transforms WhatsApp Cloud API JSON into the serialization 
    /// expected by <see cref="StatusMessage"/>.
    /// </summary>
    public const string JQ =
        """
        .entry[].changes[].value.metadata as $phone |
        .entry[].changes[].value.statuses[] | 
        select(. != null) | {
            id: .id,
            timestamp: .timestamp | tonumber,
            to: {
                id: $phone.phone_number_id,
                number: $phone.display_phone_number
            },
            from: {
                name: .recipient_id,
                number: .recipient_id
            },
            status: .status
        }        
        """;

    /// <inheritdoc/>
    public override MessageType Type => MessageType.Status;
}

public enum Status
{
    Sent,
    Delivered,
    Read,
}
