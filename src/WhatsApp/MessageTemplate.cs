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

/// <summary>Base record for template components (header and body).</summary>
public abstract record TemplateComponent(string Type);

/// <summary>Base record for parameters within components.</summary>
[JsonConverter(typeof(TemplateParameterConverter))]
public abstract record TemplateParameter([property: JsonIgnore] string Type)
{
    /// <summary>Creates a text parameter (positionally or named if a name is provided).</summary>
    public static TextParameter Text(string text, string? name = null) => new(text, name);

    /// <summary>Creates a currency parameter with the given fallback value, code and amount.</summary>
    public static CurrencyParameter Currency(string fallbackValue, string code, int amount1000) => new(fallbackValue, code, amount1000);

    /// <summary>Creates a date_time parameter with the given fallback value.</summary>
    public static DateTimeParameter DateTime(string fallbackValue) => new(fallbackValue);

    /// <summary>Creates a location parameter with latitude, longitude, name and address.</summary>
    public static LocationParameter Location(double latitude, double longitude, string name, string address) => new(latitude, longitude, name, address);

    /// <summary>Creates an image parameter from a media ID or public URL.</summary>
    public static ImageParameter Image(string id) => new(id);

    /// <summary>Creates an image parameter from a public URL.</summary>
    public static ImageParameter Image(Uri link) => new(link);

    /// <summary>Creates a video parameter from a media ID or public URL.</summary>
    public static VideoParameter Video(string id) => new(id);

    /// <summary>Creates a video parameter from a public URL.</summary>
    public static VideoParameter Video(Uri link) => new(link);

    /// <summary>Creates a document parameter from a media ID or public URL.</summary>
    public static DocumentParameter Document(string id) => new(id);

    /// <summary>Creates a document parameter from a public URL.</summary>
    public static DocumentParameter Document(Uri link) => new(link);
}

/// <summary>Positional or named text parameter.</summary>
/// <param name="Value">The parameter text to replace in the template.</param>
/// <param name="Name">Optional parameter name for named parameters.</param>
[JsonConverter(typeof(TextParameterConverter))]
public record TextParameter(string Value, [property: JsonPropertyName("parameter_name")] string? Name = null) : TemplateParameter("text")
{
    /// <summary>Creates a positional text parameter from the given text.</summary>
    public static implicit operator TextParameter(string value) => new(value);
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

/// <summary>Supports text, currency and date_time parameters.</summary>
[JsonConverter(typeof(BodyConverter))]
public record BodyComponent(List<TemplateParameter> Parameters) : TemplateComponent("body");

/// <summary>New enum for button sub-types.</summary>
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
