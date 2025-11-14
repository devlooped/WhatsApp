using System.ComponentModel;

namespace Devlooped.WhatsApp.Flows;

public interface IWhatsAppFlowsClient
{
    /// <summary>
    /// Creates an authenticated HTTP client for the given WhatsApp for Business account, with the 
    /// base address of <c>https://graph.facebook.com/{api_version}/</c> as 
    /// configured for it via <see cref="MetaOptions.ApiVersion"/>.
    /// </summary>
    /// <param name="accountId">The configured business account ID to use for authentication via <see cref="MetaOptions.Accounts"/>.</param>
    /// <returns>An HTTP client that can safely be disposed after usage.</returns>
    /// <exception cref="ArgumentException">The account <paramref name="accountId"/> is not registered in <see cref="MetaOptions"/>.</exception>
    HttpClient CreateHttp(string accountId);

    /// <summary>
    /// Sends a raw payload object that must match the WhatsApp API.
    /// </summary>
    /// <param name="accountId">The business account identifier to send the payload to, which must be configured via <see cref="MetaOptions.Accounts"/>.</param>
    /// <param name="payload">The message payload.</param>>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>The message id that was sent/reacted/marked, if any.</returns>
    /// <see cref="https://developers.facebook.com/docs/whatsapp/flows/reference/flowsapi"/>
    /// <exception cref="ArgumentException">The account <paramref name="accountId"/> is not registered in <see cref="MetaOptions"/>.</exception>
    /// <exception cref="HttpRequestException">The HTTP request failed. Exception message contains the error response body from WhatsApp.</exception>
    [Description(nameof(Devlooped) + nameof(WhatsApp) + nameof(IWhatsAppFlowsClient) + nameof(SendAsync))]
    Task<string?> SendAsync(string accountId, object payload, CancellationToken cancellationToken = default);
}
