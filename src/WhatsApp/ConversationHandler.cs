using System.Runtime.CompilerServices;

namespace Devlooped.WhatsApp;


class ConversationHandler(IWhatsAppHandler inner, IConversationStorage storage) : DelegatingWhatsAppHandler(inner)
{
    /// <summary>
    /// Configures the time window to consider for conversation messages. 
    /// Messages sent within this time frame will be grouped together as part of the same conversation.
    /// </summary>
    public int ConversationWindowSeconds { get; set; } = 5 * 60; // 5 minutes

    public override async IAsyncEnumerable<Response> HandleAsync(IEnumerable<IMessage> messages, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        var conversation = new List<IMessage>();

        foreach (var message in messages)
        {
            var fixup = await SetConversationIdAsync(message, cancellation);
            // Now that it's fixed, we can persist the message.
            await storage.SaveAsync(fixup, cancellation);

            // Pull the entire conversation for the given (potentially fixed) message.
            await foreach (var saved in GetConversationAsync(fixup, cancellation))
            {
                conversation.Add(saved);
            }
        }

        await foreach (var response in base.HandleAsync(conversation, cancellation))
        {
            await storage.SaveAsync(response, cancellation);
            yield return response;
        }
    }

    async Task<IMessage> SetConversationIdAsync(IMessage message, CancellationToken cancellation)
    {
        // Avoid setting conversation id for status messages because they occur all the time
        if (message is Message messageRecord && string.IsNullOrEmpty(message.ConversationId) && message is not StatusMessage)
            return messageRecord with { ConversationId = await GetOrCreateConversationIdAsync(message, cancellationToken: cancellation) };
        else
            return message;
    }

    async IAsyncEnumerable<IMessage> GetConversationAsync(IMessage message, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        var shouldReturnInputMessage = true;

        if (!string.IsNullOrEmpty(message.ConversationId))
        {
            var conversation = await storage
                    .GetMessagesAsync(message.UserNumber, message.ConversationId, cancellationToken)
                    // We need to sort in-memory since table-storage does not support ordering by timestamp 
                    // unless we're using CosmosDB, which we can't assume. 
                    // Note that conversations in WhatsApp are nevertheless short-lived, so this is acceptable.
                    .ToListAsync();

            conversation.Sort(MessageComparer.Ascending);

            foreach (var conversationMessage in conversation)
            {
                if (string.Equals(conversationMessage.Id, message.Id, StringComparison.OrdinalIgnoreCase))
                    shouldReturnInputMessage = false;

                yield return conversationMessage;
            }
        }

        if (shouldReturnInputMessage)
            yield return message;
    }

    async Task<string> GetOrCreateConversationIdAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(message.ConversationId))
            return message.ConversationId;

        // If the user is explicitly replying to a given message
        // We should try to use that conversion first
        // Even if the timeout is expired
        if (!string.IsNullOrEmpty(message.Context))
        {
            var context = await storage.GetMessageAsync(message.UserNumber, message.Context, cancellationToken);
            if (context?.ConversationId is { Length: > 0 } contextConversationId)
                return contextConversationId;
        }

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - ConversationWindowSeconds;

        // Use the conversation id for a message processed in the last ConversationWindowInSeconds seconds
        var conversation = await storage.GetActiveConversationAsync(message.UserNumber, cancellationToken);
        var conversationId = conversation?.Id;

        if (conversationId == null || conversation?.Timestamp < timestamp)
        {
            conversationId = Ulid.NewUlid().ToString();
        }

        return conversationId;
    }
}