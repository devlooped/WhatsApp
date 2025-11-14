using System.Text.Json.Serialization;

namespace Devlooped.WhatsApp;

public class WhatsAppException(string? message) : Exception(message)
{
    public string? Type { get; set; }
    public int? Code { get; set; }
    [JsonPropertyName("error_subcode")]
    public int? Subcode { get; set; }
    [JsonPropertyName("fbtrace_id")]
    public string? TraceId { get; set; }
    [JsonPropertyName("error_user_title")]
    public string? UserTitle { get; set; }
    [JsonPropertyName("error_user_msg")]
    public string? UserMessage { get; set; }

    public override string ToString() => $"{UserTitle ?? Message}: {UserMessage ?? (Type + ":" + Code + ":" + Subcode)}";
}
