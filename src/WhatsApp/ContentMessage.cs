namespace Devlooped.WhatsApp;

/// <summary>
/// A <see cref="Message"/> containing <see cref="Content"/>.
/// </summary>
/// <param name="Id">The message identifier.</param>
/// <param name="To">The service that received the message from the Cloud API.</param>
/// <param name="From">The user that sent the message.</param>
/// <param name="Timestamp">Timestamp of the message.</param>
/// <param name="Content">Message content.</param>
public record ContentMessage(string Id, Service To, User From, long Timestamp, Content Content) : Message(Id, To, From, Timestamp)
{
    /// <summary>
    /// A JQ query that transforms WhatsApp Cloud API JSON into polymorphic JSON for 
    /// C# deserialization of a <see cref="ContentMessage"/>.
    /// </summary>
    public const string JQ =
        """
        .entry[].changes[].value.metadata as $phone |
        .entry[].changes[].value.contacts[] as $user |
        .entry[].changes[].value.messages[] | 
        .type as $type |
        {
            id: .id,
            timestamp: .timestamp | tonumber,
            to: {
                id: $phone.phone_number_id,
                number: $phone.display_phone_number
            },
            from: {
                name: $user.profile.name,
                number: $user.wa_id
            },
            content: (
                if $type == "document" then {
                    "$type": $type,
                    id: .document.id,
                    name: .document.filename,
                    mime: .document.mime_type,
                    sha256: .document.sha256
                }
                elif $type == "contacts" then {
                    "$type": $type,
                    name: .contacts[].name.first_name,
                    surname: .contacts[].name.last_name,
                    numbers: [.contacts[].phones[] | 
                        select(.type == "MOBILE") | .wa_id]
                }
                elif $type == "text" then {
                    "$type": $type,
                    text: .text.body
                }
                elif $type == "location" then {
                    "$type": $type,
                    location: .location,
                }
                elif $type == "image" or $type == "video" or $type == "audio" then {
                    "$type": $type,
                    id: .[$type].id,
                    mime: .[$type].mime_type,
                    sha256: .[$type].sha256
                }
                else {
                    raw: .
                }
                end
            )
        }
        """;

    /// <inheritdoc/>
    public override MessageType Type => MessageType.Content;
}
