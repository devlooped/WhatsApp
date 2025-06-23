namespace Devlooped.WhatsApp;

static class IgnoreMessagesExtensions
{
    /// <summary>
    /// Ignores status and unsupported messages.
    /// </summary>
    public static WhatsAppHandlerBuilder UseIgnore(this WhatsAppHandlerBuilder builder)
        => builder.Use((inner, services) => new IgnoreMessagesHandler(inner,
            message => message.Type != MessageType.Status && message.Type != MessageType.Unsupported));

    /// <summary>
    /// Ignores messages based on the provided filter function.
    /// </summary>
    public static WhatsAppHandlerBuilder UseIgnore(this WhatsAppHandlerBuilder builder, Func<IMessage, bool> filter)
        => builder.Use((inner, services) => new IgnoreMessagesHandler(inner, filter));

    class IgnoreMessagesHandler(IWhatsAppHandler inner, Func<IMessage, bool> filter) : DelegatingWhatsAppHandler(inner)
    {
        public override IAsyncEnumerable<Response> HandleAsync(IEnumerable<IMessage> messages, CancellationToken cancellation = default)
        {
            var filtered = messages.Where(filter).ToArray();
            // Skip inner handler altogether if no messages pass the filter.
            if (filtered.Length == 0)
                return AsyncEnumerable.Empty<Response>();

            return base.HandleAsync(filtered, cancellation);
        }
    }
}