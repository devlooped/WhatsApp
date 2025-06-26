using System.Runtime.CompilerServices;
using Microsoft.Extensions.Options;

namespace Devlooped.WhatsApp;


class ConversationHandler(IWhatsAppHandler inner, IConversationStorage storage, IOptions<WhatsAppOptions> options) : DelegatingWhatsAppHandler(inner)
{
    readonly WhatsAppOptions options = options.Value;

    public int ConversationWindowSeconds { get; set; } = options.Value.ConversationWindowSeconds;

    public override async IAsyncEnumerable<Response> HandleAsync(IEnumerable<IMessage> messages, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        var conversation = new List<IMessage>();

        if (options.ReactOnConversation != null && messages.LastOrDefault() is ContentMessage content)
            yield return content.React(options.ReactOnConversation);

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
            // We don't care about typing status or reaction messages for conversation storage
            if (response is not TypingResponse or ReactionResponse)
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