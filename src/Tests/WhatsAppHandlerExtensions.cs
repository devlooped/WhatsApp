namespace Devlooped.WhatsApp;

public static class WhatsAppHandlerExtensions
{
    /// <summary>
    /// Test-only extension method to handle a single message and force enumeration of all 
    /// responses for the purpose of full pipeline execution only. It also sets the timestamp 
    /// of the message to the current UTC time.
    /// </summary>
    public static async Task HandleAsync(this IWhatsAppHandler handler, Message message, CancellationToken cancellation = default)
        => await handler.HandleAsync([message with { Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() }], cancellation)
            .ToListAsync(cancellation);
}