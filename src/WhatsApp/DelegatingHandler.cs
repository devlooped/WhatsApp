namespace Devlooped.WhatsApp;

/// <summary>
/// Provides an optional base class for an <see cref="IWhatsAppHandler"/> that passes through calls to another instance.
/// </summary>
/// <remarks>
/// This is recommended as a base type when building handlers that can be chained around an underlying <see cref="IWhatsAppHandler"/>.
/// The default implementation simply passes each call to the inner handler instance.
/// </remarks>
public class DelegatingHandler : IWhatsAppHandler, IDisposable
{
    /// <summary>
    /// Initializes a new instance of the <see cref="DelegatingHandler"/> class.
    /// </summary>
    /// <param name="innerHandler">The wrapped handler instance.</param>
    public DelegatingHandler(IWhatsAppHandler innerHandler)
        => InnerHandler = Throw.IfNull(innerHandler);

    /// <summary>Gets the inner <see cref="IWhatsAppHandler" />.</summary>
    protected IWhatsAppHandler InnerHandler { get; }

    /// <inheritdoc />
    public void Dispose()
    {
        Dispose(disposing: true);
        GC.SuppressFinalize(this);
    }

    /// <inheritdoc />
    public virtual Task HandleAsync(IEnumerable<Message> messages, CancellationToken cancellation = default) => InnerHandler.HandleAsync(messages, cancellation);

    /// <summary>Provides a mechanism for releasing unmanaged resources.</summary>
    /// <param name="disposing"><see langword="true"/> if being called from <see cref="Dispose()"/>; otherwise, <see langword="false"/>.</param>
    protected virtual void Dispose(bool disposing)
    {
        if (disposing && InnerHandler is IDisposable disposable)
        {
            disposable.Dispose();
        }
    }
}
