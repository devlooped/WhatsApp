using System.Net;
using System.Text;
using System.Text.Json;
using Devlooped.WhatsApp.Client;
using DotNetConfig;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using Spectre.Console.Json;

namespace Devlooped.WhatsApp;

enum RenderMode
{
    Yaml,
    Json,
    Text,
}

[Service]
class Interactive(IConfiguration configuration, IHttpClientFactory httpFactory) : IHostedService
{
    readonly CancellationTokenSource cts = new();

    string? serviceEndpoint = configuration["WhatsApp:Endpoint"];
    string? clientEndpoint;
    HttpListener? listener;
    RenderMode mode = RenderMode.Text;
    bool needsNewline = true;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(serviceEndpoint))
        {
            serviceEndpoint = AnsiConsole.Ask("Enter WhatsApp functions endpoint", "http://localhost:4242/whatsappcli");
            Config.Build(ConfigLevel.Global)
                .SetString("WhatsApp", "Endpoint", serviceEndpoint);
        }
        else if (!AnsiConsole.Confirm($"Use WhatsApp functions endpoint [link]{serviceEndpoint}[/]"))
        {
            serviceEndpoint = AnsiConsole.Ask("Enter WhatsApp functions endpoint", "http://localhost:4242/whatsappcli");
            Config.Build(ConfigLevel.Global)
                .SetString("WhatsApp", "Endpoint", serviceEndpoint);
        }

        var choices = Enum.GetValues<MessageType>();
        mode = AnsiConsole.Prompt(
            new SelectionPrompt<RenderMode>()
                .Title("Select render mode")
                .AddChoices([RenderMode.Text, RenderMode.Yaml, RenderMode.Json]));

        listener = new HttpListener();
        // Attempt to grab the first free port we can find on localhost
        while (true)
        {
            try
            {
                clientEndpoint = $"http://localhost:{Random.Shared.Next(5000, 6000)}/";
                listener.Prefixes.Add(clientEndpoint);
                listener.Start();
                break;
            }
            catch (HttpListenerException)
            {
                // Port is already in use, try another one
                listener.Prefixes.Clear();
            }
        }

        _ = Task.Run(ResponseListener, cancellationToken);
        _ = Task.Run(InputListener, cancellationToken);

        return Task.CompletedTask;
    }

    public Task StopAsync(CancellationToken cancellationToken)
    {
        cts.Cancel();
        AnsiConsole.MarkupLine($":robot: Stopping");
        return Task.CompletedTask;
    }

    async Task InputListener()
    {
        AnsiConsole.MarkupLine($":robot: Ready");
        AnsiConsole.Markup($":person_beard: ");

        while (!cts.IsCancellationRequested)
        {
            var input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
            {
                needsNewline = false;
                try
                {
                    var message = new ContentMessage(
                        Id: Ulid.NewUlid().ToString(),
                        Service: new Service(clientEndpoint!, "123456789"),
                        User: new User("Console", "987654321"),
                        Timestamp: DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                        Content: new TextContent(input)
                    );

                    using var httpClient = httpFactory.CreateClient("whatsapp");
                    var payload = JsonSerializer.Serialize(message, JsonContext.Default.Message);

                    var response = await httpClient.PostAsync(serviceEndpoint, new StringContent(payload, Encoding.UTF8, "application/json"));
                    if (!response.IsSuccessStatusCode)
                    {
                        AnsiConsole.MarkupLine($"[red] Failed to send message.[/] [bold]Status Code:[/] {response.StatusCode}");
                    }

                    AnsiConsole.Markup($":person_beard: ");
                    needsNewline = true;
                }
                catch (Exception e)
                {
                    AnsiConsole.WriteException(e);
                }
            }
        }
    }

    async Task ResponseListener()
    {
        while (!cts.IsCancellationRequested)
        {
            var context = await listener!.GetContextAsync();

            try
            {
                var request = context.Request;
                var response = context.Response;

                // Read the request body (if any)
                var requestBody = "{}";
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                await RenderAsync(requestBody);

                var buffer = Encoding.UTF8.GetBytes("OK");
                response.ContentLength64 = buffer.Length;
                response.ContentType = "text/plain";

                await response.OutputStream.WriteAsync(buffer);
                response.OutputStream.Close();
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing request: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.Close();
            }
        }
    }

    async Task RenderAsync(string json)
    {
        if (needsNewline)
            AnsiConsole.WriteLine();

        // Try to parse the request body as a dictionary and render it as YAML
        if (mode == RenderMode.Yaml &&
            DictionaryConverter.Parse(json) is { } dictionary &&
            DictionaryConverter.ToYaml(dictionary) is { Length: > 0 } payload)
        {
            AnsiConsole.Write(new Panel(payload)
            {
                Width = Math.Min(100, AnsiConsole.Profile.Width)
            });
            return;
        }

        if (mode == RenderMode.Text)
        {
            try
            {
                // Move discriminator to top.
                json = await JQ.ExecuteAsync(json, "{ \"$type\": .type } + .");

                if (JsonSerializer.Deserialize(json, ClientContext.Default.ClientMessage) is { } message &&
                    message.ToString() is { } text)
                {
                    // Don't render empty reaction since it's the clearing of the emoji actually in WhatsApp
                    if (message.Type == Client.MessageType.Reaction && text.Length == 0)
                        return;

                    AnsiConsole.Write(new Panel(Markup.FromInterpolated($":robot: {text}"))
                    {
                        Border = BoxBorder.None,
                        Expand = false,
                        Padding = new(0, 0, 0, 0),
                        Width = Math.Min(80, AnsiConsole.Profile.Width),
                    });
                    if (message is Client.InteractiveMessage interactive && interactive.Interactive.Action is { } node)
                    {
                        AnsiConsole.Write(new Panel(DictionaryConverter.Parse(node.ToString()).ToYaml(true))
                        {
                            Width = Math.Min(60, AnsiConsole.Profile.Width)
                        });
                    }
                    return;
                }
            }
            catch (JsonException e)
            {
                AnsiConsole.MarkupLineInterpolated($"[grey]{e.Message}[/]");
            }
        }

        AnsiConsole.Write(new Panel(new JsonText(json))
        {
            Width = Math.Min(100, AnsiConsole.Profile.Width)
        });
    }
}
