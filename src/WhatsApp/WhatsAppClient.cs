using System;
using System.Net.Http;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text.Json.Nodes;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;

namespace Devlooped.WhatsApp;

public class WhatsAppClient(IHttpClientFactory httpFactory, IOptions<MetaOptions> options, ILogger<WhatsAppClient> logger) : IWhatsAppClient
{
    readonly MetaOptions options = options.Value;

    public static IWhatsAppClient Create(IHttpClientFactory httpFactory, MetaOptions options, ILogger<WhatsAppClient> logger)
        => new WhatsAppClient(httpFactory, Options.Create(options), logger);

    public async Task<bool> SendAync(string from, object payload)
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
            logger.LogError("Failed to send WhatsApp message: {error}", error);
            return false;
        }

        return true;
    }
}
