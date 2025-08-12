using System.Diagnostics.CodeAnalysis;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace Devlooped.WhatsApp.Flows;

/// <summary>A wrapper around a flow token that encodes required information to identify the flow, user, service and any additional data required by the service.</summary>
[JsonConverter(typeof(FlowTokenConverter))]
public class FlowToken
{
    FlowToken(IDictionary<string, string> data, string raw)
        => (Data, RawToken) = (data.AsReadOnly(), raw);

    /// <summary>Gets the raw key-value data contained in the token.</summary>
    public IReadOnlyDictionary<string, string> Data { get; }

    /// <summary>Gets the original token content used to decode this instance.</summary>
    public string RawToken { get; }

    /// <summary>Gets the service identifier from the token data.</summary>
    public string ServiceId => Data.TryGetValue("service", out var service) ? service : throw new KeyNotFoundException("service");

    /// <summary>Gets the user phone number from the token data.</summary>
    public string UserNumber => Data.TryGetValue("user", out var user) ? user : throw new KeyNotFoundException("user");

    /// <summary>Gets the flow identifier or name from the token data.</summary>
    public string Flow => Data.TryGetValue("flow", out var flow) ? flow : throw new KeyNotFoundException("flow");

    /// <summary>Encodes the given response message as a token for use when starting a flow.</summary>
    public static string Encode(CallToFlowResponse message)
    {
        var sb = new StringBuilder()
            .Append("service:").Append(message.ServiceId).Append(';')
            .Append("user:").Append(message.UserNumber).Append(';')
            .Append("flow:").Append(message.Flow.Name ?? message.Flow.Id?.ToString() ?? throw new ArgumentException("Either flow name or id is required"));

        if (!string.IsNullOrEmpty(message.Flow.Token))
            sb.Append(';').Append("token:").Append(message.Flow.Token);

        return sb.ToString();
    }

    public static FlowToken Decode(string token) => TryDecode(token, out var flow) ? flow : throw new FormatException("Invalid flow token format.");

    /// <summary>Attempts to decode the given token string into a <see cref="FlowToken"/> instance.</summary>
    public static bool TryDecode(string token, [NotNullWhen(true)] out FlowToken? flow)
    {
        var parts = token.Split(';', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
        var dict = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
        foreach (var part in parts)
        {
            var kv = part.Split(':', 2);
            if (kv.Length == 2)
                dict[kv[0]] = kv[1];
        }

        if (!dict.ContainsKey("service") || !dict.ContainsKey("user") || !dict.ContainsKey("flow"))
        {
            flow = null;
            return false;
        }

        flow = new FlowToken(dict, token);
        return true;
    }

    class FlowTokenConverter : JsonConverter<FlowToken>
    {
        public override FlowToken Read(ref Utf8JsonReader reader, Type typeToConvert, JsonSerializerOptions options)
            => reader.TokenType != JsonTokenType.String || !TryDecode(reader.GetString()!, out var token) ?
                throw new JsonException("Invalid flow token format.") : token;

        public override void Write(Utf8JsonWriter writer, FlowToken value, JsonSerializerOptions options)
            => writer.WriteStringValue(value.RawToken);
    }
}
