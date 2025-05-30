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
    public async Task SaveAsync(IEnumerable<IMessage> messages, CancellationToken cancellationToken = default)
    {
        var repository = messagesRepository.Value;

        foreach (var message in messages.Where(x => !string.IsNullOrEmpty(x.Id)))
        {
            await repository.PutAsync(message, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<IMessage> GetMessagesAsync(string number, CancellationToken cancellationToken = default)
        => messagesRepository.Value.EnumerateAsync(number, cancellationToken);
}