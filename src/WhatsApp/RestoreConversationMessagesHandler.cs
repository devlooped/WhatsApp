using System.Runtime.CompilerServices;

namespace Devlooped.WhatsApp;

/// <summary>
/// Represents configuration options for a conversation.
/// </summary>
/// <remarks>This record is used to specify settings that control the behavior of a conversation.</remarks>
/// <param name="RestoreMessages">A value indicating whether to restore previous messages in the conversation. <see langword="true"/> to restore
/// messages; otherwise, <see langword="false"/>.</param>
public record ConversationOptions(bool RestoreMessages = true);

class RestoreConversationMessagesHandler(IWhatsAppHandler innerHandler, IConversationService conversationService, ConversationOptions options) : DelegatingWhatsAppHandler(innerHandler)
{
    public override async IAsyncEnumerable<Response> HandleAsync(IEnumerable<IMessage> messages, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        IEnumerable<IMessage> conversation = messages;

        if (options.RestoreMessages)
        {
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
        }

        await foreach (var response in base.HandleAsync(conversation, cancellation))
        {
            yield return response;
        }
    }
}