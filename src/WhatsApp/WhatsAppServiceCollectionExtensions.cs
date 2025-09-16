using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Devlooped.WhatsApp;

/// <summary>
/// Provides extension methods for registering <see cref="IWhatsAppClient"/> and 
/// <see cref="IWhatsAppHandler"/> with a <see cref="IServiceCollection"/>.
/// </summary>
public static class WhatsAppServiceCollectionExtensions
{
    /// <summary>Registers a singleton <see cref="IWhatsAppClient"/> and <see cref="IWhatsAppHandler"/> in the <see cref="IServiceCollection"/>.</summary>
    /// <param name="collection">The <see cref="IServiceCollection"/> to which the client should be added.</param>
    /// <param name="handler">The <see cref="IWhatsAppHandler"/> that handles incoming WhatsApp messages as the underlying backend.</param>
    /// <param name="lifetime">The service lifetime for the client and handler. Defaults to <see cref="ServiceLifetime.Singleton"/>.</param>
    /// <returns>A <see cref="WhatsAppHandlerBuilder"/> that can be used to build a pipeline around the handler.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/>.</exception> 
    public static WhatsAppHandlerBuilder AddWhatsApp(
        this IServiceCollection collection,
        IWhatsAppHandler handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
    {
        Throw.IfNull(collection);
        Throw.IfNull(handler);

        return AddWhatsApp(collection, _ => handler, lifetime, configure);
    }

    /// <summary>Registers a singleton <see cref="IWhatsAppClient"/> and <see cref="IWhatsAppHandler"/> in the <see cref="IServiceCollection"/>.</summary>
    /// <param name="collection">The <see cref="IServiceCollection"/> to which the client should be added.</param>
    /// <param name="handlerFactory">A callback that produces the inner <see cref="IWhatsAppHandler"/> that represents the underlying backend.</param>
    /// <param name="lifetime">The service lifetime for the client and handler. Defaults to <see cref="ServiceLifetime.Singleton"/>.</param>
    /// <returns>A <see cref="WhatsAppHandlerBuilder"/> that can be used to build a pipeline around the handler.</returns>
    /// <exception cref="ArgumentNullException"><paramref name="collection"/> is <see langword="null"/>.</exception>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/>.</exception> 
    public static WhatsAppHandlerBuilder AddWhatsApp(
        this IServiceCollection collection,
        Func<IServiceProvider, IWhatsAppHandler> handlerFactory,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
    {
        _ = Throw.IfNull(collection);
        _ = Throw.IfNull(handlerFactory);

        // Create builder
        var builder = new WhatsAppHandlerBuilder(handlerFactory, collection);

        // Configure default services
        ConfigureServices(collection, builder, lifetime, configure);

        return builder;
    }

    /// <summary>
    /// Add WhatsApp functions and use an already registered service that implements <see cref="IWhatsAppHandler"/>.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp(
        this IServiceCollection collection,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        => collection.AddWhatsApp(services => services.GetRequiredService<IWhatsAppHandler>(), lifetime, configure);

    /// <summary>
    /// Configure the WhatsApp handler for Azure Functions.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp<THandler>(
        this IServiceCollection collection,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where THandler : class, IWhatsAppHandler
    {
        if (collection.FirstOrDefault(x => x.ServiceType == typeof(THandler)) == null)
        {
            collection.Add(new ServiceDescriptor(typeof(THandler), typeof(THandler), lifetime));
        }

        return collection.AddWhatsApp(services => services.GetRequiredService<THandler>(), lifetime, configure);
    }

    /// <summary>
    /// Configure the WhatsApp handler for Azure Functions.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp(
        this IServiceCollection collection,
        Func<IServiceProvider, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
    {
        return collection.AddWhatsApp(
            services => AnonymousWhatsAppHandler.Create(services, handler), lifetime, configure);
    }

    /// <summary>
    /// Configure the WhatsApp handler for Azure Functions.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp(
        this IServiceCollection collection,
        Func<IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
    {
        return collection.AddWhatsApp(
            services => AnonymousWhatsAppHandler.Create(services, handler), lifetime, configure);
    }

    /// <summary>
    /// Configure the WhatsApp handler for Azure Functions.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp<TService>(
        this IServiceCollection collection,
        Func<TService, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where TService : notnull
    {
        return collection.AddWhatsApp(
            services => AnonymousWhatsAppHandler.Create(services.GetRequiredService<TService>(), handler), lifetime, configure);
    }

    /// <summary>
    /// Configure the WhatsApp handler for Azure Functions.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp<TService1, TService2>(
        this IServiceCollection collection,
        Func<TService1, TService2, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where TService1 : notnull
        where TService2 : notnull
    {
        return collection.AddWhatsApp(
            services => AnonymousWhatsAppHandler.Create(
                services.GetRequiredService<TService1>(),
                services.GetRequiredService<TService2>(),
                handler), lifetime, configure);
    }

    /// <summary>
    /// Configure the WhatsApp handler for Azure Functions.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp<TService1, TService2, TService3>(
        this IServiceCollection collection,
        Func<TService1, TService2, TService3, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where TService1 : notnull
        where TService2 : notnull
        where TService3 : notnull
    {
        return collection.AddWhatsApp(
            services => AnonymousWhatsAppHandler.Create(
                services.GetRequiredService<TService1>(),
                services.GetRequiredService<TService2>(),
                services.GetRequiredService<TService3>(),
                handler), lifetime, configure);
    }

    /// <summary>
    /// Configure the WhatsApp handler for Azure Functions.
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
    {
        return collection.AddWhatsApp(
            services => AnonymousWhatsAppHandler.Create(
                services.GetRequiredService<TService1>(),
                services.GetRequiredService<TService2>(),
                services.GetRequiredService<TService3>(),
                services.GetRequiredService<TService4>(),
                handler), lifetime, configure);
    }

    /// <summary>
    /// Configure the WhatsApp handler for Azure Functions.
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
    {
        return collection.AddWhatsApp(
            services => AnonymousWhatsAppHandler.Create(
                services.GetRequiredService<TService1>(),
                services.GetRequiredService<TService2>(),
                services.GetRequiredService<TService3>(),
                services.GetRequiredService<TService4>(),
                services.GetRequiredService<TService5>(),
                handler), lifetime, configure);
    }

    /// <summary>
    /// Configure the WhatsApp handler for Azure Functions.
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
    {
        return collection.AddWhatsApp(
            services => AnonymousWhatsAppHandler.Create(
                services.GetRequiredService<TService1>(),
                services.GetRequiredService<TService2>(),
                services.GetRequiredService<TService3>(),
                services.GetRequiredService<TService4>(),
                services.GetRequiredService<TService5>(),
                services.GetRequiredService<TService6>(),
                handler), lifetime, configure);
    }
    static WhatsAppHandlerBuilder ConfigureServices(IServiceCollection services, WhatsAppHandlerBuilder builder, ServiceLifetime lifetime, Action<WhatsAppOptions>? configure)
    {
        services.AddHttpClient("whatsapp").AddStandardResilienceHandler();
        services.AddHybridCache();
        services.AddSingleton<Idempotency>();

        if (services.FirstOrDefault(x => x.ServiceType == typeof(IWhatsAppClient)) == null)
            services.Add(new ServiceDescriptor(typeof(IWhatsAppClient), typeof(WhatsAppClient), lifetime));

        if (services.FirstOrDefault(x => x.ServiceType == typeof(TableServiceClient)) == null)
        {
            services.AddSingleton(services => new TableServiceClient(
                services.GetRequiredService<IConfiguration>()["AzureWebJobsStorage"]!,
                new TableClientOptions
                {
#if DEBUG
                    Diagnostics =
                    {
                        IsLoggingEnabled = true,
                        IsLoggingContentEnabled = true,
                    },
#endif
                }));
        }

        if (services.FirstOrDefault(x => x.ServiceType == typeof(CloudStorageAccount)) == null)
        {
            services.AddSingleton(services => CloudStorageAccount.Parse(
                services.GetRequiredService<IConfiguration>()["AzureWebJobsStorage"]!));
        }

        services.AddOptions<MetaOptions>()
            .BindConfiguration("Meta")
            .ValidateDataAnnotations();

        var options = services.AddOptions<WhatsAppOptions>()
            .BindConfiguration("WhatsApp");

        if (configure != null)
            options.Configure(configure);

        services.Add(new ServiceDescriptor(typeof(IWhatsAppHandler), builder.Build, lifetime));
        services.Add(new ServiceDescriptor(typeof(PipelineRunner), typeof(PipelineRunner), lifetime));

        // By default we use the queue processor, but it's idempotent if 
        // called subsequently
        builder.UseQueueProcessor(true);

        return builder;
    }
}
