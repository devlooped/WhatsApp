namespace Devlooped.WhatsApp;

/// <summary>
/// Meta-hosted service number.
/// </summary>
/// <param name="Id">The identifier for the number in WhatsApp Manager.</param>
/// <param name="Number">The phone number.</param>
public record Service(string Id, string Number);

static class ServiceExtensions
{
    /// <summary>
    /// Checks whether the given service number ID is a CLI local endpoint.
    /// </summary>
    public static bool IsCLI(this string serviceId)
        => serviceId.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase);
}