using System.Runtime.CompilerServices;
using System.Text.Json;
using Microsoft.Extensions.DependencyInjection;

namespace Devlooped.WhatsApp;

static class ConsoleRenderExtensions
{
    /// <summary>
    /// Renders text message and responses to the console as JSON for 
    /// troubleshooting.
    /// </summary>
    public static WhatsAppHandlerBuilder UseConsoleRender(this WhatsAppHandlerBuilder builder)
        => builder.Use((inner, services)
            => new ConsoleRenderHandler(inner, services.GetRequiredService<IWhatsAppClient>()));

    /// <summary>
    /// Checks whether the given service number ID is a CLI local endpoint.
    /// </summary>
    static bool IsCLI(this string serviceId)
        => serviceId.StartsWith("http://localhost", StringComparison.OrdinalIgnoreCase);

    class ConsoleRenderHandler(IWhatsAppHandler inner, IWhatsAppClient client) : DelegatingWhatsAppHandler(inner)
    {
        public override async IAsyncEnumerable<Response> HandleAsync(IEnumerable<IMessage> messages, [EnumeratorCancellation] CancellationToken cancellation = default)
        {
            var user = messages.OfType<UserMessage>().LastOrDefault();
            if (user != null && user.Service.Id.IsCLI())
            {
                await client.SendAsync(user,
                    $"""
                    ```json
                    {JsonSerializer.Serialize(
                        messages.Where(x => x is UserMessage || x is TextResponse),
                        JsonContext.DefaultOptions)}
                    ```
                    """);
            }

            var responses = new List<TextResponse>();

            await foreach (var response in base.HandleAsync(messages, cancellation))
            {
                if (response is TextResponse text)
                    responses.Add(text);

                yield return response;
            }

            if (user != null && user.Service.Id.IsCLI())
            {
                await client.SendAsync(user,
                    $"""
                    ```json
                    {JsonSerializer.Serialize(responses, JsonContext.DefaultOptions)}
                    ```
                    """);
            }
        }
    }
}