using System.Text.Json;
using System.Text.Json.Serialization;

namespace Devlooped.WhatsApp;

/// <summary>Represents a WhatsApp message template.</summary>
/// <param name="Name">Template name</param>
/// <param name="Language">Template language</param>
/// <see cref="https://developers.facebook.com/docs/whatsapp/api/messages/message-templates#supported-languages"/>
/// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages/#template-object"/>
/// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages/#components-object"/>
[JsonConverter(typeof(MessageTemplateConverter))]
public record MessageTemplate(string Name, string Language)
{
    /// <summary>Optional template header.</summary>
    [JsonConverter(typeof(HeaderConverter))]
    public HeaderComponent? Header { get; init; }
    [JsonConverter(typeof(BodyConverter))]
    /// <summary>Optional template body.</summary>
    public BodyComponent? Body { get; init; }

    // New property for buttons
    public List<ButtonComponent>? Buttons { get; init; }
}

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

/// <summary>Base record for template components (header and body).</summary>
public abstract record TemplateComponent(string Type);

/// <summary>Base record for parameters within components.</summary>
[JsonConverter(typeof(TemplateParameterConverter))]
public abstract record TemplateParameter([property: JsonIgnore] string Type);

/// <summary>Positional or named text parameter.</summary>
/// <param name="Text">The parameter text to replace in the template.</param>
/// <param name="Name">Optional parameter name for named parameters.</param>
[JsonConverter(typeof(TextParameterConverter))]
public record TextParameter(string Text, [property: JsonPropertyName("parameter_name")] string? Name = null) : TemplateParameter("text")
{
    /// <summary>Creates a positional text parameter from the given text.</summary>
    public static implicit operator TextParameter(string text) => new(text);
}

/// <summary>For body only, positional.</summary>
[JsonConverter(typeof(CurrencyParameterConverter))]
public record CurrencyParameter(string FallbackValue, string Code, int Amount1000) : TemplateParameter("currency");

/// <summary>For body only, positional.</summary>
[JsonConverter(typeof(DateTimeParameterConverter))]
public record DateTimeParameter(string FallbackValue) : TemplateParameter("date_time");

/// <summary>Base class for media parameters (image, video, document) that support both ID and Link.</summary>
public abstract record MediaTemplateParameter : TemplateParameter
{
    protected MediaTemplateParameter(string type, string id) : base(type) => Id = id;
    protected MediaTemplateParameter(string type, Uri link) : base(type) => Link = link;

    /// <summary>Media ID.</summary>
    public string? Id { get; }
    /// <summary>Public URL of the media.</summary>
    public Uri? Link { get; }
}

/// <summary>Image template parameter, used in header component only.</summary>
[JsonConverter(typeof(ImageParameterConverter))]
public record ImageParameter : MediaTemplateParameter
{
    /// <summary>Image parameter from a previously uploaded media ID.</summary>
    public ImageParameter(string id) : base("image", id) { }
    /// <summary>Image parameter from a public URL.</summary>
    public ImageParameter(Uri link) : base("image", link) { }
}

/// <summary>Video template parameter, used in header component only.</summary>
[JsonConverter(typeof(VideoParameterConverter))]
public record VideoParameter : MediaTemplateParameter
{
    /// <summary>Video parameter from a previously uploaded media ID.</summary>
    public VideoParameter(string id) : base("video", id) { }
    /// <summary>Video parameter from a public URL.</summary>
    public VideoParameter(Uri link) : base("video", link) { }
}

/// <summary>Document template parameter, used in header component only.</summary>
[JsonConverter(typeof(DocumentParameterConverter))]
public record DocumentParameter : MediaTemplateParameter
{
    /// <summary>Document parameter from a previously uploaded media ID.</summary>
    public DocumentParameter(string id) : base("document", id) { }
    /// <summary>Document parameter from a public URL.</summary>
    public DocumentParameter(Uri link) : base("document", link) { }
}

/// <summary>Location template parameter, used in header component only.</summary>
[JsonConverter(typeof(LocationParameterConverter))]
public record LocationParameter(double Latitude, double Longitude, string Name, string Address) : TemplateParameter("location");

/// <summary>Header component in a template message.</summary>
[JsonConverter(typeof(HeaderConverter))]
public record HeaderComponent : TemplateComponent
{
    /// <summary>List of parameters in the header component.</summary>
    public List<TemplateParameter> Parameters { get; }

    /// <summary>Creates a header component with a location parameter.</summary>
    public HeaderComponent(LocationParameter location) : this([location]) { }
    /// <summary>Creates a header component with an image parameter.</summary>
    public HeaderComponent(ImageParameter image) : this([image]) { }
    /// <summary>Creates a header component with a video parameter.</summary>
    public HeaderComponent(VideoParameter video) : this([video]) { }
    /// <summary>Creates a header component with a document parameter.</summary>
    public HeaderComponent(DocumentParameter document) : this([document]) { }
    /// <summary>Creates a header component with text parameters.</summary>
    public HeaderComponent(params TextParameter[] parameters) : this(new List<TemplateParameter>(parameters)) { }

    /// <summary>Creates a header component with multiple parameters.</summary>
    /// <devdoc>Internal since not every combination of parameters is valid.</devdoc>
    internal HeaderComponent(List<TemplateParameter> parameters) : base("header") => Parameters = parameters;
}

/// <summary>Converter for HeaderComponent.</summary>
public class HeaderConverter : JsonConverter<HeaderComponent>
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

/// <summary>Supports text, currency and date_time parameters.</summary>
[JsonConverter(typeof(BodyConverter))]
public record BodyComponent(List<TemplateParameter> Parameters) : TemplateComponent("body");

/// <summary>Converter for BodyComponent.</summary>
public class BodyConverter : JsonConverter<BodyComponent>
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

// New enum for button sub-types
public enum ButtonSubType
{
    QuickReply,
    Url,
    Catalog
}

/// <summary>Base record for parameters within button components.</summary>
[JsonConverter(typeof(ButtonParameterConverter))]
public abstract record ButtonParameter(string Type)
{
    // add two factory methods for payload and text
    /// <summary>Creates a payload button parameter.</summary>
    public static PayloadButtonParameter CreatePayload(string payload) => new(payload);

    /// <summary>Creates a text button parameter.</summary>
    public static TextButtonParameter CreateText(string text) => new(text);
}

/// <summary>Converter for ButtonParameter subclasses.</summary>
public class ButtonParameterConverter : JsonConverter<ButtonParameter>
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

/// <summary>Payload parameter for quick_reply buttons.</summary>
public record PayloadButtonParameter(string Payload) : ButtonParameter("payload");

/// <summary>Text parameter for url buttons (suffix to append to URL).</summary>
public record TextButtonParameter(string Text) : ButtonParameter("text");

/// <summary>Button component in a template message.</summary>
public record ButtonComponent(ButtonSubType SubType, List<ButtonParameter>? Parameters = default) : TemplateComponent("button")
{
    /// <summary>Creates a default button component with sub-type <see cref="ButtonSubType.QuickReply"/> and no 
    /// button parameters. This maps to the default text buttons in the template definition.</summary>
    public static ButtonComponent Default { get; } = new(ButtonSubType.QuickReply);

    /// <summary>Optional index for the button, used to maintain order in the template. Defaults to its order in the list of <see cref="MessageTemplate.Buttons"/>.</summary>
    public int? Index { get; init; }

    /// <summary>Creates a catalog button component.</summary>
    public static ButtonComponent Catalog() => new(ButtonSubType.Catalog);
    /// <summary>Creates a quick-reply button component with a custom payload on user selection.</summary>
    public static ButtonComponent Payload(string payload) => new(ButtonSubType.QuickReply, [ButtonParameter.CreatePayload(payload)]);
    /// <summary>Creates a URL button component with the given text suffix.</summary>
    public static ButtonComponent Url(string suffix, int? index = null) => new(ButtonSubType.Url, [ButtonParameter.CreateText(suffix)]) { Index = index };
}
