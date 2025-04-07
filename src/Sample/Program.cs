using Devlooped.WhatsApp;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

var builder = FunctionsApplication.CreateBuilder(args);

builder.ConfigureFunctionsWebApplication();
builder.Configuration.AddUserSecrets<Program>();

builder.UseWhatsApp<IWhatsAppClient, ILogger<Program>>(async (client, logger, message) =>
{
    logger.LogInformation("Received message: {Message}", message);
    await client.ReactAsync(message.To.Id, message.From.Number, message.Id, "🧠");
    // simulate some hard work at hand, like doing some LLM-stuff :)
    await Task.Delay(2000);
    await client.SendTextAync(message.To.Id, message.From.Number, "I'm alive, but I'm just a sample 🤷‍.");
});

builder.Build().Run();
