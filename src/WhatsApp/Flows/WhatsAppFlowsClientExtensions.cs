using System.ComponentModel;
using System.Net.Http.Json;
using System.Runtime.CompilerServices;

namespace Devlooped.WhatsApp.Flows;

/// <summary>
/// Usability extensions for Flows API.
/// </summary>
[EditorBrowsable(EditorBrowsableState.Never)]
public static class WhatsAppFlowsClientExtensions
{
    /// <summary>
    /// Validates Flow JSON locally before submission, using JSON Schema 
    /// and programmatic semantic rules.
    /// </summary>
    /// <param name="client">The WhatsApp Flows client (unused, provides API discoverability).</param>
    /// <param name="flowJson">The Flow JSON content to validate.</param>
    /// <returns>Validation result with any errors found.</returns>
    public static FlowValidationResult ValidateFlowJson(this IWhatsAppClient client, string flowJson)
        => new FlowJsonValidator().Validate(flowJson);

    /// <summary>
    /// Updates Flow JSON with optional local pre-validation.
    /// </summary>
    /// <param name="client">The WhatsApp Flows client.</param>
    /// <param name="accountId">The WhatsApp Business Account ID.</param>
    /// <param name="flowId">The Flow ID.</param>
    /// <param name="flowJson">The Flow JSON content.</param>
    /// <param name="validate">Whether to validate locally before submitting. Defaults to <see langword="false"/>.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns>The update flow JSON response.</returns>
    /// <exception cref="FlowValidationException">Thrown when local validation fails and <paramref name="validate"/> is <see langword="true"/>.</exception>
    public static async Task<UpdateFlowJsonResponse> UpdateFlowJsonAsync(this IWhatsAppClient client, string accountId, string flowId, string flowJson, bool validate, CancellationToken cancellation = default)
    {
        if (validate)
        {
            var result = new FlowJsonValidator().Validate(flowJson);
            if (!result.IsValid)
                throw new FlowValidationException(result);
        }

        return await client.UpdateFlowJsonAsync(accountId, flowId, flowJson, cancellation);
    }
    /// <summary>
    /// Creates a new Flow.
    /// </summary>
    /// <param name="client">The WhatsApp Flows client.</param>
    /// <param name="accountId">The WhatsApp Business Account ID.</param>
    /// <param name="request">The create flow request.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns>The create flow response.</returns>
    public static async Task<CreateFlowResponse> CreateFlowAsync(this IWhatsAppClient client, string accountId, CreateFlowRequest request, CancellationToken cancellation = default)
    {
        using var http = client.CreateHttp(accountId);
        var response = await http.PostAsJsonAsync($"{accountId}/flows", request, FlowsJsonContext.DefaultOptions, cancellation);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<CreateFlowResponse>(FlowsJsonContext.DefaultOptions, cancellation) ?? throw new InvalidOperationException("Invalid response");
    }

    /// <summary>
    /// Updates Flow metadata.
    /// </summary>
    /// <param name="client">The WhatsApp Flows client.</param>
    /// <param name="accountId">The WhatsApp Business Account ID.</param>
    /// <param name="request">The update flow request.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns>The update flow response.</returns>
    public static async Task<bool> UpdateFlowAsync(this IWhatsAppClient client, string accountId, UpdateFlowMetadataRequest request, CancellationToken cancellation = default)
    {
        using var http = client.CreateHttp(accountId);
        var response = await http.PostAsJsonAsync(request.Id, request, FlowsJsonContext.DefaultOptions, cancellation);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<SuccessResponse>(FlowsJsonContext.DefaultOptions, cancellation) ?? throw new InvalidOperationException("Invalid response");

        return payload.Success;
    }

    /// <summary>
    /// Updates Flow JSON.
    /// </summary>
    /// <param name="client">The WhatsApp Flows client.</param>
    /// <param name="accountId">The WhatsApp Business Account ID.</param>
    /// <param name="flowId">The Flow ID.</param>
    /// <param name="flowJson">The Flow JSON content.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns>The update flow JSON response.</returns>
    public static async Task<UpdateFlowJsonResponse> UpdateFlowJsonAsync(this IWhatsAppClient client, string accountId, string flowId, string flowJson, CancellationToken cancellation = default)
    {
        using var http = client.CreateHttp(accountId);
        using var content = new MultipartFormDataContent
        {
            { new StringContent("flow.json"), "name" },
            { new StringContent("FLOW_JSON"), "asset_type" },
            { new StringContent(flowJson, System.Text.Encoding.UTF8, "application/json"), "file", "flow.json" }
        };
        var response = await http.PostAsync($"{flowId}/assets", content, cancellation);
        response.EnsureSuccessStatusCode();
        return await response.Content.ReadFromJsonAsync<UpdateFlowJsonResponse>(FlowsJsonContext.DefaultOptions, cancellation) ?? throw new InvalidOperationException("Invalid response");
    }

    /// <summary>
    /// Gets the Flow preview.
    /// </summary>
    /// <param name="client">The WhatsApp Flows client.</param>
    /// <param name="accountId">The WhatsApp Business Account ID.</param>
    /// <param name="flowId">The Flow ID.</param>
    /// <param name="invalidate">Whether to invalidate the current preview.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns>The flow preview response.</returns>
    public static async Task<FlowPreview> GetFlowPreviewAsync(this IWhatsAppClient client, string accountId, string flowId, bool invalidate = false, CancellationToken cancellation = default)
    {
        using var http = client.CreateHttp(accountId);
        var response = await http.GetAsync($"{flowId}?fields=preview.invalidate({invalidate.ToString().ToLowerInvariant()})", cancellation);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<GetFlowPreviewResponse>(FlowsJsonContext.DefaultOptions, cancellation) ?? throw new InvalidOperationException("Invalid response");

        return payload.Preview;
    }

    /// <summary>
    /// Deletes a Flow.
    /// </summary>
    /// <param name="client">The WhatsApp Flows client.</param>
    /// <param name="accountId">The WhatsApp Business Account ID.</param>
    /// <param name="flowId">The Flow ID.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns>The delete flow response.</returns>
    public static async Task<bool> DeleteFlowAsync(this IWhatsAppClient client, string accountId, string flowId, CancellationToken cancellation = default)
    {
        using var http = client.CreateHttp(accountId);
        var response = await http.DeleteAsync(flowId, cancellation);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<SuccessResponse>(FlowsJsonContext.DefaultOptions, cancellation) ?? throw new InvalidOperationException("Invalid response");

        return payload.Success;
    }

    /// <summary>
    /// Gets the list of Flows, handling pagination internally.
    /// </summary>
    /// <param name="client">The WhatsApp Flows client.</param>
    /// <param name="accountId">The WhatsApp Business Account ID.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns>An async enumerable of flows.</returns>
    public static async IAsyncEnumerable<Flow> GetFlowsAsync(this IWhatsAppClient client, string accountId, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        string? after = null;
        using var http = client.CreateHttp(accountId);

        do
        {
            var queryString = !string.IsNullOrEmpty(after) ? $"?after={Uri.EscapeDataString(after)}" : "";
            var response = await http.GetAsync($"{accountId}/flows{queryString}", cancellation);
            response.EnsureSuccessStatusCode();
            var page = await response.Content.ReadFromJsonAsync<GetFlowsResponse>(FlowsJsonContext.DefaultOptions, cancellation) ?? throw new InvalidOperationException("Invalid response");

            foreach (var flow in page.Data ?? Enumerable.Empty<Flow>())
                yield return flow;

            after = page.Paging?.Cursors?.After;

        } while (!string.IsNullOrEmpty(after));
    }

    /// <summary>
    /// Gets Flow details.
    /// </summary>
    /// <param name="client">The WhatsApp Flows client.</param>
    /// <param name="accountId">The WhatsApp Business Account ID.</param>
    /// <param name="flowId">The Flow ID.</param>
    /// <param name="fields">Fields to retrieve.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns>The get flow response.</returns>
    public static async Task<FlowDetails> GetFlowAsync(this IWhatsAppClient client, string accountId, string flowId, string? fields = null, CancellationToken cancellation = default)
    {
        using var http = client.CreateHttp(accountId);
        var query = string.IsNullOrEmpty(fields) ? "" : $"?fields={Uri.EscapeDataString(fields)}";
        var response = await http.GetAsync($"{flowId}{query}", cancellation);
        response.EnsureSuccessStatusCode();

        return await response.Content.ReadFromJsonAsync<FlowDetails>(FlowsJsonContext.DefaultOptions, cancellation) ?? throw new InvalidOperationException("Invalid response");
    }

    /// <summary>
    /// Gets Flow assets.
    /// </summary>
    /// <param name="client">The WhatsApp Flows client.</param>
    /// <param name="accountId">The WhatsApp Business Account ID.</param>
    /// <param name="flowId">The Flow ID.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns>The get flow assets response.</returns>
    public static async IAsyncEnumerable<FlowAsset> GetFlowAssetsAsync(this IWhatsAppClient client, string accountId, string flowId, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        string? after = null;
        using var http = client.CreateHttp(accountId);

        do
        {
            var queryString = !string.IsNullOrEmpty(after) ? $"?after={Uri.EscapeDataString(after)}" : "";
            var response = await http.GetAsync($"{flowId}/assets", cancellation);
            response.EnsureSuccessStatusCode();
            var page = await response.Content.ReadFromJsonAsync<GetFlowAssetsResponse>(FlowsJsonContext.DefaultOptions, cancellation) ?? throw new InvalidOperationException("Invalid response");

            foreach (var flow in page.Data ?? Enumerable.Empty<FlowAsset>())
                yield return flow;

            after = page.Paging?.Cursors?.After;

        } while (!string.IsNullOrEmpty(after));
    }

    /// <summary>
    /// Publishes a Flow.
    /// </summary>
    /// <param name="client">The WhatsApp Flows client.</param>
    /// <param name="accountId">The WhatsApp Business Account ID.</param>
    /// <param name="flowId">The Flow ID.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns>The publish flow response.</returns>
    public static async Task<bool> PublishFlowAsync(this IWhatsAppClient client, string accountId, string flowId, CancellationToken cancellation = default)
    {
        using var http = client.CreateHttp(accountId);
        var response = await http.PostAsync($"{flowId}/publish", null, cancellation);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<SuccessResponse>(FlowsJsonContext.DefaultOptions, cancellation) ?? throw new InvalidOperationException("Invalid response");

        return payload.Success;
    }

    /// <summary>
    /// Deprecates a Flow.
    /// </summary>
    /// <param name="client">The WhatsApp Flows client.</param>
    /// <param name="accountId">The WhatsApp Business Account ID.</param>
    /// <param name="flowId">The Flow ID.</param>
    /// <param name="cancellation">The cancellation token.</param>
    /// <returns>The deprecate flow response.</returns>
    public static async Task<bool> DeprecateFlowAsync(this IWhatsAppClient client, string accountId, string flowId, CancellationToken cancellation = default)
    {
        using var http = client.CreateHttp(accountId);
        var response = await http.PostAsync($"{flowId}/deprecate", null, cancellation);
        response.EnsureSuccessStatusCode();

        var payload = await response.Content.ReadFromJsonAsync<SuccessResponse>(FlowsJsonContext.DefaultOptions, cancellation) ?? throw new InvalidOperationException("Invalid response");
        return payload.Success;
    }
}
