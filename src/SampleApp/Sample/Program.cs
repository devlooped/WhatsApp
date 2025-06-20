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
    CloudStorageAccount.Parse(builder.Configuration["AzureWebJobsStorage"]));

builder.Services
    .AddWhatsApp<ProcessHandler>()
    // Matches what we use in ConfigureOpenTelemetry
    .UseOpenTelemetry(builder.Environment.ApplicationName)
    .UseLogging()
    .Use(EchoAndHandle)
    .UseConversation(conversationWindowSeconds: 300 /* default */);

builder.Build().Run();

static async IAsyncEnumerable<Response> EchoAndHandle(IEnumerable<IMessage> messages, IWhatsAppHandler inner, [EnumeratorCancellation] CancellationToken cancellation)
{
    var content = messages.OfType<ContentMessage>().LastOrDefault();
    if (content != null)
        yield return content.Reply("Echo: " + content.Content.ToString());

    await foreach (var response in inner.HandleAsync(messages, cancellation))
        yield return response;
}