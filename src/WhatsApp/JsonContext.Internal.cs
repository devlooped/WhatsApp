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
[JsonSerializable(typeof(GraphMethodException))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(SendResponse))]
[JsonSerializable(typeof(MessageId))]
partial class InternalJsonContext : JsonSerializerContext
{
}

record ErrorResponse(GraphMethodException Error);

record SendResponse(MessageId[] Messages);

record MessageId(string Id);