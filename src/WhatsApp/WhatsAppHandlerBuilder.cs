namespace Devlooped.WhatsApp;

/// <summary>
/// Creates the handler pipeline using the given <paramref name="innerHandler"/> 
/// as the target (final) handler. 
/// </summary>
/// <param name="innerHandler">The handler that provides the target implementation.</param>
public class WhatsAppHandlerBuilder
{
    readonly Func<IServiceProvider, IWhatsAppHandler> handlerFactory;
    List<Func<IWhatsAppHandler, IServiceProvider, IWhatsAppHandler>>? factories;

    public WhatsAppHandlerBuilder() : this(_ => WhatsAppHandler.Empty)
    {
    }

    public WhatsAppHandlerBuilder(Func<IServiceProvider, IWhatsAppHandler> handlerFactory)
    {
        Throw.IfNull(handlerFactory);
        this.handlerFactory = handlerFactory;
    }

    public IWhatsAppHandler Build(IServiceProvider? services = default)
    {
        services ??= ServiceProvider.Empty;
        var handler = handlerFactory(services);

        // Matches behavior of M.E.AI chat client builder
        // Apply the factories in reverse order, so that the first factory added is the outermost.
        if (factories is not null)
        {
            for (var i = factories.Count - 1; i >= 0; i--)
            {
                handler = factories[i](handler!, services);
                if (handler is null)
                {
                    Throw.InvalidOperationException(
                        $"The {nameof(WhatsAppHandlerBuilder)} entry at index {i} returned null. " +
                        $"Ensure that the callbacks passed to {nameof(Use)} return non-null {nameof(IWhatsAppHandler)} instances.");
                }
            }
        }

        return handler!;
    }

    /// <summary>Adds a factory for an intermediate handler to the handler pipeline.</summary>
    /// <param name="handlerFactory">The handler factory function.</param>
    /// <returns>The updated <see cref="WhatsAppHandlerBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="handlerFactory"/> is <see langword="null"/>.</exception>
    public WhatsAppHandlerBuilder Use(Func<IWhatsAppHandler, IWhatsAppHandler> handlerFactory)
    {
        _ = Throw.IfNull(handlerFactory);

        return Use((innerClient, _) => handlerFactory(innerClient));
    }

    /// <summary>Adds a factory for an intermediate chat client to the chat client pipeline.</summary>
    /// <param name="handlerFactory">The handler factory function.</param>
    /// <returns>The updated <see cref="WhatsAppHandlerBuilder"/> instance.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="handlerFactory"/> is <see langword="null"/>.</exception>
    public WhatsAppHandlerBuilder Use(Func<IWhatsAppHandler, IServiceProvider, IWhatsAppHandler> handlerFactory)
    {
        _ = Throw.IfNull(handlerFactory);

        (factories ??= []).Add(handlerFactory);
        return this;
    }

    /// <summary>
    /// Adds to the handler pipeline an anonymous delegating handler based on a delegate that provides
    /// an implementation for <see cref="IWhatsAppHandler.HandleAsync(Message, CancellationToken)"/>.
    /// </summary>
    /// <param name="handlerFunc">A delegate that provides the implementation for <see cref="IWhatsAppHandler.HandleAsync"/></param>
    /// <returns>The updated <see cref="WhatsAppHandlerBuilder"/> instance.</returns>
    /// <remarks>
    /// This overload can be used when the anonymous implementation needs to provide pre-processing and/or post-processing, but doesn't
    /// need to interact with the results of the operation, which will come from the inner client.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="handlerFunc"/> is <see langword="null"/>.</exception>
    public WhatsAppHandlerBuilder Use(Func<IEnumerable<Message>, IWhatsAppHandler, CancellationToken, Task> handlerFunc)
    {
        _ = Throw.IfNull(handlerFunc);

        return Use((innerClient, _) => new AnonymousDelegatingWhatsAppHandler(innerClient, handlerFunc));
    }

    class ServiceProvider : IServiceProvider
    {
        public static IServiceProvider Empty { get; } = new ServiceProvider();
        ServiceProvider() { }
        public object? GetService(Type serviceType) => null;
    }
}
