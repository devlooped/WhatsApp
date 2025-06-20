namespace Devlooped.WhatsApp;

public static class WhatsAppHandlerExtensions
{
    /// <summary>
    /// Test-only extension method to handle a single message and force enumeration of all 
    /// responses for the purpose of full pipeline execution only. It also sets the timestamp 
    /// of the message to the current UTC time.
    /// </summary>
    public static Task HandleAsync(this IWhatsAppHandler handler, Message message, CancellationToken cancellation = default)
        => handler.HandleAsync([message with { Timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() }], cancellation)
            .ForEachAsync(x => { }, cancellation);
}