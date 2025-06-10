using Azure.Data.Tables;
using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.FeatureManagement;

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
        IConfiguration configuration,
        IWhatsAppHandler handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        Throw.IfNull(collection);
        Throw.IfNull(handler);

        return AddWhatsApp(collection, configuration, _ => handler, lifetime);
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
        IConfiguration configuration,
        Func<IServiceProvider, IWhatsAppHandler> handlerFactory,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        _ = Throw.IfNull(collection);
        _ = Throw.IfNull(handlerFactory);

        // Create builder
        var builder = new WhatsAppHandlerBuilder(handlerFactory, collection);

        // Configure default services
        ConfigureServices(collection, builder, configuration, lifetime);

        // Add storage handler for response messages (it needs to be added before the send handler to get the generated id)
        builder.Use((inner, services) =>
        {
            // Check if the storage capability was enabled by getting the storage service
            if (services.GetService<IStorageService>() is IStorageService storageService)
            {
                return new ResponseStorageHandler(inner, storageService);
            }

            return WhatsAppHandler.Empty;
        });

        // Add the handler for sending responses
        builder.Use((inner, services) => new SendResponsesHandler(inner, services.GetRequiredService<IWhatsAppClient>()));

        // Set conversation handler for restoring the conversation id
        // This MUST run before the incoming storage handler to property set the ConversationId before saving the incoming message
        builder.Use((inner, services) =>
        {
            // Check if the conversation capability was enabled by getting the conversation service
            if (services.GetService<IConversationService>() is IConversationService conversationService)
            {
                return new SetConversationHandler(inner, conversationService);
            }

            return WhatsAppHandler.Empty;
        });

        // Add storage handler for incoming messages
        builder.Use((inner, services) =>
        {
            // Check if the storage capability was enabled by getting the storage service
            if (services.GetService<IStorageService>() is IStorageService storageService)
            {
                return new MessageStorageHandler(inner, storageService);
            }

            return WhatsAppHandler.Empty;
        });

        // Add conversation handler for restoring conversation message
        builder.Use((inner, services) =>
        {
            // Check if the conversation capability was enabled by getting the conversation service
            if (services.GetService<IConversationService>() is IConversationService conversationService)
            {
                return new RestoreConversationMessagesHandler(inner, conversationService);
            }

            return WhatsAppHandler.Empty;
        });

        return builder;
    }

    /// <summary>
    /// Add WhatsApp functions and use an already registered service that implements <see cref="IWhatsAppHandler"/>.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp(
        this IServiceCollection collection,
        IConfiguration configuration,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        => collection.AddWhatsApp(configuration, services => services.GetRequiredService<IWhatsAppHandler>(), lifetime);

    /// <summary>
    /// Configure the WhatsApp handler for Azure Functions.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp<THandler>(
        this IServiceCollection collection,
        IConfiguration configuration,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where THandler : class, IWhatsAppHandler
    {
        if (collection.FirstOrDefault(x => x.ServiceType == typeof(THandler)) == null)
        {
            collection.Add(new ServiceDescriptor(typeof(THandler), typeof(THandler), lifetime));
        }

        return collection.AddWhatsApp(configuration, services => services.GetRequiredService<THandler>(), lifetime);
    }

    /// <summary>
    /// Configure the WhatsApp handler for Azure Functions.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp(
        this IServiceCollection collection,
        IConfiguration configuration,
        Func<IServiceProvider, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        return collection.AddWhatsApp(
            configuration,
            services => AnonymousWhatsAppHandler.Create(services, handler), lifetime);
    }

    /// <summary>
    /// Configure the WhatsApp handler for Azure Functions.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp(
        this IServiceCollection collection,
        IConfiguration configuration,
        Func<IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
    {
        return collection.AddWhatsApp(
            configuration,
            services => AnonymousWhatsAppHandler.Create(services, handler), lifetime);
    }

    /// <summary>
    /// Configure the WhatsApp handler for Azure Functions.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp<TService>(
        this IServiceCollection collection,
        IConfiguration configuration,
        Func<TService, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TService : notnull
    {
        return collection.AddWhatsApp(
            configuration,
            services => AnonymousWhatsAppHandler.Create(services.GetRequiredService<TService>(), handler), lifetime);
    }

    /// <summary>
    /// Configure the WhatsApp handler for Azure Functions.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp<TService1, TService2>(
        this IServiceCollection collection,
        IConfiguration configuration,
        Func<TService1, TService2, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TService1 : notnull
        where TService2 : notnull
    {
        return collection.AddWhatsApp(
            configuration,
            services => AnonymousWhatsAppHandler.Create(
                services.GetRequiredService<TService1>(),
                services.GetRequiredService<TService2>(),
                handler), lifetime);
    }

    /// <summary>
    /// Configure the WhatsApp handler for Azure Functions.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp<TService1, TService2, TService3>(
        this IServiceCollection collection,
        IConfiguration configuration,
        Func<TService1, TService2, TService3, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TService1 : notnull
        where TService2 : notnull
        where TService3 : notnull
    {
        return collection.AddWhatsApp(
            configuration,
            services => AnonymousWhatsAppHandler.Create(
                services.GetRequiredService<TService1>(),
                services.GetRequiredService<TService2>(),
                services.GetRequiredService<TService3>(),
                handler), lifetime);
    }

    /// <summary>
    /// Configure the WhatsApp handler for Azure Functions.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp<TService1, TService2, TService3, TService4>(
        this IServiceCollection collection,
        IConfiguration configuration,
        Func<TService1, TService2, TService3, TService4, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TService1 : notnull
        where TService2 : notnull
        where TService3 : notnull
        where TService4 : notnull
    {
        return collection.AddWhatsApp(
            configuration,
            services => AnonymousWhatsAppHandler.Create(
                services.GetRequiredService<TService1>(),
                services.GetRequiredService<TService2>(),
                services.GetRequiredService<TService3>(),
                services.GetRequiredService<TService4>(),
                handler), lifetime);
    }

    /// <summary>
    /// Configure the WhatsApp handler for Azure Functions.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp<TService1, TService2, TService3, TService4, TService5>(
        this IServiceCollection collection,
        IConfiguration configuration,
        Func<TService1, TService2, TService3, TService4, TService5, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TService1 : notnull
        where TService2 : notnull
        where TService3 : notnull
        where TService4 : notnull
        where TService5 : notnull
    {
        return collection.AddWhatsApp(
            configuration,
            services => AnonymousWhatsAppHandler.Create(
                services.GetRequiredService<TService1>(),
                services.GetRequiredService<TService2>(),
                services.GetRequiredService<TService3>(),
                services.GetRequiredService<TService4>(),
                services.GetRequiredService<TService5>(),
                handler), lifetime);
    }

    /// <summary>
    /// Configure the WhatsApp handler for Azure Functions.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp<TService1, TService2, TService3, TService4, TService5, TService6>(
        this IServiceCollection collection,
        IConfiguration configuration,
        Func<TService1, TService2, TService3, TService4, TService5, TService6, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton)
        where TService1 : notnull
        where TService2 : notnull
        where TService3 : notnull
        where TService4 : notnull
        where TService5 : notnull
        where TService6 : notnull
    {
        return collection.AddWhatsApp(
            configuration,
            services => AnonymousWhatsAppHandler.Create(
                services.GetRequiredService<TService1>(),
                services.GetRequiredService<TService2>(),
                services.GetRequiredService<TService3>(),
                services.GetRequiredService<TService4>(),
                services.GetRequiredService<TService5>(),
                services.GetRequiredService<TService6>(),
                handler), lifetime);
    }
    static WhatsAppHandlerBuilder ConfigureServices(IServiceCollection services, WhatsAppHandlerBuilder builder, IConfiguration configuration, ServiceLifetime lifetime)
    {
        services.AddHttpClient("whatsapp").AddStandardResilienceHandler();
        services.Add(new ServiceDescriptor(typeof(IWhatsAppClient), typeof(WhatsAppClient), lifetime));

        services.AddFeatures(configuration);

        if (services.FirstOrDefault(x => x.ServiceType == typeof(QueueServiceClient)) == null)
        {
            services.AddSingleton(services => new QueueServiceClient(
                services.GetRequiredService<IConfiguration>()["AzureWebJobsStorage"]!,
                new QueueClientOptions
                {
#if DEBUG
                    Diagnostics =
                    {
                        IsLoggingEnabled = true,
                        IsLoggingContentEnabled = true,
                    },
#endif
                    MessageEncoding = QueueMessageEncoding.Base64
                }));
        }

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

        services.AddOptions<MetaOptions>()
            .BindConfiguration("Meta")
            .ValidateDataAnnotations();

        services.Add(new ServiceDescriptor(typeof(IWhatsAppHandler), builder.Build, lifetime));

        return builder;
    }
}
