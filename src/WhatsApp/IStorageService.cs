namespace Devlooped.WhatsApp;

/// <summary>
/// Defines methods for storing and retrieving messages in an asynchronous manner.  
/// </summary>
/// <remarks>This interface provides functionality to retrieve messages associated with a specific identifier and
/// to save messages or responses to the storage. Implementations of this interface should ensure thread safety and
/// proper handling of cancellation tokens for asynchronous operations.</remarks>
public interface IStorageService
{
    /// <summary>
    /// Retrieves a message by its unique identifier for a specified number.
    /// </summary>
    /// <remarks>This method performs an asynchronous operation to retrieve a message. Ensure that the
    /// provided number and ID are valid and correspond  to an existing message. The operation can be canceled by
    /// passing a cancellation token.</remarks>
    /// <param name="number">The phone number associated with the message. Cannot be null or empty.</param>
    /// <param name="id">The unique identifier of the message to retrieve. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the message associated with the
    /// specified number and ID,  or <see langword="null"/> if no matching message is found.</returns>
    Task<IMessage?> GetMessageAsync(string number, string id, CancellationToken cancellationToken = default);

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
    IAsyncEnumerable<IMessage> GetMessagesAsync(string number, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously saves a collection of messages to the underlying storage.    
    /// </summary>
    /// <remarks>If the operation is canceled via the <paramref name="cancellationToken"/>, the returned task
    /// will be in a canceled state.</remarks>
    /// <param name="messages">The collection of <see cref="Message"/> objects to be saved. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the save operation. The default value is <see
    /// langword="default"/>.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous save operation.</returns>
    Task SaveAsync(IEnumerable<IMessage> messages, CancellationToken cancellationToken = default);

    /// <summary>
    /// Asynchronously saves a collection of messages to the underlying storage.    
    /// </summary>
    /// <remarks>If the operation is canceled via the <paramref name="cancellationToken"/>, the returned task
    /// will be in a canceled state.</remarks>
    /// <param name="message">The <see cref="Message"/> object to be saved. Cannot be null or empty.</param>
    /// <param name="cancellationToken">A <see cref="CancellationToken"/> that can be used to cancel the save operation. The default value is <see
    /// langword="default"/>.</param>
    /// <returns>A <see cref="Task"/> that represents the asynchronous save operation.</returns>
    Task SaveAsync(IMessage message, CancellationToken cancellationToken = default);
}