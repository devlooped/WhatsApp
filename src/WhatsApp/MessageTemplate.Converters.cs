using System.Text.Json;
using System.Text.Json.Serialization;

namespace Devlooped.WhatsApp;

/// <summary>Converter for MessageTemplate.</summary>
class MessageTemplateConverter : JsonConverter<MessageTemplate>
{
    public override MessageTemplate Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        var name = root.GetProperty("name").GetString() ?? throw new JsonException("Missing 'name' property.");
        var languageCode = root.GetProperty("language").GetProperty("code").GetString() ?? throw new JsonException("Missing 'language.code' property.");

        HeaderComponent? header = null;
        BodyComponent? body = null;
        List<ButtonComponent> buttons = [];

        if (root.TryGetProperty("components", out var componentsElement))
        {
            foreach (var component in componentsElement.EnumerateArray())
            {
                var type = component.GetProperty("type").GetString();
                switch (type)
                {
                    case "header":
                        header = JsonSerializer.Deserialize<HeaderComponent>(component, options);
                        break;
                    case "body":
                        body = JsonSerializer.Deserialize<BodyComponent>(component, options);
                        break;
                    case "button":
                        var subType = component.GetProperty("sub_type").GetString() switch
                        {
                            null => throw new JsonException("Missing 'sub_type' property."),
                            "quick_reply" => ButtonSubType.QuickReply,
                            "url" => ButtonSubType.Url,
                            "catalog" => ButtonSubType.Catalog,
                            var other => throw new JsonException($"Unsupported button sub_type: {other}")
                        };

                        List<ButtonParameter> parameters = [];
                        if (component.TryGetProperty("parameters", out var parametersElement))
                        {
                            parameters = JsonSerializer.Deserialize<List<ButtonParameter>>(parametersElement, options) ?? [];
                        }

                        buttons.Add(new ButtonComponent(subType, parameters));
                        break;
                }
            }
        }

        return new MessageTemplate(name, languageCode)
        {
            Header = header,
            Body = body,
            Buttons = buttons.Count != 0 ? buttons : null
        };
    }

    public override void Write(Utf8JsonWriter writer, MessageTemplate value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("name", value.Name);

        writer.WritePropertyName("language");
        writer.WriteStartObject();
        writer.WriteString("code", value.Language);
        writer.WriteEndObject();

        if (value.Header is not null || value.Body is not null || value.Buttons is { Count: > 0 })
        {
            writer.WritePropertyName("components");
            writer.WriteStartArray();

            if (value.Header is not null)
                JsonSerializer.Serialize(writer, value.Header, options);

            if (value.Body is not null)
                JsonSerializer.Serialize(writer, value.Body, options);

            if (value.Buttons != null)
            {
                for (var i = 0; i < value.Buttons.Count; i++)
                {
                    var button = value.Buttons[i];
                    writer.WriteStartObject();
                    writer.WriteString("type", "button");
                    var subType = button.SubType switch
                    {
                        ButtonSubType.QuickReply => "quick_reply",
                        ButtonSubType.Url => "url",
                        ButtonSubType.Catalog => "catalog",
                        _ => throw new JsonException($"Unsupported ButtonSubType: {button.SubType}")
                    };
                    writer.WriteString("sub_type", subType);
                    writer.WriteNumber("index", button.Index ?? i);
                    writer.WritePropertyName("parameters");
                    JsonSerializer.Serialize(writer, button.Parameters ?? [], options);
                    writer.WriteEndObject();
                }
            }

            writer.WriteEndArray();
        }

        writer.WriteEndObject();
    }
}

/// <summary>Converter for HeaderComponent.</summary>
class HeaderConverter : JsonConverter<HeaderComponent>
{
    public override HeaderComponent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        // Verify this is a header component
        var type = root.GetProperty("type").GetString();
        if (type != "header")
            throw new JsonException($"Expected header component, got {type}");

        // Extract parameters if they exist
        List<TemplateParameter> parameters = [];
        if (root.TryGetProperty("parameters", out var parametersElement))
        {
            parameters = JsonSerializer.Deserialize<List<TemplateParameter>>(parametersElement, options) ?? [];
        }

        return new HeaderComponent(parameters);
    }

    public override void Write(Utf8JsonWriter writer, HeaderComponent value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("type", value.Type);
        writer.WritePropertyName("parameters");
        JsonSerializer.Serialize(writer, value.Parameters, options);
        writer.WriteEndObject();
    }
}

/// <summary>Converter for BodyComponent.</summary>
class BodyConverter : JsonConverter<BodyComponent>
{
    public override BodyComponent Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        // Verify this is a body component
        var type = root.GetProperty("type").GetString();
        if (type != "body")
            throw new JsonException($"Expected body component, got {type}");

        // Extract parameters if they exist
        List<TemplateParameter> parameters = [];
        if (root.TryGetProperty("parameters", out var parametersElement))
        {
            parameters = JsonSerializer.Deserialize<List<TemplateParameter>>(parametersElement, options) ?? [];
        }

        return new BodyComponent(parameters);
    }

    public override void Write(Utf8JsonWriter writer, BodyComponent value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("type", value.Type);
        writer.WritePropertyName("parameters");
        JsonSerializer.Serialize(writer, value.Parameters, options);
        writer.WriteEndObject();
    }
}

/// <summary>Converter for ButtonParameter subclasses.</summary>
class ButtonParameterConverter : JsonConverter<ButtonParameter>
{
    public override ButtonParameter? Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
    {
        using var document = JsonDocument.ParseValue(ref reader);
        var root = document.RootElement;

        var type = root.GetProperty("type").GetString() ?? throw new JsonException("Missing 'type' property in button parameter.");

        return type switch
        {
            "payload" => new PayloadButtonParameter(root.GetProperty("payload").GetString() ?? throw new JsonException("Missing 'payload' property.")),
            "text" => new TextButtonParameter(root.GetProperty("text").GetString() ?? throw new JsonException("Missing 'text' property.")),
            _ => throw new JsonException($"Unsupported button parameter type: {type}")
        };
    }

    public override void Write(Utf8JsonWriter writer, ButtonParameter value, JsonSerializerOptions options)
    {
        writer.WriteStartObject();
        writer.WriteString("type", value.Type);

        switch (value)
        {
            case PayloadButtonParameter payload:
                writer.WriteString("payload", payload.Payload);
                break;
            case TextButtonParameter text:
                writer.WriteString("text", text.Text);
                break;
            default:
                throw new JsonException($"Unsupported button parameter type: {value.GetType().Name}");
        }

        writer.WriteEndObject();
    }
}

/// <summary>
/// Base converter class for TemplateParameter types that can handle both polymorphic and concrete type scenarios.
/// </summary>
/// <typeparam name="T">The specific TemplateParameter type to convert</typeparam>
abstract class TemplateParameterConverter<T> : JsonConverter<T> where T : TemplateParameter
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
                writer.WriteString("text", text.Value);
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
class TemplateParameterConverter : TemplateParameterConverter<TemplateParameter>
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
class TextParameterConverter : TemplateParameterConverter<TextParameter>
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
class CurrencyParameterConverter : TemplateParameterConverter<CurrencyParameter>
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
class DateTimeParameterConverter : TemplateParameterConverter<DateTimeParameter>
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
abstract class MediaParameterConverter<T> : TemplateParameterConverter<T> where T : MediaTemplateParameter
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
class ImageParameterConverter : MediaParameterConverter<ImageParameter>
{
    protected override string PropertyName => "image";
    protected override ImageParameter CreateFromId(string id) => new(id);
    protected override ImageParameter CreateFromLink(Uri link) => new(link);
}

/// <summary>
/// Converter for VideoParameter.
/// </summary>
class VideoParameterConverter : MediaParameterConverter<VideoParameter>
{
    protected override string PropertyName => "video";
    protected override VideoParameter CreateFromId(string id) => new(id);
    protected override VideoParameter CreateFromLink(Uri link) => new(link);
}

/// <summary>
/// Converter for DocumentParameter.
/// </summary>
class DocumentParameterConverter : MediaParameterConverter<DocumentParameter>
{
    protected override string PropertyName => "document";
    protected override DocumentParameter CreateFromId(string id) => new(id);
    protected override DocumentParameter CreateFromLink(Uri link) => new(link);
}

/// <summary>
/// Converter for LocationParameter.
/// </summary>
class LocationParameterConverter : TemplateParameterConverter<LocationParameter>
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
