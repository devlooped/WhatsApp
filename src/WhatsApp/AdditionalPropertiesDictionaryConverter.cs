using System.Text.Json;
using System.Text.Json.Serialization;


namespace Devlooped.WhatsApp;

class AdditionalPropertiesDictionaryConverter : JsonConverter<AdditionalPropertiesDictionary>
{
    const string TypeKey = "$type";
    const string ValueKey = "$value";

    public override AdditionalPropertiesDictionary Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        if (reader.TokenType != JsonTokenType.StartObject)
        {
            throw new JsonException("Expected start of object.");
        }

        var dictionary = new AdditionalPropertiesDictionary();

        while (reader.Read())
        {
            if (reader.TokenType == JsonTokenType.EndObject)
            {
                return dictionary;
            }

            if (reader.TokenType != JsonTokenType.PropertyName)
            {
                throw new JsonException("Expected property name.");
            }

            var key = reader.GetString()!;
            reader.Read();

            if (reader.TokenType == JsonTokenType.StartObject)
            {
                var nestedObject = JsonSerializer.Deserialize<JsonElement>(ref reader, options);
                if (nestedObject.TryGetProperty(TypeKey, out var typeElement) && typeElement.ValueKind == JsonValueKind.String)
                {
                    var typeString = typeElement.GetString()!;
                    if (Enum.TryParse<TypeCode>(typeString, out var typeCode))
                    {
                        if (nestedObject.TryGetProperty(ValueKey, out var valueElement))
                        {
                            var value = DeserializePrimitive(typeCode, valueElement);
                            dictionary[key] = value;
                        }
                        else
                        {
                            throw new JsonException($"Missing '{ValueKey}' in object with '{TypeKey}'.");
                        }
                    }
                    else
                    {
                        dictionary[key] = nestedObject;
                    }
                }
                else
                {
                    dictionary[key] = nestedObject;
                }
            }
            else
            {
                var value = JsonSerializer.Deserialize<object>(ref reader, options);
                dictionary[key] = value;
            }
        }

        throw new JsonException("Unexpected end of JSON.");
    }

    public override void Write(Utf8JsonWriter writer, AdditionalPropertiesDictionary value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();

        foreach (var kvp in value)
        {
            writer.WritePropertyName(kvp.Key);

            if (kvp.Value == null)
            {
                writer.WriteNullValue();
            }
            else if (IsPrimitiveType(kvp.Value.GetType()))
            {
                writer.WriteStartObject();
                writer.WriteString(TypeKey, GetTypeCode(kvp.Value.GetType()).ToString());
                writer.WritePropertyName(ValueKey);
                JsonSerializer.Serialize(writer, kvp.Value, kvp.Value.GetType(), options);
                writer.WriteEndObject();
            }
            else
            {
                JsonSerializer.Serialize(writer, kvp.Value, kvp.Value.GetType(), options);
            }
        }

        writer.WriteEndObject();
    }

    static bool IsPrimitiveType(Type type) =>
        type.IsPrimitive ||
        type == typeof(string) ||
        type == typeof(decimal) ||
        type == typeof(DateTime) ||
        type == typeof(Guid);

    static TypeCode GetTypeCode(Type type) => type == typeof(Guid) ? TypeCode.Object : type switch
    {
        var t when t == typeof(bool) => TypeCode.Boolean,
        var t when t == typeof(byte) => TypeCode.Byte,
        var t when t == typeof(sbyte) => TypeCode.SByte,
        var t when t == typeof(char) => TypeCode.Char,
        var t when t == typeof(decimal) => TypeCode.Decimal,
        var t when t == typeof(double) => TypeCode.Double,
        var t when t == typeof(float) => TypeCode.Single,
        var t when t == typeof(int) => TypeCode.Int32,
        var t when t == typeof(uint) => TypeCode.UInt32,
        var t when t == typeof(long) => TypeCode.Int64,
        var t when t == typeof(ulong) => TypeCode.UInt64,
        var t when t == typeof(short) => TypeCode.Int16,
        var t when t == typeof(ushort) => TypeCode.UInt16,
        var t when t == typeof(string) => TypeCode.String,
        var t when t == typeof(DateTime) => TypeCode.DateTime,
        _ => throw new NotSupportedException($"Type {type} is not supported.")
    };

    static object? DeserializePrimitive(TypeCode typeCode, JsonElement element) => typeCode switch
    {
        TypeCode.Boolean => element.GetBoolean(),
        TypeCode.Byte => element.GetByte(),
        TypeCode.SByte => element.GetSByte(),
        TypeCode.Char => element.GetString()![0],
        TypeCode.Decimal => element.GetDecimal(),
        TypeCode.Double => element.GetDouble(),
        TypeCode.Single => element.GetSingle(),
        TypeCode.Int32 => element.GetInt32(),
        TypeCode.UInt32 => element.GetUInt32(),
        TypeCode.Int64 => element.GetInt64(),
        TypeCode.UInt64 => element.GetUInt64(),
        TypeCode.Int16 => element.GetInt16(),
        TypeCode.UInt16 => element.GetUInt16(),
        TypeCode.String => element.GetString(),
        TypeCode.DateTime => element.GetDateTime(),
        TypeCode.Object when element.ValueKind == JsonValueKind.String => Guid.Parse(element.GetString()!),
        TypeCode.Object => throw new JsonException("Expected string for Guid."),
        _ => throw new NotSupportedException($"TypeCode {typeCode} is not supported.")
    };
}