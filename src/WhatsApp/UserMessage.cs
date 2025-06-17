namespace Devlooped.WhatsApp;

/// <summary>
/// Base message class for messages the user can interact with.
/// </summary>
/// <param name="Id">The message identifier.</param>
/// <param name="Service">The service that received the message from the Cloud API.</param>
/// <param name="User">The user that sent the message.</param>
/// <param name="Timestamp">Timestamp of the message.</param>
public abstract record UserMessage(string Id, Service Service, User User, long Timestamp) : Message(Id, Service, User, Timestamp);