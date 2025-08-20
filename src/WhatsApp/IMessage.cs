using System.Text.Json.Serialization;
using Devlooped.WhatsApp.Flows;
using Microsoft.Extensions.AI;

namespace Devlooped.WhatsApp;

/// <summary>
/// Represents a message exchanged in a communication system, serving as a base type for various message types. 
/// </summary>
/// <remarks>This interface is designed to support polymorphic serialization and deserialization of different
/// message types. Derived types are identified using JSON type discrimination, as specified by the <see
/// cref="JsonPolymorphic"/>  and <see cref="JsonDerivedTypeAttribute"/> annotations. Examples of derived types include
/// content messages,  error messages, and interactive messages.</remarks>
[JsonPolymorphic]
[JsonDerivedType(typeof(ContentMessage), "content")]
[JsonDerivedType(typeof(ErrorMessage), "error")]
[JsonDerivedType(typeof(InteractiveMessage), "interactive")]
[JsonDerivedType(typeof(ReactionMessage), "reaction")]
[JsonDerivedType(typeof(StatusMessage), "status")]
[JsonDerivedType(typeof(UnsupportedMessage), "unsupported")]
[JsonDerivedType(typeof(TextResponse), "response/text")]
[JsonDerivedType(typeof(TemplateResponse), "response/template")]
[JsonDerivedType(typeof(ReactionResponse), "response/reaction")]
[JsonDerivedType(typeof(TypingResponse), "response/typing")]
[JsonDerivedType(typeof(CallToActionResponse), "response/cta")]
[JsonDerivedType(typeof(AnonymousResponse), "response/dynamic")]
[JsonDerivedType(typeof(CallToFlowResponse), "response/flow")] // initiates flow
[JsonDerivedType(typeof(InteractiveFlowMessage), "flow")]      // flow final response
[JsonDerivedType(typeof(FlowDataRequest), "flow/int")]         // flow data_exchange input
[JsonDerivedType(typeof(FlowDataResponse), "flow/out")]        // flow data_exchange output
public interface IMessage
{
    /// <summary>Gets or sets any additional properties associated with the message.</summary>
    [JsonConverter(typeof(AdditionalPropertiesDictionaryConverter))]
    AdditionalPropertiesDictionary? AdditionalProperties { get; set; }

    /// <summary>
    /// Gets the message id.
    /// </summary>
    string Id { get; }

    /// <summary>
    /// Gets the phone number associated with the message sender.
    /// </summary>
    string UserNumber { get; }

    /// <summary>
    /// Gets the unique identifier for the service.
    /// </summary>
    string ServiceId { get; }

    /// <summary>
    /// Gets the timestamp representing the number of milliseconds since the Unix epoch (January 1, 1970, 00:00:00 UTC).
    /// </summary>
    long Timestamp { get; }

    /// <summary>
    /// Optional related message identifier, such as message being replied 
    /// or reacted to, or a status message refers to, or the interactive 
    /// selection is a response to.
    /// </summary>
    string? Context { get; }

    /// <summary>
    /// Gets the type of the message.
    /// </summary>
    MessageType Type { get; }
}