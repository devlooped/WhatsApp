using System.Diagnostics;
using System.Diagnostics.Metrics;
using System.Reflection.Metadata.Ecma335;
using Microsoft.Extensions.Logging;

namespace Devlooped.WhatsApp;

/// <summary>
/// Represents a delegating handler that implements the OpenTelemetry 
/// Semantic Conventions for Messaging systems.
/// </summary>
/// <remarks>
/// This class provides an implementation of applicable Semantic Conventions for 
/// Messaging systems v1.33, defined at <see href="https://opentelemetry.io/docs/specs/semconv/messaging/" />.
/// </remarks>
public class OpenTelemetryHandler : DelegatingWhatsAppHandler
{
    static readonly double[] ExplicitBucketBoundaries = [0.01, 0.02, 0.04, 0.08, 0.16, 0.32, 0.64, 1.28, 2.56, 5.12, 10.24, 20.48, 40.96, 81.92];
    readonly ActivitySource activitySource;
    readonly Meter meter;
    readonly Histogram<double> processDuration;
    readonly Counter<long> messagesProcessed;

    public OpenTelemetryHandler(IWhatsAppHandler innerHandler, string? sourceName = null)
        : base(innerHandler)
    {
        activitySource = new(sourceName ?? nameof(WhatsApp), ThisAssembly.Info.Version);
        meter = new(sourceName ?? nameof(WhatsApp), ThisAssembly.Info.Version);

        processDuration = meter.CreateHistogram<double>(
            "messaging.process.duration",
            "s",
            "Duration of WhatsApp message processing"
#if NET9_0_OR_GREATER
            , advice: new() { HistogramBucketBoundaries = ExplicitBucketBoundaries }
#endif
        );
        messagesProcessed = meter.CreateCounter<long>(
            "messaging.client.consumed.messages",
            "messages",
            "Number of WhatsApp messages processed"
        );
    }

    /// <inheritdoc/>
    protected override void Dispose(bool disposing)
    {
        if (disposing)
        {
            activitySource.Dispose();
            meter.Dispose();
        }

        base.Dispose(disposing);
    }

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

    public override IAsyncEnumerable<Response> HandleAsync(IEnumerable<IMessage> messages, CancellationToken cancellation = default)
    {
        // In a conversation, the last message is the most recent one sent by the user.
        // This is just in case the handler is not configured as the first in the pipeline.
        var message = messages.LastOrDefault();
        if (message is null)
        {
            return base.HandleAsync(messages, cancellation);
        }
        else
        {
            using var span = activitySource.StartActivity("whatsapp process", ActivityKind.Consumer);
            if (span != null)
            {
                span.SetTag("messaging.system", "whatsapp");
                span.SetTag("messaging.destination", "whatsapp");
                span.SetTag("messaging.operation", "process");
                span.SetTag("messaging.message.id", message.Id);
                if (message.ConversationId is string conversationId)
                    span.SetTag("messaging.message.conversation_id", conversationId);
            }

            var startTime = Stopwatch.GetTimestamp();
            var tags = new TagList
            {
                { "messaging.system", "whatsapp" },
                { "messaging.operation", "process" },
            };


            return base.HandleAsync(messages, cancellation).WithErrorHandlingAsync(
                errorCallback: ex => messagesProcessed.Add(1, span.RecordException(ex, EnableSensitiveData, tags)),
                completionCallback: () => messagesProcessed.Add(1, tags),
                finallyCallback: () =>
                {
                    var duration = Stopwatch.GetElapsedTime(startTime).TotalSeconds;
                    processDuration.Record(duration, tags);
                },
                cancellation);
        }
    }
}