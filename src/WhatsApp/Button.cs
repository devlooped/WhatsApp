namespace Devlooped.WhatsApp;

/// <summary>
/// Represents a button in an interactive message sent to a user.
/// </summary>
/// <param name="Id">The button identifier.</param>
/// <param name="Title">The button title.</param>
public record Button(string Id, string Title);