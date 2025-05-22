
namespace Devlooped.WhatsApp;

/// <summary>
/// Represents a delegating handler that wraps an inner handler with implementation provided by a delegate.
/// </summary>
/// <remarks>
/// Initializes a new instance of the <see cref="AnonymousDelegatingChatClient"/> class.
/// </remarks>
/// <param name="innerHandler">The inner handler.</param>
/// <param name="handlerFunc">A delegate that provides the implementation for <see cref="HandleAsync"/></param>
class AnonymousDelegatingWhatsAppHandler(
    IWhatsAppHandler innerHandler,
    Func<IEnumerable<Message>, IWhatsAppHandler, CancellationToken, IAsyncEnumerable<Response>> handlerFunc) : DelegatingWhatsAppHandler(innerHandler)
{
    /// <summary>The delegate to use as the implementation of <see cref="Handle"/>.</summary>
    readonly Func<IEnumerable<Message>, IWhatsAppHandler, CancellationToken, IAsyncEnumerable<Response>> handlerFunc = Throw.IfNull(handlerFunc);

    public override IAsyncEnumerable<Response> HandleAsync(IEnumerable<Message> messages, CancellationToken cancellation = default)
        => handlerFunc(messages, InnerHandler, cancellation);
}