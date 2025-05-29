namespace Devlooped.WhatsApp;

class StorageService(CloudStorageAccount storage) : IStorageService
{
    const string MessagesTableName = "messages";

    IDocumentRepository<Message>? messagesRepository;

    /// <inheritdoc/>
    public async Task SaveAsync(IEnumerable<Message> messages, CancellationToken cancellationToken = default)
    {
        var repository = EnsureMessagesRepository();

        foreach (var message in messages)
        {
            await repository.PutAsync(message, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public async Task SaveAsync(Response response, CancellationToken cancellationToken = default)
    {
        if (response.AsMessage() is Message responseMessage)
        {
            await EnsureMessagesRepository().PutAsync(responseMessage, cancellationToken);
        }
    }

    /// <inheritdoc/>
    public IAsyncEnumerable<Message> GetMessagesAsync(string number, CancellationToken cancellationToken = default)
        => EnsureMessagesRepository().EnumerateAsync(number, cancellationToken);

    /// <summary>
    /// Ensures that the repository for storing and retrieving <see cref="Message"/> objects is initialized.    
    /// </summary>
    IDocumentRepository<Message> EnsureMessagesRepository()
    {
        messagesRepository ??= DocumentRepository.Create<Message>(
            storage,
            MessagesTableName,
            x => x.From.Number,
            x => x.Id);

        return messagesRepository;
    }
}