using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Logging.Abstractions;

namespace Devlooped.WhatsApp;

/// <summary>
/// Provides extensions for configuring <see cref="LoggingHandler"/> instances.
/// </summary>
public static class LoggingHandlerBuilderExtensions
{
    /// <summary>
    /// Adds a logging handler to the pipeline.
    /// </summary>
    /// <param name="builder">The handler builder.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <returns>The updated handler builder.</returns>
    public static WhatsAppHandlerBuilder UseLogging(
        this WhatsAppHandlerBuilder builder,
        ILoggerFactory? loggerFactory = null,
        Action<LoggingHandler>? configure = null)
    {
        _ = Throw.IfNull(builder);

        return builder.Use((inner, services) =>
        {
            loggerFactory ??= services.GetRequiredService<ILoggerFactory>();
            // If the factory we resolve is for the null logger, just return the inner handler since no logging will happen anyway.
            if (loggerFactory == NullLoggerFactory.Instance)
                return inner;

            var handler = new LoggingHandler(inner, loggerFactory.CreateLogger<LoggingHandler>());
            configure?.Invoke(handler);
            return handler;
        });
    }
}
