using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Nodes;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Devlooped.WhatsApp.Client;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    UseStringEnumConverter = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    WriteIndented = true
    )]
[JsonSerializable(typeof(ClientMessage))]
[JsonSerializable(typeof(ContentMessage))]
[JsonSerializable(typeof(ReactionMessage))]
[JsonSerializable(typeof(InteractiveMessage))]
partial class ClientContext : JsonSerializerContext
{
    static readonly Lazy<JsonSerializerOptions> options = new(() => CreateDefaultOptions());

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

enum MessageType
{
    Content = 1,
    Interactive = 2,
    Reaction = 4,
}

[JsonPolymorphic]
[JsonDerivedType(typeof(ContentMessage), "text")]
[JsonDerivedType(typeof(InteractiveMessage), "interactive")]
[JsonDerivedType(typeof(ReactionMessage), "reaction")]
abstract record ClientMessage
{
    public abstract MessageType Type { get; }
}

record ContentMessage(Context Context, Text Text) : ClientMessage
{
    public override MessageType Type => MessageType.Content;
    public override string ToString() => Text.Body;
}

record Text(string Body);

record ReactionMessage(Reaction Reaction) : ClientMessage
{
    public override MessageType Type => MessageType.Reaction;
    public override string ToString() => Reaction.Emoji;
}

record Reaction(string MessageId, string Emoji);

record InteractiveMessage(Context Context, Interactive Interactive) : ClientMessage
{
    public override MessageType Type => MessageType.Interactive;
    public override string ToString() => Interactive.Body.Text;
}

record Interactive(Body Body, JsonNode? Action);

record Body(string Text);

record Context(string MessageId);