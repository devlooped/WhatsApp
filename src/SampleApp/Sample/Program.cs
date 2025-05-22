using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Devlooped.WhatsApp;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication();
builder.AddServiceDefaults();

#if DEBUG
builder.Environment.EnvironmentName = "Development";
builder.Configuration.AddUserSecrets<Program>();
#endif

if (builder.Environment.IsDevelopment())
{
    // TODO: doesn't seem to work.
    builder.Logging.AddFilter("Devlooped.WhatsApp.AzureFunctions", LogLevel.Trace);
}

builder.Services.AddSingleton(new JsonSerializerOptions(JsonSerializerDefaults.General)
{
    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    Converters =
    {
        new JsonStringEnumConverter()
    },
    DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
    WriteIndented = true
});

builder.Services
    .AddWhatsApp<ILogger<Program>, JsonSerializerOptions>(ProcessMessagesAsync)
    // Matches what we use in ConfigureOpenTelemetry
    .UseOpenTelemetry(builder.Environment.ApplicationName)
    .UseLogging();

builder.Build().Run();

#pragma warning disable CS1998 // Async method lacks 'await' operators and will run synchronously
static async IAsyncEnumerable<Response> ProcessMessagesAsync(
    ILogger<Program> logger,
    JsonSerializerOptions options,
    IEnumerable<Message> messages,
    [EnumeratorCancellation] CancellationToken cancellationToken)
{
    var message = messages.Last();
    logger.LogInformation("💬 Received message: {Message}", message);

    if (message is ErrorMessage error)
    {
        // Reengagement error, we need to invite the user.
        if (error.Error.Code == 131047)
        {
            yield return error.Reengage();
        }
        else
        {
            logger.LogWarning("⚠️ Unknown error message received: {Error}", message);
        }
    }
    else if (message is InteractiveMessage interactive)
    {
        logger.LogWarning("👤 chose {Button} ({Title})", interactive.Button.Id, interactive.Button.Title);
        yield return interactive.Text($"👤 chose: {interactive.Button.Title} ({interactive.Button.Id})");
    }
    else if (message is ReactionMessage reaction)
    {
        logger.LogInformation("👤 reaction: {Reaction}", reaction.Emoji);
        yield return reaction.Text($"👤 reaction: {reaction.Emoji}");
    }
    else if (message is StatusMessage status)
    {
        logger.LogInformation("☑️ status: {Status}", status.Status);
    }
    else if (message is ContentMessage content)
    {
        yield return content.React("🧠");

        // simulate some hard work at hand, like doing some LLM-stuff :)
        //await Task.Delay(2000);
        yield return content.TextWithButtons(
            $"☑️ Got your {content.Content.Type}:\r\n{JsonSerializer.Serialize(content, options)}",
            new Button("btn_good", "👍"),
            new Button("btn_bad", "👎"));
    }
    else if (message is UnsupportedMessage unsupported)
    {
        logger.LogWarning("⚠️ {Message}", unsupported);
    }
}
#pragma warning restore CS1998 // Async method lacks 'await' operators and will run synchronously