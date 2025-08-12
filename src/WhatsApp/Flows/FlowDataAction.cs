using System.Text.Json;
using System.Text.Json.Serialization;

namespace Devlooped.WhatsApp.Flows;

#if NET9_0_OR_GREATER
#else
[JsonConverter(typeof(FlowDataActionJsonConverter))]
#endif
public enum FlowDataAction
{
#if NET9_0_OR_GREATER
    [JsonStringEnumMemberName("INIT")]
#endif
    Init,
#if NET9_0_OR_GREATER
    [JsonStringEnumMemberName("BACK")]
#endif
    Back,
#if NET9_0_OR_GREATER
    [JsonStringEnumMemberName("data_exchange")]
#endif
    DataExchange,
}

// If we drop .net8.0, we can just use the built-in JsonStringEnumMemberName attribute
// and remove the custom converter.
class FlowDataActionJsonConverter : JsonConverter<FlowDataAction>
{
    public override FlowDataAction Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
        => reader.TokenType != JsonTokenType.String ? throw new JsonException() :
        reader.GetString()!.ToUpperInvariant() switch
        {
            "INIT" => FlowDataAction.Init,
            "BACK" => FlowDataAction.Back,
            "DATA_EXCHANGE" or "DATAEXCHANGE" => FlowDataAction.DataExchange,
            _ => throw new JsonException()
        };

    public override void Write(Utf8JsonWriter writer, FlowDataAction value, JsonSerializerOptions options)
        => writer.WriteStringValue(value switch
        {
            FlowDataAction.Init => "INIT",
            FlowDataAction.Back => "BACK",
            FlowDataAction.DataExchange => "data_exchange",
            _ => throw new ArgumentOutOfRangeException(nameof(value), value, null)
        });
}