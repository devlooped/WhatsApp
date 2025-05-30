using System.Runtime.CompilerServices;

namespace Devlooped.WhatsApp;

/// <summary>
/// A handler that processes WhatsApp messages and stores the generated responses using a storage service.
/// </summary>
class ResponseStorageHandler : DelegatingWhatsAppHandler
{
    readonly IStorageService storageService;

    public ResponseStorageHandler(IWhatsAppHandler innerHandler, IStorageService storageService)
        : base(innerHandler)
    {
        this.storageService = storageService;
    }

    public async override IAsyncEnumerable<Response> HandleAsync(IEnumerable<Message> messages, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        await foreach (var response in InnerHandler.HandleAsync(messages, cancellation))
        {
            await storageService.SaveAsync(response, cancellation);

            yield return response;
        }
    }
}