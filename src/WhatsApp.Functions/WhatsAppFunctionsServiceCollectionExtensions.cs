using System.ComponentModel;
using Microsoft.Extensions.DependencyInjection;

namespace Devlooped.WhatsApp;

/// <summary>
/// Provides extension methods for registering WhatsApp services for Azure Functions.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WhatsAppFunctionsServiceCollectionExtensions
{
    /// <summary>Registers a singleton <see cref="IWhatsAppClient"/> and <see cref="IWhatsAppHandler"/> in the <see cref="IServiceCollection"/> for Azure Functions hosting.</summary>
    /// <param name="collection">The <see cref="IServiceCollection"/> to which the client should be added.</param>
    /// <param name="handler">The <see cref="IWhatsAppHandler"/> that handles incoming WhatsApp messages as the underlying backend.</param>
    /// <param name="lifetime">The service lifetime for the client and handler. Defaults to <see cref="ServiceLifetime.Singleton"/>.</param>
    /// <param name="configure">Optional configuration callback for <see cref="WhatsAppOptions"/>.</param>
    /// <returns>A <see cref="WhatsAppHandlerBuilder"/> that can be used to build a pipeline around the handler.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static WhatsAppHandlerBuilder AddWhatsApp(
        this IServiceCollection collection,
        IWhatsAppHandler handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
    {
        var builder = collection.AddWhatsAppCore(handler, lifetime, configure);
        collection.ConfigureAzureFunctions(builder, true);
        return builder;
    }

    /// <summary>Registers a singleton <see cref="IWhatsAppClient"/> and <see cref="IWhatsAppHandler"/> in the <see cref="IServiceCollection"/> for Azure Functions hosting.</summary>
    /// <param name="collection">The <see cref="IServiceCollection"/> to which the client should be added.</param>
    /// <param name="handlerFactory">A callback that produces the inner <see cref="IWhatsAppHandler"/> that represents the underlying backend.</param>
    /// <param name="lifetime">The service lifetime for the client and handler. Defaults to <see cref="ServiceLifetime.Singleton"/>.</param>
    /// <param name="configure">Optional configuration callback for <see cref="WhatsAppOptions"/>.</param>
    /// <returns>A <see cref="WhatsAppHandlerBuilder"/> that can be used to build a pipeline around the handler.</returns>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static WhatsAppHandlerBuilder AddWhatsApp(
        this IServiceCollection collection,
        Func<IServiceProvider, IWhatsAppHandler> handlerFactory,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
    {
        var builder = collection.AddWhatsAppCore(handlerFactory, lifetime, configure);
        collection.ConfigureAzureFunctions(builder, true);
        return builder;
    }

    /// <summary>
    /// Add WhatsApp services for Azure Functions and use an already registered service that implements <see cref="IWhatsAppHandler"/>.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static WhatsAppHandlerBuilder AddWhatsApp(
        this IServiceCollection collection,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
    {
        var builder = collection.AddWhatsAppCore(lifetime, configure);
        collection.ConfigureAzureFunctions(builder, true);
        return builder;
    }

    /// <summary>
    /// Configure the WhatsApp handler with a typed handler for Azure Functions hosting.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static WhatsAppHandlerBuilder AddWhatsApp<THandler>(
        this IServiceCollection collection,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where THandler : class, IWhatsAppHandler
    {
        var builder = collection.AddWhatsAppCore<THandler>(lifetime, configure);
        collection.ConfigureAzureFunctions(builder, true);
        return builder;
    }

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate for Azure Functions hosting.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static WhatsAppHandlerBuilder AddWhatsApp(
        this IServiceCollection collection,
        Func<IServiceProvider, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
    {
        var builder = collection.AddWhatsAppCore(handler, lifetime, configure);
        collection.ConfigureAzureFunctions(builder, true);
        return builder;
    }

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate for Azure Functions hosting.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static WhatsAppHandlerBuilder AddWhatsApp(
        this IServiceCollection collection,
        Func<IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
    {
        var builder = collection.AddWhatsAppCore(handler, lifetime, configure);
        collection.ConfigureAzureFunctions(builder, true);
        return builder;
    }

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate that receives a service for Azure Functions hosting.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static WhatsAppHandlerBuilder AddWhatsApp<TService>(
        this IServiceCollection collection,
        Func<TService, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where TService : notnull
    {
        var builder = collection.AddWhatsAppCore(handler, lifetime, configure);
        collection.ConfigureAzureFunctions(builder, true);
        return builder;
    }

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate that receives two services for Azure Functions hosting.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static WhatsAppHandlerBuilder AddWhatsApp<TService1, TService2>(
        this IServiceCollection collection,
        Func<TService1, TService2, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where TService1 : notnull
        where TService2 : notnull
    {
        var builder = collection.AddWhatsAppCore(handler, lifetime, configure);
        collection.ConfigureAzureFunctions(builder, true);
        return builder;
    }

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate that receives three services for Azure Functions hosting.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static WhatsAppHandlerBuilder AddWhatsApp<TService1, TService2, TService3>(
        this IServiceCollection collection,
        Func<TService1, TService2, TService3, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where TService1 : notnull
        where TService2 : notnull
        where TService3 : notnull
    {
        var builder = collection.AddWhatsAppCore(handler, lifetime, configure);
        collection.ConfigureAzureFunctions(builder, true);
        return builder;
    }

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate that receives four services for Azure Functions hosting.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static WhatsAppHandlerBuilder AddWhatsApp<TService1, TService2, TService3, TService4>(
        this IServiceCollection collection,
        Func<TService1, TService2, TService3, TService4, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where TService1 : notnull
        where TService2 : notnull
        where TService3 : notnull
        where TService4 : notnull
    {
        var builder = collection.AddWhatsAppCore(handler, lifetime, configure);
        collection.ConfigureAzureFunctions(builder, true);
        return builder;
    }

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate that receives five services for Azure Functions hosting.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
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
    {
        var builder = collection.AddWhatsAppCore(handler, lifetime, configure);
        collection.ConfigureAzureFunctions(builder, true);
        return builder;
    }

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate that receives six services for Azure Functions hosting.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
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
    {
        var builder = collection.AddWhatsAppCore(handler, lifetime, configure);
        collection.ConfigureAzureFunctions(builder, true);
        return builder;
    }
}
