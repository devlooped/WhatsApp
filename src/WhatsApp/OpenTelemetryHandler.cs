namespace Devlooped.WhatsApp;

/// <summary>
/// Represents a delegating handler that implements the OpenTelemetry 
/// Semantic Conventions for Messaging systems.
/// </summary>
/// <remarks>
/// This class provides an implementation of applicable Semantic Conventions for 
/// Messaging systems v1.33, defined at <see href="https://opentelemetry.io/docs/specs/semconv/messaging/" />.
/// </remarks>
public class OpenTelemetryHandler(IWhatsAppHandler innerHandler) : DelegatingHandler(innerHandler)
{
    /// <summary>
    /// Gets or sets a value indicating whether potentially sensitive information 
    /// should be included in telemetry.
    /// </summary>
    /// <value>
    /// <see langword="true"/> if potentially sensitive information should be included in telemetry;
    /// <see langword="false"/> if telemetry shouldn't include raw inputs.
    /// The default value is <see langword="false"/>.
    /// </value>
    /// <remarks>
    /// By default, telemetry includes metadata, such as invocation counts, but not raw inputs, 
    /// such as message content.
    /// </remarks>
    public bool EnableSensitiveData { get; set; }
}
