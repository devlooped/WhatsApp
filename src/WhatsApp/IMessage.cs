using System.Text.Json.Serialization;

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
public interface IMessage
{
    /// <summary>
    /// Gets the phone number associated with the message sender.
    /// </summary>
    string Number { get; }

    /// <summary>
    /// Gets the message id.
    /// </summary>
    string Id { get; }
}