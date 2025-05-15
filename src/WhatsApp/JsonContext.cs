using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Devlooped.WhatsApp;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    UseStringEnumConverter = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    WriteIndented = true
    )]
[JsonSerializable(typeof(Message))]
[JsonSerializable(typeof(ContentMessage))]
[JsonSerializable(typeof(ErrorMessage))]
[JsonSerializable(typeof(InteractiveMessage))]
[JsonSerializable(typeof(ReactionMessage))]
[JsonSerializable(typeof(StatusMessage))]
[JsonSerializable(typeof(UnsupportedMessage))]
[JsonSerializable(typeof(MediaReference))]
[JsonSerializable(typeof(GraphMethodException))]
[JsonSerializable(typeof(ErrorResponse))]
[JsonSerializable(typeof(SendResponse))]
[JsonSerializable(typeof(MessageId))]
partial class JsonContext : JsonSerializerContext
{
    static readonly Lazy<JsonSerializerOptions> options = new Lazy<JsonSerializerOptions>(() => CreateDefaultOptions());

    public static JsonSerializerOptions DefaultOptions { get => options.Value; }

    [UnconditionalSuppressMessage("AotAnalysis", "IL3050", Justification = "DefaultJsonTypeInfoResolver is only used when reflection-based serialization is enabled")]
    [UnconditionalSuppressMessage("ReflectionAnalysis", "IL2026", Justification = "DefaultJsonTypeInfoResolver is only used when reflection-based serialization is enabled")]
    static JsonSerializerOptions CreateDefaultOptions()
    {
        JsonSerializerOptions options = new(Default.Options)
        {
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
        };

        if (JsonSerializer.IsReflectionEnabledByDefault)
        {
            // If reflection-based serialization is enabled by default, use it as a fallback for all other types.
            // Also turn on string-based enum serialization for all unknown enums.
            options.TypeInfoResolverChain.Add(new DefaultJsonTypeInfoResolver());
            options.Converters.Add(new JsonStringEnumConverter());
        }

        options.MakeReadOnly();
        return options;
    }
}

record ErrorResponse(GraphMethodException Error);

record SendResponse(MessageId[] Messages);

record MessageId(string Id);