using System.Runtime.CompilerServices;
using Microsoft.Extensions.Logging;

namespace Devlooped.WhatsApp;

class ConversationService(IStorageService storageService, ILogger<ConversationService> logger) : IConversationService
{
    /// <inheritdoc/>
    public async IAsyncEnumerable<IMessage> GetConversationAsync(IMessage message, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        bool shouldReturnInputMessage = true;

        if (!string.IsNullOrEmpty(message.ConversationId))
        {
            var conversation = storageService
                    .GetMessagesAsync(message.Number, cancellationToken)
                    .Where(x => x.ConversationId == message.ConversationId)
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

        var timestamp = DateTimeOffset.UtcNow.ToUnixTimeSeconds() - seconds;

        // Use the conversation id for a message processed in the last ConversationWindowInSeconds seconds
        var result = (await storageService
            .GetMessagesAsync(message.Number, cancellationToken)
            .Where(x => x.ConversationId != null && x.Timestamp > timestamp)
            .OrderBy(x => x.Timestamp)
            .LastOrDefaultAsync())?.ConversationId;

        if (result == null)
        {
            result = Ulid.NewUlid().ToString();

            logger.LogDebug("Created Conversation Id: {Id}", result);
        }

        return result;
    }
}