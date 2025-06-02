namespace Devlooped.WhatsApp;

/// <summary>
/// Defines methods for managing and retrieving conversations in an asynchronous manner.
/// </summary>
/// <remarks>This interface provides functionality to retrieve conversation messages and manage conversation
/// identifiers. Implementations should ensure thread safety and proper handling of asynchronous operations.</remarks>
public interface IConversationService
{
    /// <summary>
    /// Retrieves a conversation thread starting from the specified message.
    /// </summary>
    /// <remarks>This method uses asynchronous streaming to retrieve messages in the conversation thread.
    /// Callers can enumerate the returned messages using an <c>await foreach</c> loop.</remarks>
    /// <param name="message">The message from which to begin retrieving the conversation. This cannot be <see langword="null"/>.</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests. The operation will terminate early if the token is canceled.</param>
    /// <returns>An asynchronous stream of messages representing the conversation thread, starting from the specified message.
    /// The sequence will be empty if no messages are found.</returns>
    IAsyncEnumerable<IMessage> GetConversationAsync(IMessage message, CancellationToken cancellationToken = default);

    /// <summary>
    /// Retrieves the conversation ID associated with the specified message, or creates a new one if none exists.
    /// </summary>
    /// <param name="message">The message for which to retrieve or create a conversation ID. Cannot be <see langword="null"/>.</param>
    /// <param name="seconds">The duration, in seconds, for which the conversation ID should remain valid. Defaults to 5 minutes (300 seconds).</param>
    /// <param name="cancellationToken">A token to monitor for cancellation requests.</param>
    /// <returns>A task that represents the asynchronous operation. The task result contains the conversation ID as a string.</returns>
    Task<string> GetOrCreateConversationIdAsync(IMessage message, int seconds = 5 * 60, CancellationToken cancellationToken = default);
}