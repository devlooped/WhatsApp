using System.Runtime.CompilerServices;

namespace Devlooped.WhatsApp;

/// <summary>
/// Handles incoming messages by saving user messages to storage and delegating further processing to an inner handler.
/// </summary>
class MessageStorageHandler : DelegatingWhatsAppHandler
{
    readonly StorageService storageService;

    public MessageStorageHandler(IWhatsAppHandler innerHandler, StorageService storageService)
        : base(innerHandler)
    {
        this.storageService = storageService;
    }

    public override async IAsyncEnumerable<Response> HandleAsync(IEnumerable<Message> messages, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        // Save the incoming user messages only. Avoid system messages, etc
        // TODO: Fire and forget? Do we really need to wait for the messages to be fully saved here?
        await storageService.SaveAsync(messages.OfType<UserMessage>(), cancellation);

        await foreach (var response in base.HandleAsync(messages, cancellation))
        {
            yield return response;
        }
    }
}