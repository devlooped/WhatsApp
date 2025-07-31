using System.Text.Json;
using System.Text.Json.Serialization;

namespace Devlooped.WhatsApp;

/// <summary>
/// Base converter class for TemplateParameter types that can handle both polymorphic and concrete type scenarios.
/// </summary>
/// <typeparam name="T">The specific TemplateParameter type to convert</typeparam>
public abstract class TemplateParameterConverter<T> : JsonConverter<T> where T : TemplateParameter
{
    /// <summary>
    /// Gets the JSON property name for this parameter type (e.g., "text", "currency", "image", etc.).
    /// Return null for types that don't have a specific property (like the base TemplateParameter).
    /// </summary>
    protected virtual string? PropertyName => null;

    public override bool CanConvert(Type typeToConvert) => typeof(T).IsAssignableFrom(typeToConvert);

    public override T? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        // For concrete types, we don't need to check the type property - we know what type we're deserializing to
        if (typeof(T) != typeof(TemplateParameter))
        {
            // If the converter specifies a PropertyName, extract that property and pass it to ReadConcrete
            if (PropertyName is not null)
            {
                var propertyElement = root.GetProperty(PropertyName);
                return ReadConcrete(propertyElement);
            }
            else
            {
                // For converters without a specific property (like polymorphic base), pass the root
                return ReadConcrete(root);
            }
        }

        // For the base TemplateParameter type, we need to check the type property for polymorphic deserialization
        var paramType = root.GetProperty("type").GetString() ?? throw new JsonException("Missing 'type' property.");
        return (T)ReadPolymorphic(root, paramType);
    }

    /// <summary>
    /// Reads the concrete parameter from the specified JsonElement.
    /// For types with PropertyName, this element will be the specific property's content.
    /// For types without PropertyName, this element will be the root JSON element.
    /// </summary>
    protected abstract T ReadConcrete(JsonElement element);

    protected virtual TemplateParameter ReadPolymorphic(JsonElement root, string paramType)
    {
        return paramType switch
        {
            "text" => ReadTextParameter(root),
            "currency" => ReadCurrencyParameter(root),
            "date_time" => ReadDateTimeParameter(root),
            "image" => ReadImageParameter(root),
            "video" => ReadVideoParameter(root),
            "document" => ReadDocumentParameter(root),
            "location" => ReadLocationParameter(root),
            _ => throw new JsonException($"Unsupported Parameter type: {paramType}")
        };
    }

    static TextParameter ReadTextParameter(JsonElement root)
    {
        var text = root.GetProperty("text").GetString() ?? throw new JsonException("Missing 'text' for TextParameter.");
        string? parameterName = null;
        if (root.TryGetProperty("parameter_name", out var nameElem))
        {
            parameterName = nameElem.GetString();
        }
        return new TextParameter(text, parameterName);
    }

    static CurrencyParameter ReadCurrencyParameter(JsonElement root)
    {
        var currencyObj = root.GetProperty("currency");
        var fallbackValue = currencyObj.GetProperty("fallback_value").GetString() ?? throw new JsonException("Missing 'fallback_value' for CurrencyParameter.");
        var code = currencyObj.GetProperty("code").GetString() ?? throw new JsonException("Missing 'code' for CurrencyParameter.");
        var amount1000 = currencyObj.GetProperty("amount_1000").GetInt32();
        return new CurrencyParameter(fallbackValue, code, amount1000);
    }

    static DateTimeParameter ReadDateTimeParameter(JsonElement root)
    {
        var dateTimeObj = root.GetProperty("date_time");
        var fallbackValue = dateTimeObj.GetProperty("fallback_value").GetString() ?? throw new JsonException("Missing 'fallback_value' for DateTimeParameter.");
        return new DateTimeParameter(fallbackValue);
    }

    static ImageParameter ReadImageParameter(JsonElement root)
    {
        return ReadMediaParameter(root, "image", (id) => new ImageParameter(id), (link) => new ImageParameter(link));
    }

    static VideoParameter ReadVideoParameter(JsonElement root)
    {
        return ReadMediaParameter(root, "video", (id) => new VideoParameter(id), (link) => new VideoParameter(link));
    }

    static DocumentParameter ReadDocumentParameter(JsonElement root)
    {
        return ReadMediaParameter(root, "document", (id) => new DocumentParameter(id), (link) => new DocumentParameter(link));
    }

    static TMedia ReadMediaParameter<TMedia>(JsonElement root, string propertyName, Func<string, TMedia> createFromId, Func<Uri, TMedia> createFromLink)
    {
        var mediaObj = root.GetProperty(propertyName);
        if (mediaObj.TryGetProperty("link", out var linkElem) && linkElem.GetString() is { } link)
        {
            return createFromLink(new Uri(link));
        }
        else if (mediaObj.TryGetProperty("id", out var idElem) && idElem.GetString() is { } id)
        {
            return createFromId(id);
        }
        else
        {
            throw new JsonException($"{typeof(TMedia).Name} requires either 'link' or 'id'.");
        }
    }

    static LocationParameter ReadLocationParameter(JsonElement root)
    {
        var locationObj = root.GetProperty("location");
        var latitude = locationObj.GetProperty("latitude").GetDouble();
        var longitude = locationObj.GetProperty("longitude").GetDouble();
        var name = locationObj.GetProperty("name").GetString() ?? throw new JsonException("Missing 'name' for LocationParameter.");
        var address = locationObj.GetProperty("address").GetString() ?? throw new JsonException("Missing 'address' for LocationParameter.");
        return new LocationParameter(latitude, longitude, name, address);
    }

    public override void Write(Utf8JsonWriter writer, T value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("type", value.Type);

        switch (value)
        {
            case TextParameter text:
                writer.WriteString("text", text.Text);
                if (text.Name is not null)
                {
                    writer.WriteString("parameter_name", text.Name);
                }
                break;

            case CurrencyParameter currency:
                writer.WritePropertyName("currency");
                writer.WriteStartObject();
                writer.WriteString("fallback_value", currency.FallbackValue);
                writer.WriteString("code", currency.Code);
                writer.WriteNumber("amount_1000", currency.Amount1000);
                writer.WriteEndObject();
                break;

            case DateTimeParameter dateTime:
                writer.WritePropertyName("date_time");
                writer.WriteStartObject();
                writer.WriteString("fallback_value", dateTime.FallbackValue);
                writer.WriteEndObject();
                break;

            case MediaTemplateParameter media:
                writer.WritePropertyName(value.Type);
                writer.WriteStartObject();
                if (media.Id is not null)
                {
                    writer.WriteString("id", media.Id);
                }
                else if (media.Link is not null)
                {
                    writer.WriteString("link", media.Link.AbsoluteUri);
                }
                else
                {
                    throw new JsonException($"Media parameter requires either Id or Link.");
                }
                writer.WriteEndObject();
                break;

            case LocationParameter location:
                writer.WritePropertyName("location");
                writer.WriteStartObject();
                writer.WriteNumber("latitude", location.Latitude);
                writer.WriteNumber("longitude", location.Longitude);
                writer.WriteString("name", location.Name);
                writer.WriteString("address", location.Address);
                writer.WriteEndObject();
                break;

            default:
                throw new JsonException($"Unsupported Parameter subtype: {value.GetType().Name}");
        }

        writer.WriteEndObject();
    }
}

/// <summary>
/// Polymorphic converter for the base TemplateParameter type.
/// </summary>
public class TemplateParameterConverter : TemplateParameterConverter<TemplateParameter>
{
    protected override TemplateParameter ReadConcrete(JsonElement root)
    {
        // For the base type, we need to check the type property
        var paramType = root.GetProperty("type").GetString() ?? throw new JsonException("Missing 'type' property.");
        return ReadPolymorphic(root, paramType);
    }
}

/// <summary>
/// Converter for TextParameter.
/// </summary>
public class TextParameterConverter : TemplateParameterConverter<TextParameter>
{
    protected override string? PropertyName => null; // TextParameter reads from root, not a sub-property

    protected override TextParameter ReadConcrete(JsonElement element)
    {
        var text = element.GetProperty("text").GetString() ?? throw new JsonException("Missing 'text' for TextParameter.");
        string? parameterName = null;
        if (element.TryGetProperty("parameter_name", out var nameElem))
        {
            parameterName = nameElem.GetString();
        }
        return new TextParameter(text, parameterName);
    }
}

/// <summary>
/// Converter for CurrencyParameter.
/// </summary>
public class CurrencyParameterConverter : TemplateParameterConverter<CurrencyParameter>
{
    protected override string PropertyName => "currency";

    protected override CurrencyParameter ReadConcrete(JsonElement currencyElement)
    {
        var fallbackValue = currencyElement.GetProperty("fallback_value").GetString() ?? throw new JsonException("Missing 'fallback_value' for CurrencyParameter.");
        var code = currencyElement.GetProperty("code").GetString() ?? throw new JsonException("Missing 'code' for CurrencyParameter.");
        var amount1000 = currencyElement.GetProperty("amount_1000").GetInt32();
        return new CurrencyParameter(fallbackValue, code, amount1000);
    }
}

/// <summary>
/// Converter for DateTimeParameter.
/// </summary>
public class DateTimeParameterConverter : TemplateParameterConverter<DateTimeParameter>
{
    protected override string PropertyName => "date_time";

    protected override DateTimeParameter ReadConcrete(JsonElement dateTimeElement)
    {
        var fallbackValue = dateTimeElement.GetProperty("fallback_value").GetString() ?? throw new JsonException("Missing 'fallback_value' for DateTimeParameter.");
        return new DateTimeParameter(fallbackValue);
    }
}

/// <summary>
/// Base converter for media parameters that handles shared link/id parsing logic.
/// </summary>
/// <typeparam name="T">The specific MediaTemplateParameter type to convert</typeparam>
public abstract class MediaParameterConverter<T> : TemplateParameterConverter<T> where T : MediaTemplateParameter
{
    /// <summary>
    /// Creates an instance from a media ID.
    /// </summary>
    /// <param name="id">The media ID</param>
    /// <returns>The media parameter instance</returns>
    protected abstract T CreateFromId(string id);

    /// <summary>
    /// Creates an instance from a media URL.
    /// </summary>
    /// <param name="link">The media URL</param>
    /// <returns>The media parameter instance</returns>
    protected abstract T CreateFromLink(Uri link);

    protected override T ReadConcrete(JsonElement mediaElement)
    {
        if (mediaElement.TryGetProperty("link", out var linkElem) && linkElem.GetString() is { } link)
        {
            return CreateFromLink(new Uri(link));
        }
        else if (mediaElement.TryGetProperty("id", out var idElem) && idElem.GetString() is { } id)
        {
            return CreateFromId(id);
        }
        else
        {
            throw new JsonException($"{typeof(T).Name} requires either 'link' or 'id'.");
        }
    }
}

/// <summary>
/// Converter for ImageParameter.
/// </summary>
public class ImageParameterConverter : MediaParameterConverter<ImageParameter>
{
    protected override string PropertyName => "image";
    protected override ImageParameter CreateFromId(string id) => new(id);
    protected override ImageParameter CreateFromLink(Uri link) => new(link);
}

/// <summary>
/// Converter for VideoParameter.
/// </summary>
public class VideoParameterConverter : MediaParameterConverter<VideoParameter>
{
    protected override string PropertyName => "video";
    protected override VideoParameter CreateFromId(string id) => new(id);
    protected override VideoParameter CreateFromLink(Uri link) => new(link);
}

/// <summary>
/// Converter for DocumentParameter.
/// </summary>
public class DocumentParameterConverter : MediaParameterConverter<DocumentParameter>
{
    protected override string PropertyName => "document";
    protected override DocumentParameter CreateFromId(string id) => new(id);
    protected override DocumentParameter CreateFromLink(Uri link) => new(link);
}

/// <summary>
/// Converter for LocationParameter.
/// </summary>
public class LocationParameterConverter : TemplateParameterConverter<LocationParameter>
{
    protected override string PropertyName => "location";

    protected override LocationParameter ReadConcrete(JsonElement locationElement)
    {
        var latitude = locationElement.GetProperty("latitude").GetDouble();
        var longitude = locationElement.GetProperty("longitude").GetDouble();
        var name = locationElement.GetProperty("name").GetString() ?? throw new JsonException("Missing 'name' for LocationParameter.");
        var address = locationElement.GetProperty("address").GetString() ?? throw new JsonException("Missing 'address' for LocationParameter.");
        return new LocationParameter(latitude, longitude, name, address);
    }
}
