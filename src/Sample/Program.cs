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

builder.UseWhatsApp<IWhatsAppClient, ILogger<Program>, JsonSerializerOptions>(async (client, logger, options, message, cancellation) =>
{
    logger.LogInformation("💬 Received message: {Message}", message);

    if (message is ErrorMessage error)
    {
        // Reengagement error, we need to invite the user.
        if (error.Error.Code == 131047)
        {
            await client.SendAsync(error.To.Id, new
            {
                messaging_product = "whatsapp",
                to = error.From.Number,
                type = "template",
                template = new
                {
                    name = "reengagement",
                    language = new
                    {
                        code = "es_AR"
                    }
                }
            });
        }
        else
        {
            logger.LogWarning("⚠️ Unknown error message received: {Error}", message);
        }
        return;
    }
    else if (message is InteractiveMessage interactive)
    {
        logger.LogWarning("👤 chose {Button} ({Title})", interactive.Button.Id, interactive.Button.Title);
        await client.ReplyAsync(interactive, $"👤 chose: {interactive.Button.Title} ({interactive.Button.Id})");
        return;
    }
    else if (message is ReactionMessage reaction)
    {
        logger.LogInformation("👤 reaction: {Reaction}", reaction.Emoji);
        await client.ReplyAsync(reaction, $"👤 reaction: {reaction.Emoji}");
        return;
    }
    else if (message is StatusMessage status)
    {
        logger.LogInformation("☑️ status: {Status}", status.Status);
        return;
    }
    else if (message is ContentMessage content)
    {
        await client.ReactAsync(content, "🧠");
        // simulate some hard work at hand, like doing some LLM-stuff :)
        //await Task.Delay(2000);
        await client.ReplyAsync(content, $"☑️ Got your {content.Content.Type}:\r\n{JsonSerializer.Serialize(content, options)}",
            new Button("btn_good", "👍"),
            new Button("btn_bad", "👎"));
    }
    else if (message is UnsupportedMessage unsupported)
    {
        logger.LogWarning("⚠️ {Message}", unsupported);
        return;
    }
});

builder.Services.AddMemoryCache();
builder.Services.AddDistributedAzureTableStorageCache(options =>
{
    options.PartitionKey = "SampleCache";
    options.TableName = "SampleCache";
    options.CreateTableIfNotExists = true;
    options.ConnectionString = builder.Configuration["AzureWebJobsStorage"];
});
builder.Services.AddHybridCache(options =>
{
    options.DefaultEntryOptions = new() { Expiration = TimeSpan.FromDays(180) };
});

builder.Build().Run();
