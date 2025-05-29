using System.Runtime.CompilerServices;

namespace Devlooped.WhatsApp;

class SendResponsesHandler : DelegatingWhatsAppHandler
{
    readonly IWhatsAppClient whatsapp;

    public SendResponsesHandler(IWhatsAppHandler innerHandler, IWhatsAppClient whatsapp)
        : base(innerHandler)
    {
        this.whatsapp = whatsapp;
    }

    public async override IAsyncEnumerable<Response> HandleAsync(IEnumerable<Message> messages, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        await foreach (var response in InnerHandler.HandleAsync(messages))
        {
            await response.SendAsync(whatsapp);

            yield return response;
        }
    }
}