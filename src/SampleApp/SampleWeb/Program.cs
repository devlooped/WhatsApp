using System.Runtime.CompilerServices;
using System.Text.Json;
using System.Text.Json.Serialization;
using Azure.Messaging.EventGrid;
using Devlooped;
using Devlooped.WhatsApp;

var builder = WebApplication.CreateBuilder(args);

// Configure to use user secrets in development
if (builder.Environment.IsDevelopment())
{
    builder.Configuration.AddUserSecrets<Program>();
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

builder.AddServiceDefaults();

// Add WhatsApp services with a simple handler that echoes messages
var whatsapp = builder.Services
    .AddWhatsApp<EchoHandler>(configure: options =>
    {
        options.ReactOnMessage = "🌐";
        options.ReactOnProcess = "⚙️";
        options.ReactOnConversation = "💭";
    })
    .UseLogging()
    .UseConversation(conversationWindowSeconds: 300);

// In production, use EventGrid if configured
if (!builder.Environment.IsDevelopment())
{
    if (builder.Configuration["EventGrid:Topic"] is { Length: > 0 } topic &&
        builder.Configuration["EventGrid:Key"] is { Length: > 0 } key)
    {
        whatsapp.UseEventGridProcessor(new EventGridPublisherClient(
            new Uri(topic), new Azure.AzureKeyCredential(key)),
            options => builder.Configuration.Bind("EventGrid", options));
    }
}

var app = builder.Build();

// Map WhatsApp webhook endpoints
app.UseWhatsApp();

app.Run();

/// <summary>
/// Simple echo handler that replies to incoming messages with their content.
/// </summary>
class EchoHandler : IWhatsAppHandler
{
    public async IAsyncEnumerable<Response> HandleAsync(
        IEnumerable<IMessage> messages,
        [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        if (messages.OfType<ContentMessage>().LastOrDefault() is { } message)
        {
            yield return message.Reply($"Echo: {message.Content}");
        }

        await Task.CompletedTask;
    }
}
