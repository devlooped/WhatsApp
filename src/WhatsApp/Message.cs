using System;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace Devlooped.WhatsApp;

/// <summary>
/// Base class for WhatsApp Cloud API messages.
/// </summary>
/// <param name="Id">The message identifier.</param>
/// <param name="To">The service that received the message from the Cloud API.</param>
/// <param name="From">The user that sent the message.</param>
/// <param name="Timestamp">Timestamp of the message.</param>
public abstract partial record Message(string Id, Service To, User From, long Timestamp)
{
    /// <summary>
    /// Deserializes the given JSON string into a <see cref="Message"/> instance.
    /// </summary>
    /// <param name="json">The Cloud API JSON string.</param>
    /// <returns>The typed message, or null if the incoming JSON was null or empty, or it's an 
    /// unsupported message type (i.e. not <see cref="ContentMessage"/> nor <see cref="ErrorMessage"/>).</returns>
    public static async Task<Message?> DeserializeAsync(string json)
    {
        if (string.IsNullOrEmpty(json))
            return default;

        // NOTE: if we got a JQ-transformed payload, deserialization MUST work, or we have a bug.
        // So we don't try..catch things in that code path.

        var jq = await JQ.ExecuteAsync(json, ContentMessage.JQ);
        if (!string.IsNullOrEmpty(jq))
            return JsonSerializer.Deserialize(jq, MessageSerializerContext.Default.ContentMessage);

        jq = await JQ.ExecuteAsync(json, InteractiveMessage.JQ);
        if (!string.IsNullOrEmpty(jq))
            return JsonSerializer.Deserialize(jq, MessageSerializerContext.Default.InteractiveMessage);

        jq = await JQ.ExecuteAsync(json, ErrorMessage.JQ);
        if (!string.IsNullOrEmpty(jq))
            return JsonSerializer.Deserialize(jq, MessageSerializerContext.Default.ErrorMessage);

        jq = await JQ.ExecuteAsync(json, StatusMessage.JQ);
        if (!string.IsNullOrEmpty(jq))
            return JsonSerializer.Deserialize(jq, MessageSerializerContext.Default.StatusMessage);

        // NOTE: unsupported payloads would not generate a JQ output, so we can safely ignore them.
        return default;
    }

    /// <summary>
    /// Gets the type of message.
    /// </summary>
    public abstract MessageType Type { get; }

    [JsonSourceGenerationOptions(JsonSerializerDefaults.Web, WriteIndented = true, UseStringEnumConverter = true)]
    [JsonSerializable(typeof(ContentMessage))]
    [JsonSerializable(typeof(ErrorMessage))]
    [JsonSerializable(typeof(InteractiveMessage))]
    [JsonSerializable(typeof(StatusMessage))]
    partial class MessageSerializerContext : JsonSerializerContext { }
}
