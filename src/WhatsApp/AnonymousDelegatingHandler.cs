
namespace Devlooped.WhatsApp;

/// <summary>Represents a delegating chat client that wraps an inner handler with implementation provided by a delegate.</summary>
class AnonymousDelegatingHandler : DelegatingHandler
{
    /// <summary>The delegate to use as the implementation of <see cref="Handle"/>.</summary>
    readonly Func<IEnumerable<Message>, IWhatsAppHandler, CancellationToken, Task> handlerFunc;

    /// <summary>
    /// Initializes a new instance of the <see cref="AnonymousDelegatingChatClient"/> class.
    /// </summary>
    /// <param name="innerHandler">The inner handler.</param>
    /// <param name="handlerFunc">A delegate that provides the implementation for <see cref="HandleAsync"/></param>
    public AnonymousDelegatingHandler(
        IWhatsAppHandler innerHandler,
        Func<IEnumerable<Message>, IWhatsAppHandler, CancellationToken, Task> handlerFunc) : base(innerHandler)
        => this.handlerFunc = Throw.IfNull(handlerFunc);

    public override Task HandleAsync(IEnumerable<Message> messages, CancellationToken cancellation = default)
        => handlerFunc(messages, InnerHandler, cancellation);
}