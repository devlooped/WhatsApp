using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace Devlooped.WhatsApp;

partial class AdditionalPropertiesDictionaryConverter : JsonConverter<AdditionalPropertiesDictionary>
{
    public override AdditionalPropertiesDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
            throw new JsonException("Expected start of object.");

        var dictionary = new AdditionalPropertiesDictionary();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
                return dictionary;

            if (reader.TokenType != JsonTokenType.PropertyName)
                throw new JsonException("Expected property name.");

            var key = reader.GetString()!;
            reader.Read();

            var value = JsonSerializer.Deserialize<object>(ref reader, options);
            if (value is JsonElement element)
                dictionary[key] = GetPrimitive(element);
            else
                dictionary[key] = value;
        }

        throw new JsonException("Unexpected end of JSON.");
    }

    public override void Write(Utf8JsonWriter writer, AdditionalPropertiesDictionary value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (var kvp in value.Where(x => x.Value is not null))
        {
            writer.WritePropertyName(kvp.Key);
            JsonSerializer.Serialize(writer, kvp.Value, options);
        }

        writer.WriteEndObject();
    }

    // Helper to convert JsonElement to closest .NET primitive
    static object? GetPrimitive(JsonElement element)
    {
        switch (element.ValueKind)
        {
            case JsonValueKind.String: return element.GetString();
            case JsonValueKind.Number:
                if (element.TryGetInt32(out var i)) return i;
                if (element.TryGetInt64(out var l)) return l;
                if (element.TryGetDouble(out var d)) return d;
                return element.GetDecimal();
            case JsonValueKind.True: return true;
            case JsonValueKind.False: return false;
            case JsonValueKind.Null: return null;
            case JsonValueKind.Object: return element; // You can recurse here if needed
            case JsonValueKind.Array: return element;  // Or parse as List<object?>
            default: return element;
        }
    }
}