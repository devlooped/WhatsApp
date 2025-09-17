using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.AI;

namespace Devlooped.WhatsApp;

/// <summary>
/// Base class for WhatsApp Cloud API messages.
/// </summary>
/// <param name="Id">The message identifier.</param>
/// <param name="Service">The service that received the message from the Cloud API.</param>
/// <param name="User">The user that sent the message.</param>
/// <param name="Timestamp">Timestamp of the message.</param>
[JsonPolymorphic]
[JsonDerivedType(typeof(ContentMessage), "content")]
[JsonDerivedType(typeof(ErrorMessage), "error")]
[JsonDerivedType(typeof(InteractiveMessage), "interactive")]
[JsonDerivedType(typeof(InteractiveFlowMessage), "flow")]
[JsonDerivedType(typeof(ReactionMessage), "reaction")]
[JsonDerivedType(typeof(StatusMessage), "status")]
[JsonDerivedType(typeof(UnsupportedMessage), "unsupported")]
public abstract partial record Message(string Id, Service Service, User User, long Timestamp) : IMessage
{
    /// <summary>For debugging purposes, exposes the original JSON used to deserialize this instance, if any.</summary>
    internal string? Json => AdditionalProperties?.GetValueOrDefault("__json") as string;

    /// <inheritdoc/>
    [JsonConverter(typeof(AdditionalPropertiesDictionaryConverter))]
    public AdditionalPropertiesDictionary? AdditionalProperties { get; set; }

    /// <summary>
    /// Optional related message identifier, such as message being replied 
    /// or reacted to, or a status message refers to, or the interactive 
    /// selection is a response to.
    /// </summary>
    /// <remarks>
    /// In a <see cref="StatusMessage"/>, the context equals the status ID which 
    /// in turn equals the message ID the status refers to.
    /// </remarks>
    public string? Context { get; init; }

    [JsonInclude]
    [JsonPropertyName("notification")]
    internal string? NotificationId { get; init; }

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

        var jq = await JQ.ExecuteAsync(json, ThisAssembly.Resources.Message.Text);
        if (!string.IsNullOrEmpty(jq))
        {
            var message = JsonSerializer.Deserialize<Message>(jq, JsonContext.DefaultOptions);

            if (message is not null)
            {
                message.AdditionalProperties ??= [];
                message.AdditionalProperties["__json"] = json;

                // Fix empty id for system messages that don't have a message id otherwise
                if (string.IsNullOrEmpty(message.Id))
                    message = message with { Id = Ulid.NewUlid().ToString() };
            }

            return message;
        }

        // NOTE: unsupported payloads would not generate a JQ output, so we can safely ignore them.
        return default;
    }

    /// <summary>
    /// Gets the type of message.
    /// </summary>
    [JsonIgnore]
    public abstract MessageType Type { get; }

    /// <inheritdoc/>
    string IMessage.UserNumber => User.Number;

    /// <inheritdoc/>
    string IMessage.ServiceId => Service.Id;
}