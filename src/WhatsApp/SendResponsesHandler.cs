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

    public async override IAsyncEnumerable<Response> HandleAsync(IEnumerable<Message> messages, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        await foreach (var response in InnerHandler.HandleAsync(messages))
        {
            await response.SendAsync(client, cancellation);

            yield return response;
        }
    }
}