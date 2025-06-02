using System.Runtime.CompilerServices;

namespace Devlooped.WhatsApp;

class RestoreConversationMessagesHandler(IWhatsAppHandler innerHandler, IConversationService conversationService) : DelegatingWhatsAppHandler(innerHandler)
{
    public override async IAsyncEnumerable<Response> HandleAsync(IEnumerable<IMessage> messages, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        IEnumerable<IMessage> conversation;

        // Optimization to avoid creating the list when there is only 1 message to be processed
        if (messages.TrySingle(out var single))
        {
            conversation = await conversationService.GetConversationAsync(single, cancellation).ToArrayAsync();
        }
        else
        {
            var conversationList = new List<IMessage>();

            foreach (var message in messages)
            {
                await foreach (var conversationMessage in conversationService.GetConversationAsync(message, cancellation))
                {
                    conversationList.Add(conversationMessage);
                }
            }

            conversation = conversationList;
        }

        await foreach (var response in base.HandleAsync(conversation, cancellation))
        {
            yield return response;
        }
    }
}