namespace Devlooped.WhatsApp;

class StorageService(CloudStorageAccount storage) : IStorageService
{
    const string MessagesTableName = "messages";

    Lazy<IDocumentRepository<IMessage>> messagesRepository = new(() =>
        DocumentRepository.Create<IMessage>(
            storage,
            MessagesTableName,
            x => x.Number,
            x => x.Id));

    /// <inheritdoc/>
    public Task SaveAsync(IMessage message, CancellationToken cancellationToken = default)
        => messagesRepository.Value.PutAsync(message, cancellationToken);

    /// <inheritdoc/>
    public Task SaveAsync(IEnumerable<IMessage> messages, CancellationToken cancellationToken = default)
        => messagesRepository.Value.PutAsync(messages, cancellationToken);

    /// <inheritdoc/>
    public IAsyncEnumerable<IMessage> GetMessagesAsync(string number, CancellationToken cancellationToken = default)
        => messagesRepository.Value.EnumerateAsync(number, cancellationToken);
}