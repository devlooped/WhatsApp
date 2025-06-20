using System.Runtime.CompilerServices;
namespace Devlooped.WhatsApp;

/// <summary>
/// Handles the processing of messages by delegating to an inner handler and sending the resulting responses using the
/// specified WhatsApp client.
/// </summary>
class SendResponsesHandler : DelegatingWhatsAppHandler
{
    readonly IWhatsAppClient client;

    public SendResponsesHandler(IWhatsAppHandler innerHandler, IWhatsAppClient client)
        : base(innerHandler)
    {
        this.client = client;
    }

    public async override IAsyncEnumerable<Response> HandleAsync(IEnumerable<IMessage> messages, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        await foreach (var response in InnerHandler.HandleAsync(messages, cancellation))
        {
            // Only sent unsent messages.
            if (string.IsNullOrEmpty(response.Id) && response.Timestamp == 0)
                yield return await response.SendAsync(client, cancellation);
            else
                yield return response;
        }
    }
}