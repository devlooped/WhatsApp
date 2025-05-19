namespace Devlooped.WhatsApp;

/// <summary>
/// A handler that wraps an inner handler with implementation provided by a delegate.
/// </summary>
public class AnonymousWhatsAppHandler : IWhatsAppHandler
{
    readonly IServiceProvider services;
    readonly Func<IServiceProvider, IEnumerable<Message>, CancellationToken, Task> handler;

    AnonymousWhatsAppHandler(IServiceProvider services, Func<IServiceProvider, IEnumerable<Message>, CancellationToken, Task> handler)
        => (this.services, this.handler) = (Throw.IfNull(services), Throw.IfNull(handler));

    AnonymousWhatsAppHandler(IServiceProvider services, Func<IEnumerable<Message>, CancellationToken, Task> handler)
        : this(services, (_, messages, cancellation) => handler(messages, cancellation)) { }

    /// <inheritdoc />
    public Task HandleAsync(IEnumerable<Message> messages, CancellationToken cancellation = default)
        => handler(services, messages, cancellation);

    /// <summary>
    /// Creates a new instance of an <see cref="IWhatsAppHandler"/> with the specified service provider and message
    /// handler.
    /// </summary>
    public static IWhatsAppHandler Create(IServiceProvider services, Func<IEnumerable<Message>, CancellationToken, Task> handler)
        => new AnonymousWhatsAppHandler(services, handler);

    /// <summary>
    /// Creates a new instance of an <see cref="IWhatsAppHandler"/> with the specified service provider and message
    /// handler.
    /// </summary>
    public static IWhatsAppHandler Create(IServiceProvider services, Func<IServiceProvider, IEnumerable<Message>, CancellationToken, Task> handler)
        => new AnonymousWhatsAppHandler(services, handler);

    /// <summary>
    /// Creates a new instance of an <see cref="IWhatsAppHandler"/> using the specified service and message handler
    /// function.
    /// </summary>
    public static IWhatsAppHandler Create<TService>(TService service, Func<TService, IEnumerable<Message>, CancellationToken, Task> handler)
        => new AnonymousWhatsAppHandler1<TService>(service, handler);

    /// <summary>
    /// Creates a new instance of a WhatsApp message handler that processes messages using the specified services and
    /// handler function.
    /// </summary>
    public static IWhatsAppHandler Create<TService1, TService2>(TService1 service1, TService2 service2, Func<TService1, TService2, IEnumerable<Message>, CancellationToken, Task> handler)
        => new AnonymousWhatsAppHandler2<TService1, TService2>(service1, service2, handler);

    /// <summary>
    /// Creates a new instance of a WhatsApp message handler that processes messages using the specified services and
    /// handler function.
    /// </summary>
    public static IWhatsAppHandler Create<TService1, TService2, TService3>(TService1 service1, TService2 service2, TService3 service3, Func<TService1, TService2, TService3, IEnumerable<Message>, CancellationToken, Task> handler)
        => new AnonymousWhatsAppHandler3<TService1, TService2, TService3>(service1, service2, service3, handler);

    /// <summary>
    /// Creates a new instance of a WhatsApp message handler that processes messages using the specified services and
    /// handler function.
    /// </summary>
    public static IWhatsAppHandler Create<TService1, TService2, TService3, TService4>(TService1 service1, TService2 service2, TService3 service3, TService4 service4, Func<TService1, TService2, TService3, TService4, IEnumerable<Message>, CancellationToken, Task> handler)
        => new AnonymousWhatsAppHandler4<TService1, TService2, TService3, TService4>(service1, service2, service3, service4, handler);

    /// <summary>
    /// Creates a new instance of a WhatsApp message handler that processes messages using the specified services and
    /// handler function.
    /// </summary>
    public static IWhatsAppHandler Create<TService1, TService2, TService3, TService4, TService5>(TService1 service1, TService2 service2, TService3 service3, TService4 service4, TService5 service5, Func<TService1, TService2, TService3, TService4, TService5, IEnumerable<Message>, CancellationToken, Task> handler)
        => new AnonymousWhatsAppHandler5<TService1, TService2, TService3, TService4, TService5>(service1, service2, service3, service4, service5, handler);

    /// <summary>
    /// Creates a new instance of a WhatsApp message handler that processes messages using the specified services and
    /// handler function.
    /// </summary>
    public static IWhatsAppHandler Create<TService1, TService2, TService3, TService4, TService5, TService6>(TService1 service1, TService2 service2, TService3 service3, TService4 service4, TService5 service5, TService6 service6, Func<TService1, TService2, TService3, TService4, TService5, TService6, IEnumerable<Message>, CancellationToken, Task> handler)
        => new AnonymousWhatsAppHandler6<TService1, TService2, TService3, TService4, TService5, TService6>(service1, service2, service3, service4, service5, service6, handler);

    class AnonymousWhatsAppHandler1<TService>(TService service, Func<TService, IEnumerable<Message>, CancellationToken, Task> handler) : IWhatsAppHandler
    {
        public Task HandleAsync(IEnumerable<Message> messages, CancellationToken cancellation = default) => handler(service, messages, cancellation);
    }

    class AnonymousWhatsAppHandler2<TService1, TService2>(TService1 service1, TService2 service2, Func<TService1, TService2, IEnumerable<Message>, CancellationToken, Task> handler) : IWhatsAppHandler
    {
        public Task HandleAsync(IEnumerable<Message> messages, CancellationToken cancellation = default) => handler(service1, service2, messages, cancellation);
    }

    class AnonymousWhatsAppHandler3<TService1, TService2, TService3>(TService1 service1, TService2 service2, TService3 service3, Func<TService1, TService2, TService3, IEnumerable<Message>, CancellationToken, Task> handler) : IWhatsAppHandler
    {
        public Task HandleAsync(IEnumerable<Message> messages, CancellationToken cancellation = default) => handler(service1, service2, service3, messages, cancellation);
    }

    class AnonymousWhatsAppHandler4<TService1, TService2, TService3, TService4>(TService1 service1, TService2 service2, TService3 service3, TService4 service4, Func<TService1, TService2, TService3, TService4, IEnumerable<Message>, CancellationToken, Task> handler) : IWhatsAppHandler
    {
        public Task HandleAsync(IEnumerable<Message> messages, CancellationToken cancellation = default) => handler(service1, service2, service3, service4, messages, cancellation);
    }

    class AnonymousWhatsAppHandler5<TService1, TService2, TService3, TService4, TService5>(TService1 service1, TService2 service2, TService3 service3, TService4 service4, TService5 service5, Func<TService1, TService2, TService3, TService4, TService5, IEnumerable<Message>, CancellationToken, Task> handler) : IWhatsAppHandler
    {
        public Task HandleAsync(IEnumerable<Message> messages, CancellationToken cancellation = default) => handler(service1, service2, service3, service4, service5, messages, cancellation);
    }

    class AnonymousWhatsAppHandler6<TService1, TService2, TService3, TService4, TService5, TService6>(TService1 service1, TService2 service2, TService3 service3, TService4 service4, TService5 service5, TService6 service6, Func<TService1, TService2, TService3, TService4, TService5, TService6, IEnumerable<Message>, CancellationToken, Task> handler) : IWhatsAppHandler
    {
        public Task HandleAsync(IEnumerable<Message> messages, CancellationToken cancellation = default) => handler(service1, service2, service3, service4, service5, service6, messages, cancellation);
    }
}