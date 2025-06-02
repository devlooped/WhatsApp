using System.Runtime.CompilerServices;

namespace Devlooped.WhatsApp;

class SetConversationHandler(IWhatsAppHandler innerHandler, IConversationService conversationService) : DelegatingWhatsAppHandler(innerHandler)
{
    public override async IAsyncEnumerable<Response> HandleAsync(IEnumerable<IMessage> messages, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        IEnumerable<IMessage> conversation;

        // Optimization to avoid creating the list when there is only 1 message to be processed
        if (messages.TrySingle(out var single))
        {
            conversation = [await FixMessageAsync(single, cancellation)];
        }
        else
        {
            var conversationList = new List<IMessage>();

            foreach (var message in messages)
            {
                conversationList.Add(await FixMessageAsync(message, cancellation));
            }

            conversation = conversationList;
        }

        await foreach (var response in base.HandleAsync(conversation, cancellation))
        {
            yield return response;
        }
    }

    async Task<IMessage> FixMessageAsync(IMessage message, CancellationToken cancellation)
    {
        // Avoid setting conversation id for status messages because they occur all the time
        if (message is Message messageRecord && string.IsNullOrEmpty(message.ConversationId) && message is not StatusMessage)
            return messageRecord with { ConversationId = await conversationService.GetOrCreateConversationIdAsync(message, cancellationToken: cancellation) };
        else
            return message;
    }
}