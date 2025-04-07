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
    /// <summary>
    /// A JQ query that transforms WhatsApp Cloud API JSON into the serialization 
    /// expected by <see cref="ErrorMessage"/>.
    /// </summary>
    public const string JQ =
        """
        .entry[].changes[].value.metadata as $phone |
        .entry[].changes[].value.statuses[] | 
        {
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
            error: .errors[]? | {
                code: .code,
                message: (.error_data.details // .message),
            }
        }
        """;

    /// <inheritdoc/>
    public override MessageType Type => MessageType.Error;
}

/// <summary>
/// The error in an <see cref="ErrorMessage"/>.
/// </summary>
/// <param name="Code">The error code.</param>
/// <param name="Message">The error message.</param>
public record Error(int Code, string Message);

