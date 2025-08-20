using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;
using Devlooped.WhatsApp.Flows;
using Microsoft.Extensions.AI;

namespace Devlooped.WhatsApp;

/// <summary>
/// Provides a source-generated JSON serialization context for the built-in types in this library, 
/// optimized for web scenarios configured with custom serialization options.
/// </summary>
/// <remarks>
/// This context is used to enable high-performance JSON serialization and deserialization for the
/// specified types using source generation. It is configured with options such as snake_case property naming, 
/// ignoring null values when writing, and using string-based enum serialization. The context also 
/// supports case-insensitive property name matching and indented JSON output.
/// <para>
/// The <see cref="DefaultOptions"/> property provides a pre-configured instance of 
/// <see cref="JsonSerializerOptions"/> that aligns with the context's settings and also includes 
/// the <see cref="JavaScriptEncoder.UnsafeRelaxedJsonEscaping"/> encoder for web scenarios.
/// </para>
/// </remarks>
[JsonSourceGenerationOptions(JsonSerializerDefaults.Web,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    UseStringEnumConverter = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata
    )]
[JsonSerializable(typeof(Message))]
[JsonSerializable(typeof(ContentMessage))]
[JsonSerializable(typeof(ErrorMessage))]
[JsonSerializable(typeof(FlowDataRequest))]
[JsonSerializable(typeof(FlowToken))]
[JsonSerializable(typeof(InteractiveMessage))]
[JsonSerializable(typeof(ReactionMessage))]
[JsonSerializable(typeof(StatusMessage))]
[JsonSerializable(typeof(UnsupportedMessage))]
[JsonSerializable(typeof(MediaReference))]
[JsonSerializable(typeof(Conversation))]
[JsonSerializable(typeof(AdditionalPropertiesDictionary))]
[JsonSerializable(typeof(MessageTemplate))]
[JsonSerializable(typeof(TemplateParameter))]
[JsonSerializable(typeof(TemplateComponent))]
[JsonSerializable(typeof(HeaderComponent))]
[JsonSerializable(typeof(BodyComponent))]
[JsonSerializable(typeof(ButtonComponent))]
[JsonSerializable(typeof(ButtonParameter))]
[JsonSerializable(typeof(PayloadButtonParameter))]
[JsonSerializable(typeof(TextButtonParameter))]
[JsonSerializable(typeof(TextParameter))]
[JsonSerializable(typeof(CurrencyParameter))]
[JsonSerializable(typeof(DateTimeParameter))]
[JsonSerializable(typeof(ImageParameter))]
[JsonSerializable(typeof(VideoParameter))]
[JsonSerializable(typeof(DocumentParameter))]
[JsonSerializable(typeof(LocationParameter))]
[JsonSerializable(typeof(CallToActionResponse))]
[JsonSerializable(typeof(CallToFlowResponse))]
[JsonSerializable(typeof(FlowDataResponse))]
[JsonSerializable(typeof(ReactionResponse))]
[JsonSerializable(typeof(TemplateResponse))]
[JsonSerializable(typeof(TextResponse))]
[JsonSerializable(typeof(TypingResponse))]
public partial class JsonContext : JsonSerializerContext
{
    static readonly Lazy<JsonSerializerOptions> options = new(CreateDefaultOptions);

    /// <summary>
    /// Provides a pre-configured instance of <see cref="JsonSerializerOptions"/> that aligns with the context's settings.
    /// </summary>
    public static JsonSerializerOptions DefaultOptions { get => options.Value; }

    [UnconditionalSuppressMessage("AotAnalysis", "IL3050", Justification = "DefaultJsonTypeInfoResolver is only used when reflection-based serialization is enabled")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = "DefaultJsonTypeInfoResolver is only used when reflection-based serialization is enabled")]
    static JsonSerializerOptions CreateDefaultOptions()
    {
        JsonSerializerOptions options = new(Default.Options)
        {
            Converters =
            {
                new MessageTemplateConverter(),
                new HeaderConverter(),
                new BodyConverter(),
                new TemplateParameterConverter(),
                new TextParameterConverter(),
                new CurrencyParameterConverter(),
                new DateTimeParameterConverter(),
                new ImageParameterConverter(),
                new VideoParameterConverter(),
                new DocumentParameterConverter(),
                new LocationParameterConverter(),
                new ButtonParameterConverter(),
            },
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
        };

        if (JsonSerializer.IsReflectionEnabledByDefault)
        {
            // If reflection-based serialization is enabled by default, use it as a fallback for all other types.
            // Also turn on string-based enum serialization for all unknown enums.
            options.TypeInfoResolverChain.Add(new DefaultJsonTypeInfoResolver());
            //options.Converters.Add(new JsonStringEnumConverter());
        }

        options.MakeReadOnly();
        return options;
    }
}