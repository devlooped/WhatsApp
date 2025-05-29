using System.Runtime.CompilerServices;

namespace Devlooped.WhatsApp;

/// <summary>
/// A handler that processes WhatsApp messages and stores the generated responses using a storage service.
/// </summary>
class ResponseStorageHandler : DelegatingWhatsAppHandler
{
    readonly StorageService storageService;

    public ResponseStorageHandler(IWhatsAppHandler innerHandler, StorageService storageService)
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