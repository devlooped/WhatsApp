namespace Devlooped.WhatsApp;

class NullWhatsAppHandler : IWhatsAppHandler
{
    public static IWhatsAppHandler Default { get; } = new NullWhatsAppHandler();

    NullWhatsAppHandler() { }

    public Task HandleAsync(IEnumerable<Message> messages, CancellationToken cancellation = default) => Task.CompletedTask;
}
