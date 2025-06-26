using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Messaging.EventGrid;
using Azure.Storage.Queues;
using Devlooped;
using Devlooped.WhatsApp;
using Microsoft.Azure.Functions.Worker.Builder;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Response = Devlooped.WhatsApp.Response;

var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication();
builder.AddServiceDefaults();
builder.Configuration.AddUserSecrets<Program>();

#if CI || RELEASE
builder.Environment.EnvironmentName = "Production";
#else
builder.Environment.EnvironmentName = "Development";
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

var whatsapp = builder.Services
    .AddWhatsApp<ProcessHandler>(configure: options =>
    {
        options.ReactOnMessage = "🌐";
        options.ReactOnProcess = "⚙️";
        options.ReactOnConversation = "💭";
    })
    .UseIgnore()
    // Matches what we use in ConfigureOpenTelemetry
    .UseOpenTelemetry(builder.Environment.ApplicationName)
    .UseLogging()
    .Use(EchoAndHandle)
    .UseConversation(conversationWindowSeconds: 300 /* default */)
    .UseConsole();
// Uncomment next line to render a JSON of text message/responses 
//.UseConsoleRender();

// If event grid is set up, switch to processing messages using that
if (builder.Configuration["EventGrid:Topic"] is { Length: > 0 } topic &&
    builder.Configuration["EventGrid:Key"] is { Length: > 0 } key)
{
    whatsapp.UseEventGridProcessor(new EventGridPublisherClient(
        new Uri(topic), new Azure.AzureKeyCredential(key)));
}

var app = builder.Build();

#region DebugInit
#if DEBUG
async Task InitAsync(QueueClient queue)
{
    await queue.CreateIfNotExistsAsync();
    await queue.ClearMessagesAsync();
}
// Create and clear queues locally so we don't get constant warnings in the logs
var queues = app.Services.GetRequiredService<QueueServiceClient>();
await InitAsync(queues.GetQueueClient("whatsappwebhook"));
await InitAsync(queues.GetQueueClient("whatsappmessages"));
await InitAsync(queues.GetQueueClient("whatsappmemory"));
#endif
#endregion

app.Run();

static async IAsyncEnumerable<Response> EchoAndHandle(IEnumerable<IMessage> messages, IWhatsAppHandler inner, [EnumeratorCancellation] CancellationToken cancellation)
{
    var content = messages.OfType<ContentMessage>().LastOrDefault();
    if (content != null)
        yield return content.Reply("Echo: " + content.Content.ToString());

    await foreach (var response in inner.HandleAsync(messages, cancellation))
        yield return response;
}