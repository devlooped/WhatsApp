using System.Text.Json;
using System.Text.Json.Serialization;

namespace Devlooped.WhatsApp;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    UseStringEnumConverter = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    WriteIndented = true
    )]
[JsonSerializable(typeof(Message))]
[JsonSerializable(typeof(ContentMessage))]
[JsonSerializable(typeof(ErrorMessage))]
[JsonSerializable(typeof(InteractiveMessage))]
[JsonSerializable(typeof(ReactionMessage))]
[JsonSerializable(typeof(StatusMessage))]
[JsonSerializable(typeof(UnsupportedMessage))]
[JsonSerializable(typeof(MediaReference))]
[JsonSerializable(typeof(GraphMethodException))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(SendResponse))]
[JsonSerializable(typeof(MessageId))]
partial class WhatsAppSerializerContext : JsonSerializerContext { }

record ErrorResponse(GraphMethodException Error);

record SendResponse(MessageId[] Messages);

record MessageId(string Id);