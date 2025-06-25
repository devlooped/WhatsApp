namespace Devlooped.WhatsApp;

/// <summary>
/// Provides an empty implementation of <see cref="IWhatsAppHandler"/>.
/// </summary>
public static class WhatsAppHandler
{
    /// <summary>
    /// An empty implementation of <see cref="IWhatsAppHandler"/> that is skipped 
    /// when building the processing pipeline so that normal processing continues 
    /// as if this handler was never added at all. It's useful to implement conditional 
    /// <c>Use(..)</c> logic that is dependent on runtime conditions, such as the 
    /// hosting environment or configuration settings, with zero impact on runtime 
    /// when the condition is not met.
    /// </summary>
    public static IWhatsAppHandler Continue { get; } = new ContinueWhatsAppHandler();

    /// <summary>
    /// An empty implementation of <see cref="IWhatsAppHandler"/> that stops further 
    /// execution of the pipeline and generates no responses.
    /// </summary>
    public static IWhatsAppHandler Stop { get; } = new StopWhatsAppHandler();

    class StopWhatsAppHandler : IWhatsAppHandler
    {
        public IAsyncEnumerable<Response> HandleAsync(IEnumerable<IMessage> messages, CancellationToken cancellation = default)
            => AsyncEnumerable.Empty<Response>();
    }

    class ContinueWhatsAppHandler : IWhatsAppHandler
    {
        public IAsyncEnumerable<Response> HandleAsync(IEnumerable<IMessage> messages, CancellationToken cancellation = default)
            => throw new NotSupportedException("This handler should never be invoked by the pipeline.");
    }
}
