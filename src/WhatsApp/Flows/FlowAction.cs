using System.Text.Json;
using System.Text.Json.Serialization;

namespace Devlooped.WhatsApp.Flows;

#if NET9_0_OR_GREATER
#else
[JsonConverter(typeof(FlowActionJsonConverter))]
#endif
public enum FlowAction
{
    /// <summary>Causes the flow to be navigated to a specific screen without involving a server request.</summary>
    /// <remarks>The default if not specified for <see cref="FlowParameters"/>.</remarks>
    Navigate,
    /// <summary>Causes the flow to be initialized by a data exchange request to the server.</summary>
#if NET9_0_OR_GREATER
    [JsonStringEnumMemberName("data_exchange")]
#endif
    DataExchange,
}

// If we drop .net8.0, we can just use the built-in JsonStringEnumMemberName attribute
// and remove the custom converter.
class FlowActionJsonConverter : JsonConverter<FlowAction>
{
    public override FlowAction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType != JsonTokenType.String ? throw new JsonException() :
        reader.GetString()!.ToLowerInvariant() switch
        {
            "navigate" => FlowAction.Navigate,
            "data_exchange" or "dataexchange" => FlowAction.DataExchange,
            _ => throw new JsonException()
        };
    public override void Write(Utf8JsonWriter writer, FlowAction value, JsonSerializerOptions options)
        => writer.WriteStringValue(value == FlowAction.Navigate ? "navigate" : "data_exchange");
}