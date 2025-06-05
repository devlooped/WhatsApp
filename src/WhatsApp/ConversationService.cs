using System.Runtime.CompilerServices;

namespace Devlooped.WhatsApp;

class ConversationService(IStorageService storage) : IConversationService
{
    /// <inheritdoc/>
    public async IAsyncEnumerable<IMessage> GetConversationAsync(IMessage message, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        bool shouldReturnInputMessage = true;

        if (!string.IsNullOrEmpty(message.ConversationId))
        {
            var conversation = storage
                    .GetMessagesAsync(message.Number, message.ConversationId, cancellationToken)
                    .OrderBy(x => x.Timestamp);

            await foreach (var conversationMessage in conversation)
            {
                if (string.Equals(conversationMessage.Id, message.Id, StringComparison.OrdinalIgnoreCase))
                    shouldReturnInputMessage = false;

                yield return conversationMessage;
            }
        }

        if (shouldReturnInputMessage)
            yield return message;
    }

    /// <inheritdoc/>
    public async Task<string> GetOrCreateConversationIdAsync(IMessage message, int seconds = 5 * 60, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(message.ConversationId))
            return message.ConversationId;

        // If the user is explicitly replying to a given message
        // We should try to use that conversion first
        // Even if the timeout is expired
        if (!string.IsNullOrEmpty(message.Context))
        {
            var contextMsg = await storage.GetMessageAsync(message.Number, message.Context, cancellationToken);

            if (contextMsg?.ConversationId is string contextConversationId && !string.IsNullOrEmpty(contextConversationId))
                return contextConversationId;
        }

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - seconds;

        // Use the conversation id for a message processed in the last ConversationWindowInSeconds seconds
        var conversation = await storage.GetActiveConversationAsync(message.Number, cancellationToken);
        var conversationId = conversation?.Id;

        if (conversationId == null || conversation?.Timestamp < timestamp)
        {
            conversationId = Ulid.NewUlid().ToString();
        }

        return conversationId;
    }
}