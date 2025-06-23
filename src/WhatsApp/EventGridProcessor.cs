using Azure.Messaging.EventGrid;

namespace Devlooped.WhatsApp;

/// <summary>
/// Options used to populate <see cref="EventGridEvent"/> when publishing 
/// to the processor. Can be used to collect telemetry and logs.
/// </summary>
public class EventGridOptions
{
    /// <summary>
    /// The <see cref="EventGridEvent.Subject"/>. Defaults to <c>EventGridProcessor</c>.
    /// </summary>
    public string Subject { get; set; } = nameof(EventGridProcessor);
    /// <summary>
    /// The <see cref="EventGridEvent.EventType"/>. Defaults to <c>Devlooped.WhatsApp.MessageReceived</c>."/>
    /// </summary>
    public string EventType { get; set; } = "Devlooped.WhatsApp.MessageReceived";
    /// <summary>
    /// The <see cref="EventGridEvent.DataVersion"/>. Defaults to the assembly informational version.
    /// </summary>
    public string DataVersion { get; set; } = ThisAssembly.Info.InformationalVersion;
}

class EventGridProcessor(EventGridPublisherClient client, EventGridOptions options) : IMessageProcessor
{
    public async Task EnqueueAsync(/* lang=json */ string json, CancellationToken cancellation = default)
    {
        await client.SendEventAsync(new EventGridEvent(
            options.Subject, options.EventType, options.DataVersion, json), cancellation);
    }
}