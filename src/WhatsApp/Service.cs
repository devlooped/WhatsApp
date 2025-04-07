namespace Devlooped.WhatsApp;

/// <summary>
/// Meta-hosted service number.
/// </summary>
/// <param name="Id">The identifier for the number in WhatsApp Manager.</param>
/// <param name="Number">The phone number.</param>
public record Service(string Id, string Number);