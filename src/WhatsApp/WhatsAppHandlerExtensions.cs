using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Devlooped.WhatsApp;

/// <summary>
/// Provides the <see cref="AsBuilder"/> extension method to build a pipeline 
/// around a given handler.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WhatsAppHandlerExtensions
{
    /// <summary>
    /// Creates a new <see cref="WhatsAppHandlerBuilder"/> using <paramref name="handler"/> as its inner handler.
    /// </summary>
    /// <remarks>
    /// This method is equivalent to using the <see cref="WhatsAppHandlerBuilder"/> constructor directly.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/>.</exception>
    public static WhatsAppHandlerBuilder AsBuilder(this IWhatsAppHandler handler)
    {
        Throw.IfNull(handler);
        return new(_ => handler);
    }

    /// <summary>
    /// Provides an easy way to handle errors and completion in an asynchronous enumerator.
    /// </summary>
    /// <param name="responsesEnumeratorProvider">The function that provides the async enumerator for responses.</param>
    /// <param name="errorCallback">The callback to invoke on error.</param>
    /// <param name="completionCallback">The callback to invoke upon completion.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns></returns>
    internal static async IAsyncEnumerable<Response> WithErrorHandlingAsync(
        this IAsyncEnumerable<Response> responses,
        Action<Exception>? errorCallback = default,
        Action? completionCallback = default,
        Action? finallyCallback = default,
        [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        IAsyncEnumerator<Response> responsesEnumerator;
        try
        {
            responsesEnumerator = responses.GetAsyncEnumerator();
        }
        catch (Exception ex)
        {
            errorCallback?.Invoke(ex);
            throw;
        }

        try
        {
            Response? currentResponse = null;

            while (true)
            {
                try
                {
                    if (!await responsesEnumerator.MoveNextAsync())
                    {
                        break;
                    }

                    currentResponse = responsesEnumerator.Current;
                }
                catch (Exception ex)
                {
                    errorCallback?.Invoke(ex);
                    throw;
                }

                yield return currentResponse;
            }

            completionCallback?.Invoke();
        }
        finally
        {
            finallyCallback?.Invoke();

            await responsesEnumerator.DisposeAsync();
        }
    }
}