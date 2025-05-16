using System.Net.Http.Json;

namespace Devlooped.WhatsApp;

partial class WhatsAppClientExtensions
{
    /// <summary>
    /// Resolves a media content message to a <see cref="MediaReference"/> object.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="message">A <see cref="ContentMessage"/> with <see cref="MediaContent"/> in <see cref="ContentMessage.Content"/>.</param>
    /// <param name="cancellation">Optional cancellation token.</param>
    /// <returns>The resolved media reference.</returns>
    /// <exception cref="NotSupportedException">The <see cref="ContentMessage.Content"/> is not <see cref="MediaContent"/>.</exception>
    /// <exception cref="GraphMethodException">The media content could not be resolved to a <see cref="MediaReference"/>.</exception>
    /// <exception cref="HttpRequestException">An unknown HTTP exception occurred while resolving the media.</exception>
    public static async Task<MediaReference> ResolveMediaAsync(this IWhatsAppClient client, ContentMessage message, CancellationToken cancellation = default)
    {
        if (message.Content is not MediaContent media)
            throw new NotSupportedException("Message does not contain media.");

        return await ResolveMediaAsync(client, message.To.Id, media.Id, cancellation);
    }

    /// <summary>
    /// Resolves a media identifier to a <see cref="MediaReference"/> object.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="numberId">The service number identifier that received the media.</param>
    /// <param name="mediaId">The media identifier.</param>
    /// <param name="cancellation">Optional cancellation token.</param>
    /// <returns>The resolved media reference.</returns>
    /// <exception cref="GraphMethodException">The media content could not be resolved to a <see cref="MediaReference"/>.</exception>
    /// <exception cref="HttpRequestException">An unknown HTTP exception occurred while resolving the media.</exception>
    /// <exception cref="ArgumentException">The number <paramref name="numberId"/> is not registered in <see cref="MetaOptions"/>.</exception>
    public static async Task<MediaReference> ResolveMediaAsync(this IWhatsAppClient client, string numberId, string mediaId, CancellationToken cancellation = default)
    {
        using var http = client.CreateHttp(numberId);
        var response = await http.GetAsync(mediaId, cancellation);
        await response.Content.LoadIntoBufferAsync();

        if (!response.IsSuccessStatusCode &&
            await response.Content.ReadFromJsonAsync(InternalJsonContext.Default.ErrorResponse, cancellation) is { } error)
            throw error.Error;

        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync(JsonContext.Default.MediaReference, cancellation) ??
            throw new InvalidOperationException("Failed to deserialize media reference.");
    }
}
