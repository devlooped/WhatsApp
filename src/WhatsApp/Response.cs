using System.Text.Json.Serialization;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace Devlooped.WhatsApp;

/// <summary>
/// Base class for responses.
/// </summary>
public abstract partial record Response
{
    internal abstract Task SendAsync(IWhatsAppClient client, CancellationToken cancellation = default);
}