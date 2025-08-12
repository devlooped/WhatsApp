using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace Devlooped.WhatsApp.Flows;

/// <summary>An incoming flow data exchange message, which initiates or continues a flow session.</summary>
public record FlowDataRequest(
    [property: JsonPropertyName("service")] string ServiceId,
    [property: JsonPropertyName("user")] string UserNumber,
    FlowDataAction Action, string Screen, JsonElement Data,
    [property: JsonPropertyName("flow_token")] FlowToken Token) : IMessage
{
    /// <inheritdoc/>
    [JsonConverter(typeof(AdditionalPropertiesDictionaryConverter))]
    public AdditionalPropertiesDictionary? AdditionalProperties { get; set; }

    /// <summary>Gets the <see cref="Token"/>.<see cref="FlowToken.Flow"/> value.</summary>
    public string Flow => Token.Flow;

    public string Id => $"{Token.RawToken};ts:{Timestamp}";

    public long Timestamp { get; init; } = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();

    [JsonIgnore]
    public string? Context => default;

    [JsonIgnore]
    public MessageType Type => MessageType.FlowData;
}

/// <summary>Provides the <c>DataResponse</c> methods for a flow data exchange request.</summary>
public static class FlowDataRequestExtensions
{
    /// <summary>Creates a <see cref="FlowDataResponse"/> for the given <paramref name="message"/>.</summary>
    public static FlowDataResponse DataResponse(this FlowDataRequest message, string screen, JsonElement data)
       => new(message.ServiceId, message.UserNumber, screen, data);

    /// <summary>Creates a <see cref="FlowDataResponse"/> for the given <paramref name="message"/>.</summary>
    public static FlowDataResponse DataResponse<T>(this FlowDataRequest message, string screen, T data)
       => new(message.ServiceId, message.UserNumber, screen, JsonSerializer.SerializeToElement(data, JsonContext.DefaultOptions));
}

/// <summary>Represents a response to a flow data exchange request, which is consumed by the flow.</summary>
public record FlowDataResponse(string ServiceId, string UserNumber, string Screen, JsonElement Data) : Response(ServiceId, UserNumber)
{
    /// <devdoc>The flow response is not actually sent via the client, but rather processed by the webhook itself.</devdoc>
    protected override Task<string?> SendCoreAsync(IWhatsAppClient client, CancellationToken cancellation = default)
        => Task.FromResult<string?>(Ulid.NewUlid().ToString());
}