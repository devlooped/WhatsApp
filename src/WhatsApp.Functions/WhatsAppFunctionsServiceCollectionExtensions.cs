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
public static class WhatsAppFunctionsServiceCollectionExtensions
{
    /// <summary>
    /// Configures WhatsApp services for Azure Functions hosting.
    /// </summary>
    /// <remarks>
    /// This method should be called after <see cref="WhatsAppServiceCollectionExtensions.AddWhatsApp(IServiceCollection, IWhatsAppHandler, ServiceLifetime, Action{WhatsAppOptions}?)"/> 
    /// to configure Azure Functions-specific services.
    /// </remarks>
    public static WhatsAppHandlerBuilder UseAzureFunctions(this WhatsAppHandlerBuilder builder, Action<QueueClientOptions>? configure = default)
    {
        Throw.IfNull(builder);

        var services = builder.Services;

        // Validate that IFunctionContextAccessor is registered
        if (services.AsEnumerable().FirstOrDefault(x => x.ServiceType == typeof(IFunctionContextAccessor)) == null)
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
            var ctx = accessor.FunctionContext ?? throw new InvalidOperationException("FunctionContext is not available. Ensure UseWhatsApp() has been called on the application builder.");
            return ctx.InstanceServices.GetRequiredService<IWhatsAppHandler>();
        });

        services.AddSingleton<Func<PipelineRunner>>(services => () =>
        {
            var accessor = services.GetRequiredService<IFunctionContextAccessor>();
            var ctx = accessor.FunctionContext ?? throw new InvalidOperationException("FunctionContext is not available. Ensure UseWhatsApp() has been called on the application builder.");
            return ctx.InstanceServices.GetRequiredService<PipelineRunner>();
        });

        // Register the queue processor as the default message processor for Azure Functions
        builder.UseQueueProcessor(configure);

        return builder;
    }
}
