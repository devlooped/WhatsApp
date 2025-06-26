using System.Net;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;
using Devlooped.WhatsApp.Client;
using DotNetConfig;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Spectre.Console;
using Spectre.Console.Json;
using Spectre.Console.Rendering;
using Timer = System.Timers.Timer;

namespace Devlooped.WhatsApp;

[Service]
partial class Interactive(IConfiguration configuration, IHttpClientFactory httpFactory) : IHostedService
{
    readonly CancellationTokenSource cts = new();

    string? service = configuration["whatsapp:endpoint"];
    string? number = configuration["whatsapp:number"];
    OutputFormat? format = Enum.TryParse<OutputFormat>(configuration["whatsApp:format"], true, out var value) ? value : null;
    string? clientEndpoint;
    HttpListener? listener;
    bool needsNewline = true;
    Timer? personTimer = null;

    public Task StartAsync(CancellationToken cancellationToken)
    {
        if (string.IsNullOrEmpty(service))
        {
            service = AnsiConsole.Ask("Enter WhatsApp functions endpoint", "http://localhost:4242/whatsappcli");
            Config.Build(ConfigLevel.Global)
                .SetString("whatsapp", "endpoint", service);
        }
        if (format == null)
        {
            var choices = Enum.GetValues<MessageType>();
            format = AnsiConsole.Prompt(
                new SelectionPrompt<OutputFormat>()
                    .Title("Select output format")
                    .AddChoices([OutputFormat.Text, OutputFormat.Yaml, OutputFormat.Json]));

            Config.Build(ConfigLevel.Global)
                .SetString("whatsapp", "format", format.ToString()!.ToLowerInvariant());
        }
        if (number == null)
        {
            number = AnsiConsole.Ask<long>("Enter WhatsApp user phone number", 987654321).ToString();
            Config.Build(ConfigLevel.Global)
                .SetString("whatsapp", "number", number);
        }

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
                listener = new HttpListener();
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
        personTimer = new Timer { AutoReset = false };
        // Initially non-started
        personTimer.Elapsed += (sender, e) =>
        {
            AnsiConsole.Markup($":person_beard: ");
            needsNewline = true;
            personTimer.Stop();
        };

        while (!cts.IsCancellationRequested)
        {
            var input = Console.ReadLine();
            if (!string.IsNullOrWhiteSpace(input))
            {
                needsNewline = false;
                try
                {
                    if (input.Trim() is "cls" or "clear")
                    {
                        Console.Clear();
                    }
                    else
                    {
                        var message = new ContentMessage(
                            Id: Ulid.NewUlid().ToString(),
                            Service: new Service(clientEndpoint!, "123456789"),
                            User: new User("Console", number ?? "987654321"),
                            Timestamp: DateTimeOffset.UtcNow.ToUnixTimeSeconds(),
                            Content: new TextContent(input)
                        );

                        using var httpClient = httpFactory.CreateClient("whatsapp");
                        var payload = JsonSerializer.Serialize(message, JsonContext.Default.Message);

                        var response = await httpClient.PostAsync(service, new StringContent(payload, Encoding.UTF8, "application/json"));
                        if (!response.IsSuccessStatusCode)
                        {
                            AnsiConsole.MarkupLine($"[red] Failed to send message.[/] [bold]Status Code:[/] {response.StatusCode}");
                        }
                    }
                }
                catch (Exception e)
                {
                    AnsiConsole.WriteException(e);
                }
                finally
                {
                    RestartTimer();
                }
            }
        }
    }

    async Task ResponseListener()
    {
        while (!cts.IsCancellationRequested)
        {
            var context = await listener!.GetContextAsync();
            var request = context.Request;
            var response = context.Response;

            try
            {
                // Read the request body (if any)
                var requestBody = "{}";
                using (var reader = new StreamReader(request.InputStream, request.ContentEncoding))
                {
                    requestBody = await reader.ReadToEndAsync();
                }

                // Callbacks from the server push the head timer forward always, 
                // so given it some time to render responses.
                RestartTimer();
                await RenderAsync(requestBody);
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error processing request: {ex.Message}");
                context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;
                context.Response.Close();
            }
            finally
            {
                var buffer = Encoding.UTF8.GetBytes("OK");
                response.ContentLength64 = buffer.Length;
                response.ContentType = "text/plain";

                await response.OutputStream.WriteAsync(buffer);
                response.OutputStream.Close();
            }
        }
    }

    CancellationTokenSource typingCancellation = new();
    Task? typingStatus;

    void RestartTimer()
    {
        personTimer?.Start();         // no-op if already started
        personTimer?.Interval = 500; // moves event .5'' into the future if already started
    }

    async Task RenderAsync(string json)
    {
        if (needsNewline)
            AnsiConsole.WriteLine();

        // Try to parse the request body as a dictionary and render it as YAML
        if (format == OutputFormat.Yaml &&
            DictionaryConverter.Parse(json) is { } dictionary &&
            DictionaryConverter.ToYaml(dictionary) is { Length: > 0 } payload)
        {
            AnsiConsole.Write(new Panel(payload)
            {
                Border = BoxBorder.None,
                Padding = new Padding(0, 0, 0, 0),
                Width = Math.Min(100, AnsiConsole.Profile.Width)
            });
            return;
        }

        if (format == OutputFormat.Text)
        {
            try
            {
                // Move discriminator to top.
                json = await JQ.ExecuteAsync(json,
                    """
                    { "$type": (."type" // "typing") } + .
                    """);

                if (JsonSerializer.Deserialize(json, ClientContext.Default.ClientMessage) is { } message &&
                    message.ToString() is { } text)
                {
                    if (message.Type == Client.MessageType.Typing)
                    {
                        await ResetTypingAsync();
                        typingStatus = AnsiConsole.Status().StartAsync("...", async x =>
                        {
                            while (!cts.IsCancellationRequested && !typingCancellation.IsCancellationRequested)
                            {
                                await Task.Delay(100);
                                // We should never let the head appear while we're still "typing"
                                RestartTimer();
                            }
                        });
                        return;
                    }

                    // Don't render empty reaction since it's the clearing of the emoji actually in WhatsApp
                    if (message.Type == Client.MessageType.Reaction && text.Length == 0)
                        return;

                    if (message.Type != Client.MessageType.Reaction)
                        await ResetTypingAsync();

                    var parts = text.Split('|');
                    var emoji = ":robot:";
                    if (parts.Length > 1)
                    {
                        emoji = parts[0].Trim();
                        text = parts[1].Trim();
                    }

                    IRenderable body = message.Type == Client.MessageType.Reaction || (text.StartsWith("[") && text.EndsWith("]"))
                        ? TryMarkup(text)
                        : text.Contains("```")
                        ? TryCodeBlocks(text.Trim())
                        : TryCode(text.Trim(), false);

                    var grid = new Grid()
                        .AddColumn(new GridColumn().Width(2).Padding(0, 0))
                        .AddColumn(new GridColumn().Width(80).Padding(1, 0))
                        .AddRow(new Markup(emoji), body);

                    if (message is Client.InteractiveMessage interactive && interactive.Interactive.Action is { } node)
                        grid.AddRow(new Markup(" "), new Markup(DictionaryConverter.Parse(node.ToString()).ToYaml(true)));

                    AnsiConsole.Write(grid);
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
            Border = BoxBorder.None,
            Padding = new Padding(0, 0, 0, 0),
            Width = Math.Min(100, AnsiConsole.Profile.Width)
        });
    }

    static IRenderable TryCodeBlocks(string text)
    {
        var grid = new Grid();
        grid.AddColumn(new());

        // Regular expression to find code blocks with text before/after
        var regex = CodeBlockExpr();

        var lastEnd = 0;
        var matches = regex.Matches(text);

        if (matches.Count == 0)
            return TryMarkup(text);

        foreach (Match match in matches)
        {
            // Add text before code block if any
            var beforeText = match.Groups[1].Value;
            if (!string.IsNullOrWhiteSpace(beforeText))
                grid.AddRow(TryMarkup(beforeText.Trim()));

            var codeBlock = match.Groups[2].Value.Trim();
            grid.AddRow(TryCode(codeBlock));

            lastEnd = match.Index + match.Length;
        }

        // Add any remaining text after the last code block
        if (lastEnd < text.Length)
        {
            var remainingText = text[lastEnd..];
            if (!string.IsNullOrWhiteSpace(remainingText))
                grid.AddRow(TryMarkup(remainingText.Trim()));
        }

        return grid;
    }

    static IRenderable TryCode(string code, bool greyFallback = true)
    {
        try
        {
            JsonDocument.Parse(code, new JsonDocumentOptions
            {
                AllowTrailingCommas = true,
                CommentHandling = JsonCommentHandling.Skip
            });

            return new JsonText(code);
        }
        catch (JsonException)
        {
            // if it fails as JSON, try parsing as a YAML dictionary
            try
            {
                var yaml = DictionaryConverter.ToYaml(DictionaryConverter.FromYaml(code), formatted: true);
                return new Panel(yaml)
                {
                    Border = BoxBorder.None,
                    Padding = new Padding(0, 0, 0, 0),
                    Width = Math.Min(80, AnsiConsole.Profile.Width)
                };
            }
            catch (Exception)
            {
                if (greyFallback)
                {
                    // If it fails as YAML, fallback to grey text
                    return new Markup($"[grey]{code}[/]");
                }
                else
                {
                    return TryMarkup(code);
                }
            }
        }
    }

    static IRenderable TryMarkup(string text)
    {
        try
        {
            return new Markup(text);
        }
        catch (Exception)
        {
            return new Spectre.Console.Text(text).Overflow(Overflow.Fold);
        }
    }

    async Task ResetTypingAsync()
    {
        if (typingStatus != null && !typingStatus.IsCompleted)
        {
            typingCancellation.Cancel();
            await typingStatus;
            typingStatus = null;
            if (!typingCancellation.TryReset())
                typingCancellation = new CancellationTokenSource();
        }
    }

    [GeneratedRegex(@"(.*?)```([\s\S]*?)```", RegexOptions.Singleline)]
    private static partial Regex CodeBlockExpr();
}
