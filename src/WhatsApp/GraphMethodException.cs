using System.Text.Json.Serialization;

namespace Devlooped.WhatsApp;

/// <summary>
/// Generic exception for Meta Graph API errors.
/// </summary>
public class GraphMethodException(string message, int code) : Exception(message)
{
    /// <summary>
    /// The error code returned by the API.
    /// </summary>
    public int Code { get; } = code;

    /// <summary>
    /// Optional error subcode returned by the API.
    /// </summary>
    [JsonPropertyName("error_subcode")]
    public int? Subcode { get; init; }

    /// <summary>
    /// Meta Graph API trace ID for the error.
    /// </summary>
    [JsonPropertyName("fbtrace_id")]
    public required string TraceId { get; init; }
}
