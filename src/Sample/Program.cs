using System.Text.Json;
using System.Text.Json.Serialization;
using Devlooped.WhatsApp;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);
var options = new JsonSerializerOptions(JsonSerializerDefaults.General)
{
    Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
    Converters =
    {
        new JsonStringEnumConverter()
    },
    WriteIndented = true
};

builder.ConfigureFunctionsWebApplication();
builder.Configuration.AddUserSecrets<Program>();

builder.UseWhatsApp<IWhatsAppClient, ILogger<Program>>(async (client, logger, message) =>
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
        logger.LogWarning("👤 User chose button {Button} ({Title})", interactive.Button.Id, interactive.Button.Title);
        return;
    }
    else if (message is StatusMessage status)
    {
        logger.LogInformation("☑️ New message status: {Status}", status.Status);
        return;
    }
    else if (message is ContentMessage content)
    {
        await client.ReactAsync(message, "🧠");
        // simulate some hard work at hand, like doing some LLM-stuff :)
        //await Task.Delay(2000);
        await client.ReplyAsync(message, $"☑️ Got your {content.Content.Type}:\r\n{JsonSerializer.Serialize(content, options)}");
    }
});

builder.Build().Run();
