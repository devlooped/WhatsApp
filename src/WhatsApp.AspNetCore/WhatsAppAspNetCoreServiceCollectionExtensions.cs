using System.ComponentModel;
using Devlooped.WhatsApp;
using Microsoft.Extensions.DependencyInjection;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Provides extension methods for registering <see cref="IWhatsAppClient"/> and 
/// <see cref="IWhatsAppHandler"/> with a <see cref="IServiceCollection"/> for ASP.NET Core hosting.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WhatsAppAspNetCoreServiceCollectionExtensions
{
    /// <summary>Registers a singleton <see cref="IWhatsAppClient"/> and <see cref="IWhatsAppHandler"/> in the <see cref="IServiceCollection"/>.</summary>
    /// <param name="collection">The <see cref="IServiceCollection"/> to which the client should be added.</param>
    /// <param name="handler">The <see cref="IWhatsAppHandler"/> that handles incoming WhatsApp messages as the underlying backend.</param>
    /// <param name="lifetime">The service lifetime for the client and handler. Defaults to <see cref="ServiceLifetime.Singleton"/>.</param>
    /// <param name="configure">Optional configuration callback for <see cref="WhatsAppOptions"/>.</param>
    /// <returns>A <see cref="WhatsAppHandlerBuilder"/> that can be used to build a pipeline around the handler.</returns>
    public static WhatsAppHandlerBuilder AddWhatsApp(
        this IServiceCollection collection,
        IWhatsAppHandler handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        => collection.AddWhatsAppCore(handler, lifetime, configure);

    /// <summary>Registers a singleton <see cref="IWhatsAppClient"/> and <see cref="IWhatsAppHandler"/> in the <see cref="IServiceCollection"/>.</summary>
    /// <param name="collection">The <see cref="IServiceCollection"/> to which the client should be added.</param>
    /// <param name="handlerFactory">A callback that produces the inner <see cref="IWhatsAppHandler"/> that represents the underlying backend.</param>
    /// <param name="lifetime">The service lifetime for the client and handler. Defaults to <see cref="ServiceLifetime.Singleton"/>.</param>
    /// <param name="configure">Optional configuration callback for <see cref="WhatsAppOptions"/>.</param>
    /// <returns>A <see cref="WhatsAppHandlerBuilder"/> that can be used to build a pipeline around the handler.</returns>
    public static WhatsAppHandlerBuilder AddWhatsApp(
        this IServiceCollection collection,
        Func<IServiceProvider, IWhatsAppHandler> handlerFactory,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        => collection.AddWhatsAppCore(handlerFactory, lifetime, configure);

    /// <summary>
    /// Add WhatsApp services and use an already registered service that implements <see cref="IWhatsAppHandler"/>.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp(
        this IServiceCollection collection,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        => collection.AddWhatsAppCore(lifetime, configure);

    /// <summary>
    /// Configure the WhatsApp handler with a typed handler.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp<THandler>(
        this IServiceCollection collection,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where THandler : class, IWhatsAppHandler
        => collection.AddWhatsAppCore<THandler>(lifetime, configure);

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp(
        this IServiceCollection collection,
        Func<IServiceProvider, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        => collection.AddWhatsAppCore(handler, lifetime, configure);

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp(
        this IServiceCollection collection,
        Func<IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        => collection.AddWhatsAppCore(handler, lifetime, configure);

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate that receives a service.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp<TService>(
        this IServiceCollection collection,
        Func<TService, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where TService : notnull
        => collection.AddWhatsAppCore(handler, lifetime, configure);

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate that receives two services.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp<TService1, TService2>(
        this IServiceCollection collection,
        Func<TService1, TService2, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where TService1 : notnull
        where TService2 : notnull
        => collection.AddWhatsAppCore(handler, lifetime, configure);

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate that receives three services.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp<TService1, TService2, TService3>(
        this IServiceCollection collection,
        Func<TService1, TService2, TService3, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where TService1 : notnull
        where TService2 : notnull
        where TService3 : notnull
        => collection.AddWhatsAppCore(handler, lifetime, configure);

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate that receives four services.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp<TService1, TService2, TService3, TService4>(
        this IServiceCollection collection,
        Func<TService1, TService2, TService3, TService4, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where TService1 : notnull
        where TService2 : notnull
        where TService3 : notnull
        where TService4 : notnull
        => collection.AddWhatsAppCore(handler, lifetime, configure);

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate that receives five services.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp<TService1, TService2, TService3, TService4, TService5>(
        this IServiceCollection collection,
        Func<TService1, TService2, TService3, TService4, TService5, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where TService1 : notnull
        where TService2 : notnull
        where TService3 : notnull
        where TService4 : notnull
        where TService5 : notnull
        => collection.AddWhatsAppCore(handler, lifetime, configure);

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate that receives six services.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp<TService1, TService2, TService3, TService4, TService5, TService6>(
        this IServiceCollection collection,
        Func<TService1, TService2, TService3, TService4, TService5, TService6, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where TService1 : notnull
        where TService2 : notnull
        where TService3 : notnull
        where TService4 : notnull
        where TService5 : notnull
        where TService6 : notnull
        => collection.AddWhatsAppCore(handler, lifetime, configure);
}
