namespace Devlooped.WhatsApp;

/// <summary>
/// WhatsApp end user that either originated a message or is the target of a message.
/// </summary>
/// <param name="Name">User's name</param>
/// <param name="Number">User's phone number</param>
public record User(string Name, string Number);
