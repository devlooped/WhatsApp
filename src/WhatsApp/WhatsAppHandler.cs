namespace Devlooped.WhatsApp;

/// <summary>
/// Provides an empty implementation of <see cref="IWhatsAppHandler"/>.
/// </summary>
public static class WhatsAppHandler
{
    /// <summary>
    /// An empty implementation of <see cref="IWhatsAppHandler"/> that does nothing and 
    /// can be used to shortcircuit the processing of WhatsApp messages.
    /// </summary>
    public static IWhatsAppHandler Empty { get; } = new EmptyWhatsAppHandler();

    /// <summary>
    /// An empty implementation of <see cref="IWhatsAppHandler"/> that is skipped 
    /// when building the processing pipeline. It's useful to implement conditional 
    /// <c>Use(..)</c> logic that is dependent on runtime conditions, such as the 
    /// hosting environment or configuration settings.
    /// </summary>
    public static IWhatsAppHandler Skip { get; } = new SKipWhatsAppHandler();

    class EmptyWhatsAppHandler : IWhatsAppHandler
    {
        public IAsyncEnumerable<Response> HandleAsync(IEnumerable<IMessage> messages, CancellationToken cancellation = default)
            => AsyncEnumerable.Empty<Response>();
    }

    class SKipWhatsAppHandler : IWhatsAppHandler
    {
        public IAsyncEnumerable<Response> HandleAsync(IEnumerable<IMessage> messages, CancellationToken cancellation = default)
            => throw new NotSupportedException("This handler should never be invoked by the pipeline.");
    }
}
