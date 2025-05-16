using System.ComponentModel;

namespace Devlooped.WhatsApp;

/// <summary>
/// Provides extensions for configuring <see cref="OpenTelemetryHandler"/> instances.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class OpenTelemetryHandlerExtensions
{
    /// <summary>
    /// Adds the <see cref="OpenTelemetryHandler"/> handler to the pipeline.
    /// </summary>
    /// <param name="builder">The handler builder.</param>
    /// <param name="loggerFactory">The logger factory.</param>
    /// <returns>The updated handler builder.</returns>
    public static WhatsAppHandlerBuilder UseOpenTelemetry(
        this WhatsAppHandlerBuilder builder,
        string? sourceName = null,
        Action<OpenTelemetryHandler>? configure = null)
    {
        return Throw.IfNull(builder).Use((inner, services) =>
        {
            var handler = new OpenTelemetryHandler(inner, sourceName);
            configure?.Invoke(handler);
            return handler;
        });
    }
}
