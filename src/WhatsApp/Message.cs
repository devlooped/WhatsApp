using System.Text.Json;
using System.Text.Json.Serialization;

namespace Devlooped.WhatsApp;

/// <summary>
/// Base class for WhatsApp Cloud API messages.
/// </summary>
/// <param name="Id">The message identifier.</param>
/// <param name="To">The service that received the message from the Cloud API.</param>
/// <param name="From">The user that sent the message.</param>
/// <param name="Timestamp">Timestamp of the message.</param>
[JsonPolymorphic]
[JsonDerivedType(typeof(ContentMessage), "content")]
[JsonDerivedType(typeof(ErrorMessage), "error")]
[JsonDerivedType(typeof(InteractiveMessage), "interactive")]
[JsonDerivedType(typeof(ReactionMessage), "reaction")]
[JsonDerivedType(typeof(StatusMessage), "status")]
[JsonDerivedType(typeof(UnsupportedMessage), "unsupported")]
public abstract partial record Message(string Id, Service To, User From, long Timestamp)
{
    /// <summary>
    /// Optional related message identifier, such as message being replied 
    /// or reacted to, or a status message refers to, or the interactive 
    /// selection is a response to.
    /// </summary>
    /// <remarks>
    /// In a <see cref="StatusMessage"/>, the context equals the status ID which 
    /// in turn equals the message ID the status refers to.
    /// </remarks>
    public string? Context { get; init; }

    [JsonInclude]
    [JsonPropertyName("notification")]
    internal string? NotificationId { get; init; }

    const string JQ =
        """
        (
          .entry[] | 
          .id as $notification |
          .changes[] |
          select(.value.messages != null) |
          (.value.metadata as $phone |
           .value.contacts[0] as $user |
           .value.messages[0] as $msg |
           select($msg != null) |
           ($msg.type as $msgType |
            # Compute context once for all message types
            (if $msgType == "reaction" then $msg.reaction.message_id else ($msg.context.id // null) end) as $context |
            if $msgType == "interactive" then
              {
                "$type": "interactive",
                "notification": $notification,
                "id": $msg.id,
                "context": $context,
                "timestamp": $msg.timestamp | tonumber,
                "to": {
                  "id": $phone.phone_number_id,
                  "number": $phone.display_phone_number
                },
                "from": {
                  "name": ($user.profile.name // "Unknown"),
                  "number": $msg.from
                },
                "button": $msg.interactive.button_reply
              }
            elif $msgType == "reaction" then
              {
                "$type": "reaction",
                "notification": $notification,
                "id": $msg.id,                    
                "context": $context,              
                "timestamp": $msg.timestamp | tonumber,
                "to": {
                  "id": $phone.phone_number_id,
                  "number": $phone.display_phone_number
                },
                "from": {
                  "name": ($user.profile.name // "Unknown"),
                  "number": $msg.from
                },
                "emoji": $msg.reaction.emoji
              }
            elif $msgType == "document" or $msgType == "contacts" or $msgType == "text" or $msgType == "location" or $msgType == "image" or $msgType == "video" or $msgType == "audio" then
              {
                "$type": "content",
                "notification": $notification,
                "id": $msg.id,
                "context": $context,
                "timestamp": $msg.timestamp | tonumber,
                "to": {
                  "id": $phone.phone_number_id,
                  "number": $phone.display_phone_number
                },
                "from": {
                  "name": ($user.profile.name // "Unknown"),
                  "number": $msg.from
                },
                "content": (
                  if $msgType == "document" then
                    {
                      "$type": "document",
                      "id": $msg.document.id,
                      "name": $msg.document.filename,
                      "mime": $msg.document.mime_type,
                      "sha256": $msg.document.sha256
                    }
                  elif $msgType == "contacts" then
                    {
                      "$type": "contacts",
                      "name": $msg.contacts[0].name.first_name,
                      "surname": $msg.contacts[0].name.last_name,
                      "numbers": [$msg.contacts[0].phones[] | select(.wa_id? != null) | .wa_id]
                    }
                  elif $msgType == "text" then
                    {
                      "$type": "text",
                      "text": $msg.text.body
                    }
                  elif $msgType == "location" then
                    {
                      "$type": "location",
                      "location": {
                        "latitude": $msg.location.latitude,
                        "longitude": $msg.location.longitude
                      },
                      "address": $msg.location.address,
                      "name": $msg.location.name,
                      "url": $msg.location.url
                    }
                  elif $msgType == "image" or $msgType == "video" or $msgType == "audio" then
                    {
                      "$type": $msgType,
                      "id": $msg[$msgType].id,
                      "mime": $msg[$msgType].mime_type,
                      "sha256": $msg[$msgType].sha256
                    }
                  end
                )
              }
            else
              {
                "$type": "unsupported",
                "notification": $notification,
                "id": $msg.id,
                "context": $context,
                "timestamp": $msg.timestamp | tonumber,
                "to": {
                  "id": $phone.phone_number_id,
                  "number": $phone.display_phone_number
                },
                "from": {
                  "name": ($user.profile.name // "Unknown"),
                  "number": $msg.from
                },
                "raw": $msg
              }
            end
           )
          )
        ),
        (
          .entry[] | 
          .id as $notification |
          .changes[] |
          select(.value.statuses != null) |
          (.value.metadata as $phone |
           .value.statuses[0] as $status |
           select($status != null) |
           if $status.errors? then
             $status.errors[] |
             {
               "$type": "error",
               "notification": $notification,
               "id": $status.id,
               "timestamp": $status.timestamp | tonumber,
               "to": {
                 "id": $phone.phone_number_id,
                 "number": $phone.display_phone_number
               },
               "from": {
                 "name": $status.recipient_id,
                 "number": $status.recipient_id
               },
               "error": {
                 "code": .code,
                 "message": (.error_data.details // .message)
               }
             }
           else
             {
               "$type": "status",
               "notification": $notification,
               "id": $status.id,
               "context": $status.id,
               "timestamp": $status.timestamp | tonumber,
               "to": {
                 "id": $phone.phone_number_id,
                 "number": $phone.display_phone_number
               },
               "from": {
                 "name": $status.recipient_id,
                 "number": $status.recipient_id
               },
               "status": $status.status
             }
           end
          )
        )
        """;

    /// <summary>
    /// Deserializes the given JSON string into a <see cref="Message"/> instance.
    /// </summary>
    /// <param name="json">The Cloud API JSON string.</param>
    /// <returns>The typed message, or null if the incoming JSON was null or empty, or it's an 
    /// unsupported message type (i.e. not <see cref="ContentMessage"/> nor <see cref="ErrorMessage"/>).</returns>
    public static async Task<Message?> DeserializeAsync(string json)
    {
        if (string.IsNullOrEmpty(json))
            return default;

        // NOTE: if we got a JQ-transformed payload, deserialization MUST work, or we have a bug.
        // So we don't try..catch things in that code path.

        var jq = await Devlooped.JQ.ExecuteAsync(json, JQ);
        if (!string.IsNullOrEmpty(jq))
            return JsonSerializer.Deserialize(jq, JsonContext.Default.Message);

        // NOTE: unsupported payloads would not generate a JQ output, so we can safely ignore them.
        return default;
    }

    /// <summary>
    /// Gets the type of message.
    /// </summary>
    [JsonIgnore]
    public abstract MessageType Type { get; }
}
