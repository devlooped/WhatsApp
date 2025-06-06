using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Devlooped;
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

builder.Services.AddSingleton(services => builder.Environment.IsDevelopment() ?
    CloudStorageAccount.DevelopmentStorageAccount :
    CloudStorageAccount.TryParse(builder.Configuration["App:Storage"] ?? "", out var storage) ?
    storage :
    throw new InvalidOperationException("Missing required App:Storage connection string."));

builder.Services
    .AddWhatsApp<ILogger<Program>, JsonSerializerOptions>(ProcessMessagesAsync)
    // Matches what we use in ConfigureOpenTelemetry
    .UseOpenTelemetry(builder.Environment.ApplicationName)
    .UseLogging()
    .UseStorage()
    .UseConversation();

builder.Build().Run();

static async IAsyncEnumerable<Response> ProcessMessagesAsync(
    ILogger<Program> logger,
    JsonSerializerOptions options,
    IEnumerable<IMessage> messages,
    [EnumeratorCancellation] CancellationToken cancellationToken)
{
    // Avoid warning CS1998 // Async method lacks 'await' operators and will run synchronously
    await Task.CompletedTask;

    var message = messages.Last();
    logger.LogInformation("💬 Received message: {Message}", message);

    if (message is ErrorMessage error)
    {
        // Reengagement error, we need to invite the user.
        if (error.Error.Code == 131047)
        {
            // Showcases how to use a pre-declared template response to reengage the user.
            yield return error.Template("reengagement", "es_AR");
        }
        else
        {
            logger.LogWarning("⚠️ Unknown error message received: {Error}", message);
        }
    }
    else if (message is InteractiveMessage interactive)
    {
        logger.LogWarning("👤 chose {Button} ({Title})", interactive.Button.Id, interactive.Button.Title);
        yield return interactive.Reply($"👤 chose: {interactive.Button.Title} ({interactive.Button.Id})");
    }
    else if (message is ReactionMessage reaction)
    {
        logger.LogInformation("👤 reaction: {Reaction}", reaction.Emoji);
        yield return reaction.Reply($"👤 reaction: {reaction.Emoji}");
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
        yield return content.Reply(
            $"☑️ Got your {content.Content.Type}:\r\n{JsonSerializer.Serialize(content, options)}",
            new Button("btn_good", "👍"),
            new Button("btn_bad", "👎"));
    }
    else if (message is UnsupportedMessage unsupported)
    {
        logger.LogWarning("⚠️ {Message}", unsupported);
    }
}
