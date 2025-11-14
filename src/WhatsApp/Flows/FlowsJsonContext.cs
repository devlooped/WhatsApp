using System.Diagnostics.CodeAnalysis;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Text.Json.Serialization.Metadata;

namespace Devlooped.WhatsApp.Flows;

[JsonSourceGenerationOptions(JsonSerializerDefaults.Web,
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    UseStringEnumConverter = true,
    UnmappedMemberHandling = JsonUnmappedMemberHandling.Skip,
    PropertyNameCaseInsensitive = true,
    PropertyNamingPolicy = JsonKnownNamingPolicy.SnakeCaseLower,
    WriteIndented = true,
    GenerationMode = JsonSourceGenerationMode.Metadata
    )]
[JsonSerializable(typeof(CreateFlowRequest))]
[JsonSerializable(typeof(CreateFlowResponse))]
[JsonSerializable(typeof(ValidationError))]
[JsonSerializable(typeof(ValidationPointer))]
[JsonSerializable(typeof(UpdateFlowMetadataRequest))]
[JsonSerializable(typeof(UpdateFlowJsonResponse))]
[JsonSerializable(typeof(GetFlowPreviewResponse))]
[JsonSerializable(typeof(SuccessResponse))]
[JsonSerializable(typeof(Flow))]
[JsonSerializable(typeof(Cursors))]
[JsonSerializable(typeof(Paging))]
[JsonSerializable(typeof(GetFlowsResponse))]
[JsonSerializable(typeof(FlowDetails))]
[JsonSerializable(typeof(FlowAsset))]
[JsonSerializable(typeof(GetFlowAssetsResponse))]
[JsonSerializable(typeof(MigratedFlow))]
[JsonSerializable(typeof(FailedFlow))]
partial class FlowsJsonContext : JsonSerializerContext
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
                // Required to parse flow preview expiresat since it uses '+0000'rather than extended ISO 8601-1:2019 (which requires a colon).
                new FlexibleDateTimeOffsetConverter()
            },
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            WriteIndented = true,
        };

        if (JsonSerializer.IsReflectionEnabledByDefault)
        {
            // If reflection-based serialization is enabled by default, use it as a fallback for all other types.
            // Also turn on string-based enum serialization for all unknown enums.
            options.TypeInfoResolverChain.Add(new DefaultJsonTypeInfoResolver());
        }

        options.MakeReadOnly();
        return options;
    }

    class FlexibleDateTimeOffsetConverter : JsonConverter<DateTimeOffset>
    {
        public override DateTimeOffset Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => System.DateTimeOffset.Parse(reader.GetString()!);

        public override void Write(Utf8JsonWriter writer, DateTimeOffset value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.ToString("O"));
    }
}