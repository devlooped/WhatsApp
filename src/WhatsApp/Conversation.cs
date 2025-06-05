namespace Devlooped.WhatsApp;

/// <summary>
/// Represents a conversation consisting of a unique identifier, a phone number, a collection of messages, and a
/// timestamp.
/// </summary>
/// <remarks>This record is immutable and provides a structured way to store and access conversation details. The
/// <see cref="Messages"/> property contains all messages associated with the conversation, while the <see
/// cref="Timestamp"/> property represents the time the conversation was created or last updated, depending on the
/// context.</remarks>
/// <param name="Number">The phone number associated with the conversation. Cannot be null or empty.</param>
/// <param name="Id">The unique identifier for the conversation. Cannot be null or empty.</param>
/// <param name="Messages">A list of messages exchanged in the conversation. Cannot be null; may be empty if no messages exist.</param>
/// <param name="Timestamp">The timestamp of the conversation, represented as the number of milliseconds since the Unix epoch.</param>
public record Conversation(string Number, string Id, List<IMessage> Messages, long Timestamp);