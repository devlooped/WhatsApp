using Azure.Storage.Queues;

namespace Devlooped.WhatsApp;

class QueueMessageProcessor(QueueServiceClient client) : IMessageProcessor
{
    public async Task EnqueueAsync(string json, CancellationToken cancellation = default)
    {
        var queue = client.GetQueueClient("whatsappwebhook");
        await queue.CreateIfNotExistsAsync(cancellationToken: cancellation);
        await queue.SendMessageAsync(json, cancellation);
    }
}
