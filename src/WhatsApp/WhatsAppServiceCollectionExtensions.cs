using System.ComponentModel;
using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace Devlooped.WhatsApp;

/// <summary>
/// Provides internal extension methods for registering core WhatsApp services.
/// These methods are used by hosting-specific packages (ASP.NET Core, Azure Functions) 
/// to configure common WhatsApp services.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
static class WhatsAppServiceCollectionExtensions
{
    /// <summary>Registers a singleton <see cref="IWhatsAppClient"/> and <see cref="IWhatsAppHandler"/> in the <see cref="IServiceCollection"/>.</summary>
    internal static WhatsAppHandlerBuilder AddWhatsAppCore(
        this IServiceCollection collection,
        IWhatsAppHandler handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
    {
        Throw.IfNull(collection);
        Throw.IfNull(handler);

        return AddWhatsAppCore(collection, _ => handler, lifetime, configure);
    }

    /// <summary>Registers a singleton <see cref="IWhatsAppClient"/> and <see cref="IWhatsAppHandler"/> in the <see cref="IServiceCollection"/>.</summary>
    internal static WhatsAppHandlerBuilder AddWhatsAppCore(
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
        ConfigureCoreServices(collection, builder, lifetime, configure);

        return builder;
    }

    /// <summary>
    /// Add WhatsApp services and use an already registered service that implements <see cref="IWhatsAppHandler"/>.
    /// </summary>
    internal static WhatsAppHandlerBuilder AddWhatsAppCore(
        this IServiceCollection collection,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        => collection.AddWhatsAppCore(services => services.GetRequiredService<IWhatsAppHandler>(), lifetime, configure);

    /// <summary>
    /// Configure the WhatsApp handler with a typed handler.
    /// </summary>
    internal static WhatsAppHandlerBuilder AddWhatsAppCore<THandler>(
        this IServiceCollection collection,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where THandler : class, IWhatsAppHandler
    {
        if (collection.FirstOrDefault(x => x.ServiceType == typeof(THandler)) == null)
        {
            collection.Add(new ServiceDescriptor(typeof(THandler), typeof(THandler), lifetime));
        }

        return collection.AddWhatsAppCore(services => services.GetRequiredService<THandler>(), lifetime, configure);
    }

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate.
    /// </summary>
    internal static WhatsAppHandlerBuilder AddWhatsAppCore(
        this IServiceCollection collection,
        Func<IServiceProvider, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
    {
        return collection.AddWhatsAppCore(
            services => AnonymousWhatsAppHandler.Create(services, handler), lifetime, configure);
    }

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate.
    /// </summary>
    internal static WhatsAppHandlerBuilder AddWhatsAppCore(
        this IServiceCollection collection,
        Func<IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
    {
        return collection.AddWhatsAppCore(
            services => AnonymousWhatsAppHandler.Create(services, handler), lifetime, configure);
    }

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate that receives a service.
    /// </summary>
    internal static WhatsAppHandlerBuilder AddWhatsAppCore<TService>(
        this IServiceCollection collection,
        Func<TService, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where TService : notnull
    {
        return collection.AddWhatsAppCore(
            services => AnonymousWhatsAppHandler.Create(services.GetRequiredService<TService>(), handler), lifetime, configure);
    }

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate that receives two services.
    /// </summary>
    internal static WhatsAppHandlerBuilder AddWhatsAppCore<TService1, TService2>(
        this IServiceCollection collection,
        Func<TService1, TService2, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where TService1 : notnull
        where TService2 : notnull
    {
        return collection.AddWhatsAppCore(
            services => AnonymousWhatsAppHandler.Create(
                services.GetRequiredService<TService1>(),
                services.GetRequiredService<TService2>(),
                handler), lifetime, configure);
    }

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate that receives three services.
    /// </summary>
    internal static WhatsAppHandlerBuilder AddWhatsAppCore<TService1, TService2, TService3>(
        this IServiceCollection collection,
        Func<TService1, TService2, TService3, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where TService1 : notnull
        where TService2 : notnull
        where TService3 : notnull
    {
        return collection.AddWhatsAppCore(
            services => AnonymousWhatsAppHandler.Create(
                services.GetRequiredService<TService1>(),
                services.GetRequiredService<TService2>(),
                services.GetRequiredService<TService3>(),
                handler), lifetime, configure);
    }

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate that receives four services.
    /// </summary>
    internal static WhatsAppHandlerBuilder AddWhatsAppCore<TService1, TService2, TService3, TService4>(
        this IServiceCollection collection,
        Func<TService1, TService2, TService3, TService4, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where TService1 : notnull
        where TService2 : notnull
        where TService3 : notnull
        where TService4 : notnull
    {
        return collection.AddWhatsAppCore(
            services => AnonymousWhatsAppHandler.Create(
                services.GetRequiredService<TService1>(),
                services.GetRequiredService<TService2>(),
                services.GetRequiredService<TService3>(),
                services.GetRequiredService<TService4>(),
                handler), lifetime, configure);
    }

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate that receives five services.
    /// </summary>
    internal static WhatsAppHandlerBuilder AddWhatsAppCore<TService1, TService2, TService3, TService4, TService5>(
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
        return collection.AddWhatsAppCore(
            services => AnonymousWhatsAppHandler.Create(
                services.GetRequiredService<TService1>(),
                services.GetRequiredService<TService2>(),
                services.GetRequiredService<TService3>(),
                services.GetRequiredService<TService4>(),
                services.GetRequiredService<TService5>(),
                handler), lifetime, configure);
    }

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate that receives six services.
    /// </summary>
    internal static WhatsAppHandlerBuilder AddWhatsAppCore<TService1, TService2, TService3, TService4, TService5, TService6>(
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
        return collection.AddWhatsAppCore(
            services => AnonymousWhatsAppHandler.Create(
                services.GetRequiredService<TService1>(),
                services.GetRequiredService<TService2>(),
                services.GetRequiredService<TService3>(),
                services.GetRequiredService<TService4>(),
                services.GetRequiredService<TService5>(),
                services.GetRequiredService<TService6>(),
                handler), lifetime, configure);
    }

    /// <summary>
    /// Configures core WhatsApp services that are platform-agnostic.
    /// </summary>
    internal static WhatsAppHandlerBuilder ConfigureCoreServices(IServiceCollection services, WhatsAppHandlerBuilder builder, ServiceLifetime lifetime, Action<WhatsAppOptions>? configure)
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

        // Register direct handler and runner factories for non-Functions hosting
        services.TryAdd(new ServiceDescriptor(typeof(Func<IWhatsAppHandler>), services => () =>
            services.GetRequiredService<IWhatsAppHandler>(), lifetime));

        services.TryAdd(new ServiceDescriptor(typeof(Func<PipelineRunner>), services => () =>
            services.GetRequiredService<PipelineRunner>(), lifetime));

        // By default we use the task scheduler processor, which doesn't require Azure Functions
        builder.UseTaskSchedulerProcessor();

        return builder;
    }
}
