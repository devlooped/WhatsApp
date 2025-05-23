namespace Devlooped.WhatsApp;

/// <summary>
/// Provides an empty implementation of <see cref="IWhatsAppHandler"/>.
/// </summary>
public static class WhatsAppHandler
{
    /// <summary>
    /// An empty implementation of <see cref="IWhatsAppHandler"/> that does nothing.
    /// </summary>
    public static IWhatsAppHandler Empty { get; } = new EmptyWhatsAppHandler();

    class EmptyWhatsAppHandler : IWhatsAppHandler
    {
        public Task HandleAsync(IEnumerable<Message> messages, CancellationToken cancellation = default) => Task.CompletedTask;
    }
}
