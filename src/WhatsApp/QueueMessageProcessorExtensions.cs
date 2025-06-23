using Azure.Storage.Queues;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Devlooped.WhatsApp;

/// <summary>
/// Provides extensions for processing WhatsApp messages asynchronusly 
/// using Azure Functions queue.
/// </summary>
public static class QueueMessageProcessorExtensions
{
    /// <summary>
    /// Uses the Azure Functions queue to process WhatsApp messages asynchronously.
    /// </summary>
    /// <param name="builder">The builder pipeline</param>
    /// <param name="configure">Optional configuration callback for the queue.></param>
    public static WhatsAppHandlerBuilder UseQueueProcessor(this WhatsAppHandlerBuilder builder, Action<QueueClientOptions>? configure = default)
        => UseQueueProcessor(builder, false, configure: configure);
    internal static WhatsAppHandlerBuilder UseQueueProcessor(this WhatsAppHandlerBuilder builder, bool isDefault, Action<QueueClientOptions>? configure = default)
    {
        Throw.IfNull(builder);

        if (builder.Services.FirstOrDefault(x => x.ServiceType == typeof(QueueServiceClient)) is DefaultServiceDescriptor { } defaultService)
        {
            builder.Services.Remove(defaultService);
        }

        if (builder.Services.FirstOrDefault(x => x.ServiceType == typeof(QueueServiceClient)) == null)
        {
            Func<IServiceProvider, QueueServiceClient> factory = services =>
            {
                var options = new QueueClientOptions
                {
                    MessageEncoding = QueueMessageEncoding.Base64
                };
                if (services.GetRequiredService<IHostEnvironment>().IsDevelopment())
                {
                    options.Diagnostics.IsLoggingEnabled = true;
                    options.Diagnostics.IsLoggingContentEnabled = true;
                }
                configure?.Invoke(options);
                return new QueueServiceClient(
                    services.GetRequiredService<IConfiguration>()["AzureWebJobsStorage"]!,
                    options);
            };

            if (isDefault)
            {
                // By adding services by default this way, we can detect the service 
                // we added ourselves to remove it when the user calls UseQueueProcessor again 
                // passing their own configurator delegate.
                builder.Services.Add(DefaultServiceDescriptor.Create(factory, ServiceLifetime.Singleton));
            }
            else
            {
                builder.Services.AddSingleton(factory);
            }
        }

        if (builder.Services.FirstOrDefault(x => x.ServiceType == typeof(IMessageProcessor)) is { } processor)
        {
            builder.Services.Remove(processor);
        }

        builder.Services.AddSingleton<IMessageProcessor, QueueMessageProcessor>();

        return builder;
    }

    abstract class DefaultServiceDescriptor(Type serviceType, Func<IServiceProvider, object> factory, ServiceLifetime lifetime)
        : ServiceDescriptor(serviceType, factory, lifetime)
    {
        public static ServiceDescriptor Create<TService>(Func<IServiceProvider, TService> factory, ServiceLifetime lifetime)
            where TService : class
        {
            return new DefaultServiceDescriptor<TService>(factory, lifetime);
        }
    }

    class DefaultServiceDescriptor<TService>(Func<IServiceProvider, object> factory, ServiceLifetime lifetime)
        : DefaultServiceDescriptor(typeof(TService), factory, lifetime)
    {
    }
}