namespace Devlooped.WhatsApp;

/// <summary>
/// Interface implemented by the async message processing used 
/// to decouple the WhatsApp webhook from the actual processing. 
/// </summary>
/// <remarks>
/// Unless explicitly configured otherwise, the default implementation 
/// will use Azure Storage Queues to enqueue the messages for processing.
/// </remarks>
public interface IMessageProcessor
{
    /// <summary>
    /// Enqueues the WhatsApp for Business webhook message for async processing.
    /// </summary>
    Task EnqueueAsync(string json, CancellationToken cancellation = default);
}
