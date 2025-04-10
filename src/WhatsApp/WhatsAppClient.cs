﻿using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Devlooped.WhatsApp;

/// <summary>
/// Default implementation of the <see cref="IWhatsAppClient"/>.
/// </summary>
/// <param name="httpFactory">The factory used to make HTTP requests. The name <c>whatsapp</c> is used when creating clients, 
/// which allows customization at the app level.</param>
/// <param name="options">Configuration options for communicating with the service.</param>
/// <param name="logger">A logger for messages.</param>
public class WhatsAppClient(IHttpClientFactory httpFactory, IOptions<MetaOptions> options, ILogger<WhatsAppClient> logger) : IWhatsAppClient
{
    readonly MetaOptions options = options.Value;

    /// <summary>
    /// Creates a new instance of the <see cref="WhatsAppClient"/> class.
    /// </summary>
    /// <remarks>
    /// This method is used mostly in tests so you don't need to create an <see cref="IOptions{MetaOptions}"/>.
    /// </remarks>
    public static IWhatsAppClient Create(IHttpClientFactory httpFactory, MetaOptions options, ILogger<WhatsAppClient> logger)
        => new WhatsAppClient(httpFactory, Options.Create(options), logger);

    /// <inheritdoc />
    public async Task SendAsync(string from, object payload)
    {
        if (!options.Numbers.TryGetValue(from, out var token))
            throw new ArgumentException($"The number '{from}' is not registered in the options.", nameof(from));

        using var http = httpFactory.CreateClient("whatsapp");

        http.DefaultRequestHeaders.TryAddWithoutValidation("Authorization", $"Bearer {token}");
        http.DefaultRequestHeaders.Accept.Add(new MediaTypeWithQualityHeaderValue("application/json"));

        var result = await http.PostAsJsonAsync($"https://graph.facebook.com/{options.ApiVersion}/{from}/messages", payload);

        if (!result.IsSuccessStatusCode)
        {
            var error = JsonNode.Parse(await result.Content.ReadAsStringAsync())?.ToJsonString(new() { WriteIndented = true });
            logger.LogError("Failed to send WhatsApp message from {From}: {Error}", from, error);
            throw new HttpRequestException(error, null, result.StatusCode);
        }
    }
}
