using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

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
[JsonDerivedType(typeof(StatusMessage), "status")]
[JsonDerivedType(typeof(UnsupportedMessage), "unsupported")]
public abstract partial record Message(string Id, Service To, User From, long Timestamp)
{
    const string JQ =
        """
        (
          .entry[].changes[] |
          select(.value.messages != null) |
          (.value.metadata as $phone |
           .value.contacts[0] as $user |
           .value.messages[0] as $msg |
           select($msg != null) |
           if $msg.type == "interactive" then
             {
               "$type": "interactive",
               "id": $msg.id,
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
           elif $msg.type == "document" or $msg.type == "contacts" or $msg.type == "text" or $msg.type == "location" or $msg.type == "image" or $msg.type == "video" or $msg.type == "audio" then
             {
               "$type": "content",
               "id": $msg.id,
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
                 if $msg.type == "document" then
                   {
                     "$type": "document",
                     "id": $msg.document.id,
                     "name": $msg.document.filename,
                     "mime": $msg.document.mime_type,
                     "sha256": $msg.document.sha256
                   }
                 elif $msg.type == "contacts" then
                   {
                     "$type": "contacts",
                     "name": $msg.contacts[0].name.first_name,
                     "surname": $msg.contacts[0].name.last_name,
                     "numbers": [$msg.contacts[0].phones[] | select(.wa_id? != null) | .wa_id]
                   }
                 elif $msg.type == "text" then
                   {
                     "$type": "text",
                     "text": $msg.text.body
                   }
                 elif $msg.type == "location" then
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
                 elif $msg.type == "image" or $msg.type == "video" or $msg.type == "audio" then
                   {
                     "$type": $msg.type,
                     "id": $msg[$msg.type].id,
                     "mime": $msg[$msg.type].mime_type,
                     "sha256": $msg[$msg.type].sha256
                   }
                 end
               )
             }
           else
             {
               "$type": "unsupported",
               "id": $msg.id,
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
        ),
        (
          .entry[].changes[] |
          select(.value.statuses != null) |
          (.value.metadata as $phone |
           .value.statuses[0] as $status |
           select($status != null) |
           if $status.errors? then
             $status.errors[] |
             {
               "$type": "error",
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
            return JsonSerializer.Deserialize(jq, MessageSerializerContext.Default.Message);

        // NOTE: unsupported payloads would not generate a JQ output, so we can safely ignore them.
        return default;
    }

    /// <summary>
    /// Gets the type of message.
    /// </summary>
    [JsonIgnore]
    public abstract MessageType Type { get; }

    [JsonSourceGenerationOptions(JsonSerializerDefaults.Web, WriteIndented = true, UseStringEnumConverter = true)]
    [JsonSerializable(typeof(Message))]
    [JsonSerializable(typeof(ContentMessage))]
    [JsonSerializable(typeof(ErrorMessage))]
    [JsonSerializable(typeof(InteractiveMessage))]
    [JsonSerializable(typeof(StatusMessage))]
    [JsonSerializable(typeof(UnsupportedMessage))]
    partial class MessageSerializerContext : JsonSerializerContext { }
}
