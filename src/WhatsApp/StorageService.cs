namespace Devlooped.WhatsApp;

class StorageService(CloudStorageAccount storage) : IStorageService
{
    const string MessagesTableName = "messages";

    IDocumentRepository<IMessage>? messagesRepository;

    /// <inheritdoc/>
    public async Task SaveAsync(IEnumerable<IMessage> messages, CancellationToken cancellationToken = default)
    {
        var repository = EnsureMessagesRepository();

        foreach (var message in messages.Where(x => !string.IsNullOrEmpty(x.Id)))
        {
            await repository.PutAsync(message, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<IMessage> GetMessagesAsync(string number, CancellationToken cancellationToken = default)
        => EnsureMessagesRepository().EnumerateAsync(number, cancellationToken);

    /// <summary>
    /// Ensures that the repository for storing and retrieving <see cref="Message"/> objects is initialized.    
    /// </summary>
    IDocumentRepository<IMessage> EnsureMessagesRepository()
    {
        messagesRepository ??= DocumentRepository.Create<IMessage>(
            storage,
            MessagesTableName,
            x => x.Number,
            x => x.Id);

        return messagesRepository;
    }
}