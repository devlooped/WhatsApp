using Azure.Messaging.EventGrid;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Devlooped.WhatsApp;

/// <summary>
/// Provides extensions for processing WhatsApp messages asynchronusly 
/// using Azure Functions queue.
/// </summary>
public static class EventGridProcessorExtensions
{
    /// <summary>
    /// Uses the Azure Functions queue to process WhatsApp messages asynchronously.
    /// </summary>
    /// <param name="builder">The builder pipeline</param>
    /// <param name="configure">Optional configuration callback for the queue.></param>
    public static WhatsAppHandlerBuilder UseEventGridProcessor(this WhatsAppHandlerBuilder builder,
        EventGridPublisherClient? publisher = default,
        Action<EventGridOptions>? configure = default)
    {
        Throw.IfNull(builder);

        if (builder.Services.FirstOrDefault(x => x.ServiceType == typeof(IMessageProcessor)) is { } processor)
        {
            builder.Services.Remove(processor);
        }

        builder.Services.AddSingleton<IMessageProcessor>(services =>
        {
            var options = services.GetService<IOptions<EventGridOptions>>()?.Value ?? new();
            configure?.Invoke(options);
            return new EventGridProcessor(publisher ??
                services.GetRequiredService<EventGridPublisherClient>(),
                options);
        });

        return builder;
    }
}