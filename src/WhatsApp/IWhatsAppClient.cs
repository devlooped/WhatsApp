using System.ComponentModel;

namespace Devlooped.WhatsApp;

/// <summary>
/// Interface for WhatsApp for Business client API.
/// </summary>
public interface IWhatsAppClient
{
    /// <summary>
    /// Creates an authenticated HTTP client for the given number, with the 
    /// base address of <c>https://graph.facebook.com/{api_version}/</c> as 
    /// configured for it via <see cref="MetaOptions.ApiVersion"/>.
    /// </summary>
    /// <param name="numberId">The configured number ID to use for authentication via <see cref="MetaOptions.Numbers"/>.</param>
    /// <returns>An HTTP client that can safely be disposed after usage.</returns>
    /// <exception cref="ArgumentException">The number <paramref name="numberId"/> is not registered in <see cref="MetaOptions"/>.</exception>
    HttpClient CreateHttp(string numberId);

    /// <summary>
    /// Sends a raw payload object that must match the WhatsApp API.
    /// </summary>
    /// <param name="numberId">The phone identifier to send the message from, which must be configured via <see cref="MetaOptions.Numbers"/>.</param>
    /// <param name="payload">The message payload.</param>
    /// <returns>The message id that was sent/reacted/marked, if any.</returns>
    /// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages"/>
    /// <exception cref="ArgumentException">The number <paramref name="numberId"/> is not registered in <see cref="MetaOptions"/>.</exception>
    /// <exception cref="HttpRequestException">The HTTP request failed. Exception message contains the error response body from WhatsApp.</exception>
    [Description(nameof(Devlooped) + nameof(WhatsApp) + nameof(IWhatsAppClient) + nameof(SendAsync))]
    Task<string?> SendAsync(string numberId, object payload);
}