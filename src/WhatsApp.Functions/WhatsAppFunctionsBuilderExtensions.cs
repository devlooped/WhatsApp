using System.ComponentModel;
using Azure.Storage.Queues;
using Microsoft.Azure.Functions.Worker;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Devlooped.WhatsApp;

/// <summary>
/// Provides extension methods for registering WhatsApp services for Azure Functions.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WhatsAppFunctionsBuilderExtensions
{
    /// <summary>
    /// Adds required WhatsApp middleware to the functions worker application builder.
    /// </summary>
    [EditorBrowsable(EditorBrowsableState.Never)]
    public static IFunctionsWorkerApplicationBuilder UseWhatsApp(this IFunctionsWorkerApplicationBuilder builder)
        => builder.UseFunctionContextAccessor();

    /// <summary>Registers a singleton <see cref="IWhatsAppClient"/> and <see cref="IWhatsAppHandler"/> in the <see cref="IServiceCollection"/> for Azure Functions hosting.</summary>
    /// <param name="builder">The <see cref="IFunctionsWorkerApplicationBuilder"/> to which the WhatsApp services should be added.</param>
    /// <param name="handler">The <see cref="IWhatsAppHandler"/> that handles incoming WhatsApp messages as the underlying backend.</param>
    /// <param name="lifetime">The service lifetime for the client and handler. Defaults to <see cref="ServiceLifetime.Singleton"/>.</param>
    /// <param name="configure">Optional configuration callback for <see cref="WhatsAppOptions"/>.</param>
    /// <returns>A <see cref="WhatsAppHandlerBuilder"/> that can be used to build a pipeline around the handler.</returns>
    public static WhatsAppHandlerBuilder AddWhatsApp(
        this IFunctionsWorkerApplicationBuilder builder,
        IWhatsAppHandler handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
    {
        builder.UseFunctionContextAccessor();
        var handlerBuilder = builder.Services.AddWhatsAppCore(handler, lifetime, configure);
        return ConfigureAzureFunctions(builder.Services, handlerBuilder, false);
    }

    /// <summary>Registers a singleton <see cref="IWhatsAppClient"/> and <see cref="IWhatsAppHandler"/> in the <see cref="IServiceCollection"/> for Azure Functions hosting.</summary>
    /// <param name="builder">The <see cref="IFunctionsWorkerApplicationBuilder"/> to which the WhatsApp services should be added.</param>
    /// <param name="handlerFactory">A callback that produces the inner <see cref="IWhatsAppHandler"/> that represents the underlying backend.</param>
    /// <param name="lifetime">The service lifetime for the client and handler. Defaults to <see cref="ServiceLifetime.Singleton"/>.</param>
    /// <param name="configure">Optional configuration callback for <see cref="WhatsAppOptions"/>.</param>
    /// <returns>A <see cref="WhatsAppHandlerBuilder"/> that can be used to build a pipeline around the handler.</returns>
    public static WhatsAppHandlerBuilder AddWhatsApp(
        this IFunctionsWorkerApplicationBuilder builder,
        Func<IServiceProvider, IWhatsAppHandler> handlerFactory,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
    {
        builder.UseFunctionContextAccessor();
        var handlerBuilder = builder.Services.AddWhatsAppCore(handlerFactory, lifetime, configure);
        return ConfigureAzureFunctions(builder.Services, handlerBuilder, false);
    }

    /// <summary>
    /// Add WhatsApp services for Azure Functions and use an already registered service that implements <see cref="IWhatsAppHandler"/>.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp(
        this IFunctionsWorkerApplicationBuilder builder,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
    {
        builder.UseFunctionContextAccessor();
        var handlerBuilder = builder.Services.AddWhatsAppCore(lifetime, configure);
        return ConfigureAzureFunctions(builder.Services, handlerBuilder, false);
    }

    /// <summary>
    /// Configure the WhatsApp handler with a typed handler for Azure Functions hosting.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp<THandler>(
        this IFunctionsWorkerApplicationBuilder builder,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where THandler : class, IWhatsAppHandler
    {
        builder.UseFunctionContextAccessor();
        var handlerBuilder = builder.Services.AddWhatsAppCore<THandler>(lifetime, configure);
        return ConfigureAzureFunctions(builder.Services, handlerBuilder, false);
    }

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate for Azure Functions hosting.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp(
        this IFunctionsWorkerApplicationBuilder builder,
        Func<IServiceProvider, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
    {
        builder.UseFunctionContextAccessor();
        var handlerBuilder = builder.Services.AddWhatsAppCore(handler, lifetime, configure);
        return ConfigureAzureFunctions(builder.Services, handlerBuilder, false);
    }

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate for Azure Functions hosting.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp(
        this IFunctionsWorkerApplicationBuilder builder,
        Func<IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
    {
        builder.UseFunctionContextAccessor();
        var handlerBuilder = builder.Services.AddWhatsAppCore(handler, lifetime, configure);
        return ConfigureAzureFunctions(builder.Services, handlerBuilder, false);
    }

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate that receives a service for Azure Functions hosting.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp<TService>(
        this IFunctionsWorkerApplicationBuilder builder,
        Func<TService, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where TService : notnull
    {
        builder.UseFunctionContextAccessor();
        var handlerBuilder = builder.Services.AddWhatsAppCore(handler, lifetime, configure);
        return ConfigureAzureFunctions(builder.Services, handlerBuilder, false);
    }

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate that receives two services for Azure Functions hosting.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp<TService1, TService2>(
        this IFunctionsWorkerApplicationBuilder builder,
        Func<TService1, TService2, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where TService1 : notnull
        where TService2 : notnull
    {
        builder.UseFunctionContextAccessor();
        var handlerBuilder = builder.Services.AddWhatsAppCore(handler, lifetime, configure);
        return ConfigureAzureFunctions(builder.Services, handlerBuilder, false);
    }

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate that receives three services for Azure Functions hosting.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp<TService1, TService2, TService3>(
        this IFunctionsWorkerApplicationBuilder builder,
        Func<TService1, TService2, TService3, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where TService1 : notnull
        where TService2 : notnull
        where TService3 : notnull
    {
        builder.UseFunctionContextAccessor();
        var handlerBuilder = builder.Services.AddWhatsAppCore(handler, lifetime, configure);
        return ConfigureAzureFunctions(builder.Services, handlerBuilder, false);
    }

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate that receives four services for Azure Functions hosting.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp<TService1, TService2, TService3, TService4>(
        this IFunctionsWorkerApplicationBuilder builder,
        Func<TService1, TService2, TService3, TService4, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where TService1 : notnull
        where TService2 : notnull
        where TService3 : notnull
        where TService4 : notnull
    {
        builder.UseFunctionContextAccessor();
        var handlerBuilder = builder.Services.AddWhatsAppCore(handler, lifetime, configure);
        return ConfigureAzureFunctions(builder.Services, handlerBuilder, false);
    }

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate that receives five services for Azure Functions hosting.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp<TService1, TService2, TService3, TService4, TService5>(
        this IFunctionsWorkerApplicationBuilder builder,
        Func<TService1, TService2, TService3, TService4, TService5, IEnumerable<IMessage>, CancellationToken, IAsyncEnumerable<Response>> handler,
        ServiceLifetime lifetime = ServiceLifetime.Singleton,
        Action<WhatsAppOptions>? configure = null)
        where TService1 : notnull
        where TService2 : notnull
        where TService3 : notnull
        where TService4 : notnull
        where TService5 : notnull
    {
        builder.UseFunctionContextAccessor();
        var handlerBuilder = builder.Services.AddWhatsAppCore(handler, lifetime, configure);
        return ConfigureAzureFunctions(builder.Services, handlerBuilder, false);
    }

    /// <summary>
    /// Configure the WhatsApp handler with an anonymous handler delegate that receives six services for Azure Functions hosting.
    /// </summary>
    public static WhatsAppHandlerBuilder AddWhatsApp<TService1, TService2, TService3, TService4, TService5, TService6>(
        this IFunctionsWorkerApplicationBuilder builder,
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
        builder.UseFunctionContextAccessor();
        var handlerBuilder = builder.Services.AddWhatsAppCore(handler, lifetime, configure);
        return ConfigureAzureFunctions(builder.Services, handlerBuilder, false);
    }

    internal static WhatsAppHandlerBuilder ConfigureAzureFunctions(this IServiceCollection services, WhatsAppHandlerBuilder handlerBuilder, bool validateAccessor, Action<QueueClientOptions>? configure = null)
    {
        if (validateAccessor && services.AsEnumerable().FirstOrDefault(x => x.ServiceType == typeof(IFunctionContextAccessor)) == null)
            throw new InvalidOperationException("Function context accessor is missing. Please ensure you call UseWhatsApp() on the functions application builder to register IFunctionContextAccessor.");

        // Remove the default handler and runner factories from the core registration
        var handlerFactory = services.FirstOrDefault(x => x.ServiceType == typeof(Func<IWhatsAppHandler>));
        if (handlerFactory != null)
            services.Remove(handlerFactory);

        var runnerFactory = services.FirstOrDefault(x => x.ServiceType == typeof(Func<PipelineRunner>));
        if (runnerFactory != null)
            services.Remove(runnerFactory);

        // Register Azure Functions-specific handler and runner factories that use FunctionContext
        services.AddSingleton<Func<IWhatsAppHandler>>(services => () =>
        {
            var accessor = services.GetRequiredService<IFunctionContextAccessor>();
            var ctx = accessor.FunctionContext ?? throw new InvalidOperationException("FunctionContext is not available.");
            return ctx.InstanceServices.GetRequiredService<IWhatsAppHandler>();
        });

        services.AddSingleton<Func<PipelineRunner>>(services => () =>
        {
            var accessor = services.GetRequiredService<IFunctionContextAccessor>();
            var ctx = accessor.FunctionContext ?? throw new InvalidOperationException("FunctionContext is not available.");
            return ctx.InstanceServices.GetRequiredService<PipelineRunner>();
        });

        // Register the queue processor as the default message processor for Azure Functions
        handlerBuilder.UseQueueProcessor(configure);

        // Use Azure Table Storage for durable idempotency tracking in Azure Functions
        handlerBuilder.UseIdempotencyStorage("AzureWebJobsStorage");

        return handlerBuilder;
    }
}
