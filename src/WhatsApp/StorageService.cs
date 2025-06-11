using System.Runtime.CompilerServices;
using Microsoft.FeatureManagement;

namespace Devlooped.WhatsApp;

class StorageService(CloudStorageAccount storage, IFeatureManager featureManager) : IStorageService
{
    readonly List<IMessage> EmptyList = new();

    const string MessagesTableName = "WhatsAppMessages";
    const string ConversationsTableName = "WhatsAppConversations";

    Lazy<IDocumentRepository<IMessage>> messagesRepository = new(() =>
        DocumentRepository.Create<IMessage>(
            storage,
            MessagesTableName,
            x => x.UserNumber,
            x => x.Id));

    Lazy<IDocumentRepository<Conversation>> conversationsRepository = new(() =>
        DocumentRepository.Create<Conversation>(
            storage,
            ConversationsTableName,
            x => x.Number,
            x => x.Id));

    Lazy<IDocumentRepository<Conversation>> activeConversationRepository = new(() =>
        DocumentRepository.Create<Conversation>(
            storage,
            ConversationsTableName,
            x => x.Number,
            // We only have one active conversation by number
            // NOTE: we can use the same table name since no conversation will 
            // ever have the ID 'active'
            x => "active"));

    /// <inheritdoc/>
    public async Task SaveAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        if (!string.IsNullOrEmpty(message.ConversationId) && await featureManager.IsEnabledAsync(FeatureFlags.Conversation))
        {
            var conversation = await conversationsRepository.Value.GetAsync(message.UserNumber, message.ConversationId, cancellationToken) ??
                new(message.UserNumber, message.ConversationId, new(), message.Timestamp);

            conversation.Messages.Add(message);

            // Save the conversation
            await conversationsRepository.Value.PutAsync(conversation, cancellationToken);

            // And also the active single entry
            // Avoid saving the messages for the active conversation
            await activeConversationRepository.Value.PutAsync(conversation with { Messages = EmptyList }, cancellationToken);
        }

        await messagesRepository.Value.PutAsync(message, cancellationToken);
    }

    /// <inheritdoc/>
    public Task<IMessage?> GetMessageAsync(string number, string id, CancellationToken cancellationToken = default)
        => messagesRepository.Value.GetAsync(number, id, cancellationToken);

    /// <inheritdoc/>
    public IAsyncEnumerable<IMessage> GetMessagesAsync(string number, CancellationToken cancellationToken = default)
        => messagesRepository.Value.EnumerateAsync(number, cancellationToken);

    /// <inheritdoc/>
    public async IAsyncEnumerable<IMessage> GetMessagesAsync(string number, string conversationId, [EnumeratorCancellation] CancellationToken cancellationToken = default)
    {
        if (await featureManager.IsEnabledAsync(FeatureFlags.Conversation))
        {
            if (await conversationsRepository.Value.GetAsync(number, conversationId, cancellationToken) is Conversation conversation)
            {
                foreach (var message in conversation.Messages)
                {
                    yield return message;
                }
            }
        }
    }

    /// <inheritdoc/>
    public Task<Conversation?> GetActiveConversationAsync(string number, CancellationToken cancellationToken = default)
        => activeConversationRepository.Value.GetAsync(number, "active", cancellationToken);
}