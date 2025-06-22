using System.Runtime.CompilerServices;

namespace Devlooped.WhatsApp;

class ConversationStorage : IConversationStorage
{
    static readonly DocumentSerializer defaultSerializer = new DocumentSerializer(JsonContext.DefaultOptions);
    readonly CloudStorageAccount storage;
    readonly Lazy<IDocumentRepository<IMessage>> messagesRepository;
    readonly Lazy<ITableStorage<Conversation>> conversationsRepository;
    readonly Lazy<IDocumentRepository<Conversation>> activeConversationRepository;

    public ConversationStorage(CloudStorageAccount storage)
    {
        this.storage = storage;
        messagesRepository = new(CreateMessagesRepository);
        conversationsRepository = new(CreateConversationsRepository);
        activeConversationRepository = new(CreateActiveConversationRepository);
    }

    protected virtual IDocumentRepository<IMessage> CreateMessagesRepository()
        => DocumentRepository.Create<IMessage>(storage, "WhatsAppMessages", x => x.UserNumber, x => x.Id, defaultSerializer);

    protected virtual ITableStorage<Conversation> CreateConversationsRepository()
        => BlobStorage.Create<Conversation>(storage, "whatsapp-conversations",
            serializer: defaultSerializer);

    protected virtual IDocumentRepository<Conversation> CreateActiveConversationRepository()
        // We only have one active conversation by number. We can use the same table name since no conversation
        // will ever have the ID 'active'
        => DocumentRepository.Create<Conversation>(storage, rowKey: _ => "active", serializer: defaultSerializer);

    /// <inheritdoc/>
    public async Task SaveAsync(IMessage message, CancellationToken cancellationToken = default)
    {
        var data = defaultSerializer.Serialize(message);
        if (!string.IsNullOrEmpty(message.ConversationId))
        {
            var conversation = await conversationsRepository.Value.GetAsync(message.UserNumber, message.ConversationId, cancellationToken) ??
                new(message.UserNumber, message.ConversationId, [], message.Timestamp);

            conversation.Messages.Add(message);

            // Save the conversation
            await conversationsRepository.Value.PutAsync(conversation, cancellationToken);

            // And also the active single entry
            // Avoid saving the messages for the active conversation
            await activeConversationRepository.Value.PutAsync(conversation with { Messages = [] }, cancellationToken);
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
        if (await conversationsRepository.Value.GetAsync(number, conversationId, cancellationToken) is Conversation conversation)
        {
            foreach (var message in conversation.Messages)
            {
                yield return message;
            }
        }
    }

    /// <inheritdoc/>
    public Task<Conversation?> GetActiveConversationAsync(string number, CancellationToken cancellationToken = default)
        => activeConversationRepository.Value.GetAsync(number, "active", cancellationToken);
}