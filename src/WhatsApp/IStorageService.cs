namespace Devlooped.WhatsApp;

/// <summary>
/// Defines methods for storing and retrieving messages in an asynchronous manner.  
/// </summary>
/// <remarks>This interface provides functionality to retrieve messages associated with a specific identifier and
/// to save messages or responses to the storage. Implementations of this interface should ensure thread safety and
/// proper handling of cancellation tokens for asynchronous operations.</remarks>
interface IStorageService
{
    /// <summary>
    /// Retrieves a stream of messages associated with the specified phone number.
    /// </summary>
    /// <remarks>This method uses asynchronous streaming to retrieve messages, allowing the caller to process
    /// messages as they are received. Ensure proper handling of the <see cref="IAsyncEnumerable{T}"/> by using `await
    /// foreach` or equivalent constructs.</remarks>
    /// <param name="number">The phone number for which to retrieve messages. This must be a valid phone number in the expected format.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The operation will terminate early if the token is canceled.</param>
    /// <returns>An asynchronous stream of <see cref="Message"/> objects representing the messages associated with the specified
    /// phone number. The stream will be empty if no messages are found.</returns>
    IAsyncEnumerable<Message> GetMessagesAsync(string number, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously saves a collection of messages to the underlying storage.    
    /// </summary>
    /// <remarks>If the operation is canceled via the <paramref name="cancellationToken"/>, the returned task
    /// will be in a canceled state.</remarks>
    /// <param name="messages">The collection of <see cref="Message"/> objects to be saved. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the save operation. The default value is <see
    /// langword="default"/>.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous save operation.</returns>
    Task SaveAsync(IEnumerable<Message> messages, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously saves the specified response to the underlying storage.
    /// </summary>
    /// <remarks>This method performs an asynchronous operation to persist the provided response.  If the
    /// operation is canceled via the <paramref name="cancellationToken"/>, the returned task will be in a canceled
    /// state.</remarks>
    /// <param name="response">The response object to be saved. Cannot be null.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The default value is <see cref="CancellationToken.None"/>.</param>
    /// <returns>A task that represents the asynchronous save operation.</returns>
    Task SaveAsync(Response response, CancellationToken cancellationToken = default);
}