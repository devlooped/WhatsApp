namespace Devlooped.WhatsApp;

public static class WhatsAppHandlerExtensions
{
    public static Task HandleAsync(this IWhatsAppHandler handler, Message message, CancellationToken cancellation = default)
        => handler.HandleAsync([message], cancellation);
}
