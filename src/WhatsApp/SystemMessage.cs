namespace Devlooped.WhatsApp;

/// <summary>
/// Base message class for messages that cannot be interacted with by the user.
/// </summary>
/// <param name="Id">The message identifier.</param>
/// <param name="To">The service that received the message from the Cloud API.</param>
/// <param name="From">The user that sent the message.</param>
/// <param name="Timestamp">Timestamp of the message.</param>
public abstract record SystemMessage(string Id, Service To, User From, long Timestamp) : Message(Id, To, From, Timestamp);
