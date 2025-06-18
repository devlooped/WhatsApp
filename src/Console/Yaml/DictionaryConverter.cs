using System.Text.Json;
using System.Text.RegularExpressions;
using YamlDotNet.Serialization;

namespace Devlooped.WhatsApp;

/// <summary>
/// Provides conversion methods to and from the data format used by SAIJ.
/// </summary>
public static partial class DictionaryConverter
{
    static readonly ISerializer serializer = new SerializerBuilder()
        .WithTypeConverter(new YamlDictionaryConverter())
        .WithTypeConverter(new YamlListConverter())
        .WithTypeConverter(new YamlDateOnlyConverter())
        .ConfigureDefaultValuesHandling(DefaultValuesHandling.OmitNull | DefaultValuesHandling.OmitEmptyCollections)
        .Build();

    static readonly JsonSerializerOptions options = new()
    {
        Converters = { new JsonDictionaryConverter() },
        Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    };

    static readonly IDeserializer deserializer = new DeserializerBuilder().Build();

    public static Dictionary<string, object?>? Parse(string json)
        => JsonSerializer.Deserialize<Dictionary<string, object?>>(json, options);

    public static string ToYaml(this object? value)
    {
        if (value is null)
            return string.Empty;

        return ConvertUnicodeEscapes(serializer.Serialize(value).Trim());
    }

    public static Dictionary<string, object?> FromYaml(string yaml)
        => deserializer.Deserialize<Dictionary<string, object?>>(yaml);

    static string ConvertUnicodeEscapes(string input) => UnicodeEscapeExpr().Replace(input, match =>
    {
        var type = match.Groups[1].Value; // u or U
        var hex = match.Groups[2].Value;  // Hex digits (4 or 8)

        // Convert hex to integer
        var codePoint = int.Parse(hex, System.Globalization.NumberStyles.HexNumber);

        // Handle \uXXXX (4 digits) or \UXXXXXXXX (up to 8 digits)
        return char.ConvertFromUtf32(codePoint);
    });

    [GeneratedRegex(@"\\([uU])([0-9A-Fa-f]{4,8})")]
    private static partial Regex UnicodeEscapeExpr();
}
