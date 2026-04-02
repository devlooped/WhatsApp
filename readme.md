![Icon](assets/img/icon.png) WhatsApp agents for .NET
============

[![Version](https://img.shields.io/nuget/vpre/Devlooped.WhatsApp.svg?color=royalblue)](https://www.nuget.org/packages/Devlooped.WhatsApp)
[![Downloads](https://img.shields.io/nuget/dt/Devlooped.WhatsApp.svg?color=darkmagenta)](https://www.nuget.org/packages/Devlooped.WhatsApp)
[![EULA](https://img.shields.io/badge/EULA-OSMF-blue?labelColor=black&color=C9FF30)](https://github.com/devlooped/Devlooped.WhatsApp/blob/main/osmfeula.txt)
[![License](https://img.shields.io/github/license/devlooped/WhatsApp.svg?color=blue)](https://github.com//devlooped/Devlooped.WhatsApp/blob/main/license.txt)


<!-- #description -->
Create agents for WhatsApp using .NET with support for Azure Functions and ASP.NET Core.
<!-- #description -->

## Open Source Maintenance Fee

To ensure the long-term sustainability of this project, use of SmallSharp requires an 
[Open Source Maintenance Fee](https://opensourcemaintenancefee.org). While the source 
code is freely available under the terms of the [MIT License](./license.txt), all other aspects of the 
project --including opening or commenting on issues, participating in discussions and 
downloading releases-- require [adherence to the Maintenance Fee](./osmfeula.txt).

In short, if you use this project to generate revenue, the [Maintenance Fee is required](./osmfeula.txt).

To pay the Maintenance Fee, [become a Sponsor](https://github.com/sponsors/devlooped).

## Usage

### Azure Functions
<!-- #usage-functions -->
```csharp
var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication();

// add your messages handler here 👇 
builder.AddWhatsApp<MyWhatsAppHandler>();

builder.Build().Run();
```
<!-- #usage-functions -->

### ASP.NET Core
<!-- #usage-aspnet -->
```csharp
var builder = WebApplication.CreateBuilder(args);

// add your messages handler here 👇 
builder.Services.AddWhatsApp<MyWhatsAppHandler>();

var app = builder.Build();

app.UseWhatsApp(); // 👈 map webhook endpoints

app.Run();
```
<!-- #usage-aspnet -->
<!-- #content -->
Both integrations (Azure Functions and ASP.NET Core) map the following endpoints:
- `POST /whatsapp` - Main webhook for receiving messages
- `GET /whatsapp` - Webhook verification endpoint
- `POST /whatsapp/process` - Direct message processing
- `POST /whatsapp/eventgrid` - Event Grid processing
- `POST/GET /whatsapp/cli` - Development console

Instead of providing an `IWhatsAppHandler` implementation, you can also 
register an inline handler using minimal API style:

```csharp
builder.Services.AddWhatsApp((messages, cancellation) =>
{
    foreach (var message in messages)
    {
        // MessageType: Content | Error | Interactive | Reaction | Status
        Console.WriteLine($"Got message type {message.Type}");
        switch (message)
        {
            case ContentMessage content:
                // ContentType = Text | Contact | Document | Image | Audio | Video | Location | Unknown (raw JSON)
                Console.WriteLine($"Got content type {content.Content.Type}");
                break;
            case ErrorMessage error:
                Console.WriteLine($"Error: {error.Error.Message} ({error.Error.Code})");
                break;
            case InteractiveMessage interactive:
                Console.WriteLine($"Interactive: {interactive.Selection.Text} ({interactive.Selection.Value})");
                break;
            case StatusMessage status:
                Console.WriteLine($"Status: {status.Status}");
                break;
        }
    }

    return AsyncEnumerable.Empty<Response>();
});
```

If the handler needs additional services, they can be provided directly 
as generic parameters of the `UseWhatsApp` method, such as:

```csharp
builder.Services.AddWhatsApp<ILogger<Program>>((logger, message, cancellation) =>
{
    logger.LogInformation($"Got messages!");

    return messages.OfType<ContentMessage>()
        .Select(content => content.Reply($"☑️ Got your {content.Content.Type}"))
        .ToAsyncEnumerable();
}
```

You can also specify the parameter types in the delegate itself and avoid the 
generic parameters:

```csharp
builder.Services.AddWhatsApp(async (ILogger<Program> logger, IEnumerable<Message> messages, CancellationToken cancellation) =>
```

Handlers generate responses by returning an `IAsyncEnumerable<Response>`, and the 
responses are typically created by calling extension methods on the incoming messages, 
such as `Reply` or `React`:

```csharp
if (message is ContentMessage content)
{
    yield return message.React(message, "🧠");
    // simulate some hard work at hand, like doing some LLM-stuff :)
    await Task.Delay(2000);
    var json = JsonSerializer.Serialize(content, options);
    yield return message.Reply($"☑️ Got your {content.Content.Type}:\r\n{json}");
}
```

This allows the handler to remain decoupled from the actual sending of messages, making it 
easier to unit test. 

There's no limitation on the number of responses you can yield during processing, but 
sending an intermediate reply will cause the typing indicator sent by default by the 
webhook to dissapear. To signal ongoing processing to the user, you can send the typing 
status response as follows:

```csharp
yield return content.Reply("Spinning my digital neurons...");
yield return content.Typing();

// simulate some hard work at hand, like doing some LLM-stuff :)
await Task.Delay(2000);

yield return content.Reply("That was tough, but here's your reponse: ... ");
}
```

In this case, the typing status will be restored right after the initial reply.

This is how the initial typing status looks like when the webhook gets the message: 

![](https://raw.githubusercontent.com/devlooped/WhatsApp/main/assets/img/progress1.png)

And after we send the "Spinning..." message, the restored typing status would 
look like the following:

![](https://raw.githubusercontent.com/devlooped/WhatsApp/main/assets/img/progress2.png)


If sending messages outside the handler pipeline is needed, you can use the provided 
`IWhatsAppClient`, which is a very thin abstraction allowing you to send arbitrary payloads 
to WhatsApp for Business:

```csharp
public interface IWhatsAppClient
{
    /// Payloads from https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages
    Task SendAync(string from, object payload);
}
```

Extensions methods for this interface take care of simplifying usage for some 
common scenarios, such as reacting to a message and replying with plain text:

```csharp
if (message is ContentMessage content)
{
    await client.ReactAsync(message, "🧠");
    // simulate some hard work at hand, like doing some LLM-stuff :)
    await Task.Delay(2000);
    var json = JsonSerializer.Serialize(content, options);
    await client.ReplyAsync(message, $"☑️ Got your {content.Content.Type}:\r\n{json}");
}
```

Regardless of the approach used (handler-generated reponse async enumerable or direct 
client calls), the above examples would render as follows in WhatsApp:

![](https://raw.githubusercontent.com/devlooped/WhatsApp/main/assets/img/whatsapp.png)


## Conversations

WhatsApp does not provide a way to keep track of conversations, at most providing the 
related message ID of a message that was replied to. In many agents, however, keeping 
track of conversations is crucial for maintaining context and continuity. 

This library provides a simple built-in functionality for this based on some simple 
heuristics: 

- If a message is sent in response to another message, it is considered part of the same conversation.
- Messages sent within a short time frame (default: 5 minutes) are considered part of the same conversation.
- Individual messages, conversations and the active conversations are stored in an Azure 
  storage account

Usage:

```csharp
builder.Services
    .AddWhatsApp<MyWhatsAppHandler>()
    .UseConversation(conversationWindowSeconds: 300 /* default */);
```

The conversation window can also be configured via `WhatsAppOptions` in the configuration, 
such as setting `WhatsApp:ConversationWindowSeconds = 600`.

Unless you provide a [CloudStorageAccount](https://www.nuget.org/packages/Devlooped.CloudStorageAccount) in 
the service collection, the library will use the `AzureWebJobsStorage` connection string automatically 
for this, so things will just work out of the box. 

An example of providing storage to a different account than the functions runtime one:

```csharp
builder.Services.AddSingleton(services => builder.Environment.IsDevelopment() ?
    // Always local emulator in development
    CloudStorageAccount.DevelopmentStorageAccount :
    // First try with custom connection string
    CloudStorageAccount.TryParse(builder.Configuration["App:Storage"] ?? "", out var storage) ?
    storage :
    // Fallback to built-in functions storage (default behavior).
    CloudStorageAccount.Parse(builder.Configuration["AzureWebJobsStorage"]));
```

## Configuration

You need to register an app in the Meta [App Dashboard](https://developers.facebook.com/apps/). 
The app must then be configured to use the WhatsApp Business API, and the webhook and 
verification token (an arbitrary value) must be set up in the app settings under WhatsApp. 
The webhook URL is `/whatsapp` under your Azure Functions app.

Make sure you subscribe the webhook to the `messages` field, with API version `v22.0` or later.

Configuration on the Azure Functions side is done via the 
[ASP.NET options pattern](https://learn.microsoft.com/en-us/aspnet/core/fundamentals/configuration/options) 
and the `MetaOptions` type. When you call `UseWhatsApp`, the options will be bound by 
default to the `Meta` section in the configuration. You can also configure it programmatically 
as follows:

```csharp
builder.Services.Configure<MetaOptions>(options =>
{
    options.VerifyToken = "my-webhook-1234";
    options.Numbers["12345678"] = "asff=";
});
```

Via configuration:

```json
{
  "Meta": {
    "VerifyToken": "my-webhook-1234",
    "Numbers": {
      "12345678": "asff="
    }
  }
}
```

The `Numbers` dictionary is a map of WhatsApp phone identifiers and the 
corresponding access token for it. To get a permanent access token for 
use, you'd need to create a [system user](https://business.facebook.com/latest/settings/system_users) 
with full control permissions to the WhatsApp Business API (app).

You can also configure how the WhatsApp webhook and processing pipeline behaves by passing 
in an additional delegate to the `AddWhatsApp` method via the `configure` parameter:

```csharp
builder.Services
    .AddWhatsApp<ProcessHandler>(configure: options =>
    {
        options.ReactOnMessage = "🌐";
        options.ReactOnProcess = "⚙️";
        options.ReactOnConversation = "💭";
    })
```

The `WhatsAppOptions` passed in can also be set in configuration, which will be read 
automatically when the `AddWhatsApp` method is called, so the following configuration 
is equivalent to the above:

```json
{
    "WhatsApp": {
        "ReactOnMessage": "🌐",
        "ReactOnProcess": "⚙️",
        "ReactOnConversation": "💭"
    }
}
```

By default, the library will mark messages read on webhook invocation by WhatsApp, 
and send the typing status to the user:

![](https://raw.githubusercontent.com/devlooped/WhatsApp/main/assets/img/typing.png)

You can modify this behavior through the `WhatsAppOptions` as well, with the 
`TypingOnMessage` and `TypingOnProcess` properties. Sending the typing status implies 
marking the message as read too.

## Functionality pipelines

`IWhatsAppHandler` instances can be layered to form a pipeline of components, each 
contributing unique capabilities. These components may originate from `Devlooped.WhatsApp`, 
external NuGet libraries, or custom implementations. This mechanism enables flexible 
enhancement of the WhatsApp handler's functionality to suit specific requirements. 
Below is an example that wraps a WhatsApp handler with logging, OpenTelemetry tracing, 
message storage and conversation management:

```csharp
var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication();

builder.Services.AddWhatsApp<MyWhatsAppHandler>()
    .UseOpenTelemetry(builder.Environment.ApplicationName)
    .UseLogging()
    .UseStorage()
    .UseConversation();

builder.Build().Run();
```

Creating additional cross-cutting behaviors to keep handlers clean and focused on 
a single responsibility is straightforward. For example, let's say you don't want 
to perform any processing for status messages (these are VERY noisy, sent by whatsApp 
foreach thing that happens to a message, such as when it is sent, delivered, read, etc.).
You could easily create a custom component that filters out these messages:

```csharp
static class IgnoreMessagesExtensions
{
    public static WhatsAppHandlerBuilder UseIgnore(this WhatsAppHandlerBuilder builder)
        => builder.Use((inner, services) => new IgnoreMessagesHandler(inner,
            message => message.Type != MessageType.Status && message.Type != MessageType.Unsupported));

    public static WhatsAppHandlerBuilder UseIgnore(this WhatsAppHandlerBuilder builder, Func<IMessage, bool> filter)
        => builder.Use((inner, services) => new IgnoreMessagesHandler(inner, filter));

    class IgnoreMessagesHandler(IWhatsAppHandler inner, Func<IMessage, bool> filter) : DelegatingWhatsAppHandler(inner)
    {
        public override IAsyncEnumerable<Response> HandleAsync(IEnumerable<IMessage> messages, CancellationToken cancellation = default)
        {
            var filtered = messages.Where(filter).ToArray();
            // Skip inner handler altogether if no messages pass the filter.
            if (filtered.Length == 0)
                return AsyncEnumerable.Empty<Response>();

            return base.HandleAsync(filtered, cancellation);
        }
    }
}
```

This new extension method can now be used in the pipeline without changing any of the 
existing handlers:

```csharp
builder.Services.AddWhatsApp<MyWhatsAppHandler>()
    .UseOpenTelemetry(builder.Environment.ApplicationName)
    .UseLogging()
    .UseIgnore()  // 👈 Ignore status+unsupported messages. We do log them.
    .UseConversation();
```

Finally, the pipeline handlers are encouraged to use the `Response` model 
for sending messages, instead of invoking the `IWhatsAppClient` directly. 
This allows the pipeline to automatically manage the sending and integrate 
better with persistence or other cross-cutting concerns, while allowing for 
flexible in-progress message generation (i.e. notify user that processing 
is ongoing via reactions, etc.). 

For example, to send the typing indicator when starting processing messages 
in a handler:

```csharp
public override async IAsyncEnumerable<Response> HandleAsync(IEnumerable<IMessage> messages, [EnumeratorCancellation] CancellationToken cancellation = default)
{
    foreach (var message in messages)
    {
        if (message is UserMessage user)
            yield return user.Typing();
        
        // Do some processing, send final response

        yield return message.Reply("Processing complete!");
    }
}
```

The response model supports all the common WhatsApp message types, including 
plain text responses, interactive buttons, reactions and templates, such as:

```csharp
    yield return message.Template(new MessageTemplate("order", "en")
    {
        Buttons =
        [
            // i.e. template button get tracking info for an order
            ButtonComponent.Payload(orderId),
            // i.e. a template url button to navigate to the order page
            ButtonComponent.Url(orderId),
            // i.e. a template catalog button to view the WhatsApp business catalog
            ButtonComponent.Catalog()
        ]
    });
```

### OpenTelemetry

The configurable built-in support for OpenTelemetry shown above allows tracking 
of key metrics such as message processing time and the number of messages processed.

This is a rendering of the telemetry data in Aspire in the sample app provided in 
this repository:

![](https://raw.githubusercontent.com/devlooped/WhatsApp/main/assets/img/aspire.png)

The spans/activites created by the `OpenTelemetryHandler` follow the [OpenTelemetry Semantic Conventions for Messaging Spans](https://opentelemetry.io/docs/specs/semconv/messaging/messaging-spans/). 
Specifically, it uses a [consumer span](https://opentelemetry.io/docs/specs/semconv/messaging/messaging-spans/#consumer-spans) 
named "process whatsapp" to track the processing of incoming WhatsApp messages.

| Attribute/Tag | Value | Description | OTEL Convention |
|-----------|-------|-------------|-----------------|
| `messaging.system` | `whatsapp` | Identifies the messaging system being used. | [messaging.system](https://opentelemetry.io/docs/specs/semconv/registry/attributes/messaging/) |
| `messaging.operation.name` | `process` | The name of the operation performed on the message, indicating processing of incoming messages. | [messaging.operation.name](https://opentelemetry.io/docs/specs/semconv/registry/attributes/messaging/) |
| `messaging.destination.name` | Service ID (e.g., WhatsApp Business Account phone number) | The name of the destination to which the message is sent. In this context, it's the service identifier for the WhatsApp endpoint. | [messaging.destination.name](https://opentelemetry.io/docs/specs/semconv/registry/attributes/messaging/) |
| `messaging.client.id` | User phone number | The identifier of the client that sent the message. | [messaging.client.id](https://opentelemetry.io/docs/specs/semconv/registry/attributes/messaging/) |
| `messaging.message.id` | Message ID | The unique identifier of the message being processed. | [messaging.message.id](https://opentelemetry.io/docs/specs/semconv/registry/attributes/messaging/) |
| `messaging.message.conversation_id` | Conversation ID (if available) | The identifier of the conversation the message belongs to, if applicable. | [messaging.message.conversation_id](https://opentelemetry.io/docs/specs/semconv/registry/attributes/messaging/) |

These attributes provide detailed context for tracing message processing flows in distributed systems.

In addition to spans, the handler emits metrics:
- `messaging.process.duration` (histogram): Duration of WhatsApp message processing in seconds. See [OTEL convention](https://opentelemetry.io/docs/specs/semconv/messaging/messaging-metrics/#metric-messagingprocessduration)
- `messaging.client.consumed.messages` (counter): Number of WhatsApp messages processed. See [OTEL convention](https://opentelemetry.io/docs/specs/semconv/messaging/messaging-metrics/#metric-messagingclientconsumedmessages)

These metrics also carry the same tags as the spans for correlation.

<!-- #content -->

## Scalability and Performance

In order to quickly and efficiently process incoming messages, the library uses 
Azure Storage Queues to queue incoming messages from WhatsApp, which provides 
a reliable and scalable way to handle incoming messages. It also uses Azure Table Storage 
to detect duplicate messages and avoid processing the same message multiple times.

If `QueueServiceClient` and `TableServiceClient` are registered in the DI container 
before invoking `UseWhatsApp`, the library will automatically use them. Otherwise, 
it will register both services using the `AzureWebJobsStorage` connection string, 
therefore sharing storage with the Azure Functions runtime.

As an alternative (and more instantaneous in practice) approach to asynchronous message 
processing, Azure Event Grid is also a supported option. It requires setting up an 
Azure Event Grid topic, and subscribing it to the `whatsapp_eventgrid` function 
and then configure the topic URL and access key for use with the pipeline, such as:


```csharp
var whatsapp = builder.Services.AddWhatsApp<MyWhatsAppHandler>()
    ...;

// If event grid is set up, switch to processing messages using that
if (builder.Configuration["EventGrid:Topic"] is { Length: > 0 } topic &&
    builder.Configuration["EventGrid:Key"] is { Length: > 0 } key)
{
    whatsapp.UseEventGridProcessor(new EventGridPublisherClient(
        new Uri(topic), new Azure.AzureKeyCredential(key)));
}
```

You can also create your own enqueue message processing implementation 
by creating your own `IMessageProcessor` interface 
and registering it in the container: 

```csharp
public interface IMessageProcessor
{
    /// <summary>
    /// Enqueues the WhatsApp for Business webhook message for async processing.
    /// </summary>
    Task EnqueueAsync(string json, CancellationToken cancellation = default);
}
```

No additional configuration is needed in this case since the 
Azure Functions just take `IMessageProcessor` as a constructor 
dependency and will automatically pick up your custom implementation.

<!-- #content -->

## WhatsApp CLI

[![Version](https://img.shields.io/nuget/vpre/dotnet-whatsapp.svg?color=royalblue)](https://www.nuget.org/packages/dotnet-whatsapp)
[![Downloads](https://img.shields.io/nuget/dt/dotnet-whatsapp.svg?color=green)](https://www.nuget.org/packages/dotnet-whatsapp)

<!-- #cli -->

Provides a command-line interface for the [WhatsApp](https://nuget.org/packages/Devlooped.WhatsApp) 
library and its backend functions. This allows you to interact with your WhatsApp pipeline without 
having to set up your WhatsApp for Business app for local development. 

The backend functions are only enabled if the hosting environment is set to `Development` so that 
in production, the CLI endpoint is not available. Example with text format:

![](https://raw.githubusercontent.com/devlooped/WhatsApp/main/assets/img/cli-text.png)

Yaml format:

![](https://raw.githubusercontent.com/devlooped/WhatsApp/main/assets/img/cli-yaml.png)

JSON format:

![](https://raw.githubusercontent.com/devlooped/WhatsApp/main/assets/img/cli-json.png)

The console will automatically remember the last used WhatsApp endpoint, output format and simulated 
user phone number.

```bash
Usage: whatsapp [OPTIONS]+
Options:
  -u, --url                  WhatsApp functions endpoint
  -n, --number=VALUE         Your WhatsApp user phone number
  -j, --json                 Format output as JSON
  -t, --text                 Format output as text
  -y, --yaml                 Format output as YAML
  -?, -h, --help             Display this help.
  -v, --version              Render tool version and updates.
```

to render the responses since it provides a more readable format than JSON.

For non-text messages, the CLI falls short since you cannot attach files or images. For these 
cases, you can continue to send messages via WhatsApp, but get the responses also in the CLI. 
This works by inspecting messages in the current conversation (so it depends on `UseConversation`) 
and detecting if any messages were sent by the CLI. If that is the case, non-console messages 
will generate responses for the CLI as well:

```csharp
builder.Services.AddWhatsApp<MyWhatsAppHandler>()
    .UseOpenTelemetry(builder.Environment.ApplicationName)
    .UseConversation()
    .UseConsole() // 👈 Enable CLI support for WhatsApp-originated messages

```

> [!IMPORTANT]
> `UseConsole` will only be added to the pipeline if the hosting environment is set to `Development`, 
> so it's not necessary to check for that in your code. This is to ensure that the CLI behaviors 
> never impact production environments.

<!-- #cli -->

## Dogfooding

[![CI Version](https://img.shields.io/endpoint?url=https://shields.kzu.app/vpre/Devlooped.WhatsApp/main&label=nuget.ci&color=brightgreen)](https://pkg.kzu.app/index.json)
[![Build](https://github.com/devlooped/WhatsApp/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/devlooped/WhatsApp/actions/workflows/build.yml)

We also produce CI packages from branches and pull requests so you can dogfood builds as quickly as they are produced. 

The CI feed is `https://pkg.kzu.app/index.json`. 

The versioning scheme for packages is:

- PR builds: *42.42.42-pr*`[NUMBER]`
- Branch builds: *42.42.42-*`[BRANCH]`.`[COMMITS]`

To install or update the CLI from the main branch:

```bash
dotnet tool update -g dotnet-whatsapp --add-source https://pkg.kzu.app/index.json --prerelease
```

<!-- include https://github.com/devlooped/sponsors/raw/main/footer.md -->
# Sponsors 

<!-- sponsors.md -->
[![Clarius Org](https://avatars.githubusercontent.com/u/71888636?v=4&s=39 "Clarius Org")](https://github.com/clarius)
[![MFB Technologies, Inc.](https://avatars.githubusercontent.com/u/87181630?v=4&s=39 "MFB Technologies, Inc.")](https://github.com/MFB-Technologies-Inc)
[![SandRock](https://avatars.githubusercontent.com/u/321868?u=99e50a714276c43ae820632f1da88cb71632ec97&v=4&s=39 "SandRock")](https://github.com/sandrock)
[![DRIVE.NET, Inc.](https://avatars.githubusercontent.com/u/15047123?v=4&s=39 "DRIVE.NET, Inc.")](https://github.com/drivenet)
[![Keith Pickford](https://avatars.githubusercontent.com/u/16598898?u=64416b80caf7092a885f60bb31612270bffc9598&v=4&s=39 "Keith Pickford")](https://github.com/Keflon)
[![Thomas Bolon](https://avatars.githubusercontent.com/u/127185?u=7f50babfc888675e37feb80851a4e9708f573386&v=4&s=39 "Thomas Bolon")](https://github.com/tbolon)
[![Kori Francis](https://avatars.githubusercontent.com/u/67574?u=3991fb983e1c399edf39aebc00a9f9cd425703bd&v=4&s=39 "Kori Francis")](https://github.com/kfrancis)
[![Uno Platform](https://avatars.githubusercontent.com/u/52228309?v=4&s=39 "Uno Platform")](https://github.com/unoplatform)
[![Reuben Swartz](https://avatars.githubusercontent.com/u/724704?u=2076fe336f9f6ad678009f1595cbea434b0c5a41&v=4&s=39 "Reuben Swartz")](https://github.com/rbnswartz)
[![Jacob Foshee](https://avatars.githubusercontent.com/u/480334?v=4&s=39 "Jacob Foshee")](https://github.com/jfoshee)
[![](https://avatars.githubusercontent.com/u/33566379?u=bf62e2b46435a267fa246a64537870fd2449410f&v=4&s=39 "")](https://github.com/Mrxx99)
[![Eric Johnson](https://avatars.githubusercontent.com/u/26369281?u=41b560c2bc493149b32d384b960e0948c78767ab&v=4&s=39 "Eric Johnson")](https://github.com/eajhnsn1)
[![Jonathan ](https://avatars.githubusercontent.com/u/5510103?u=98dcfbef3f32de629d30f1f418a095bf09e14891&v=4&s=39 "Jonathan ")](https://github.com/Jonathan-Hickey)
[![Ken Bonny](https://avatars.githubusercontent.com/u/6417376?u=569af445b6f387917029ffb5129e9cf9f6f68421&v=4&s=39 "Ken Bonny")](https://github.com/KenBonny)
[![Simon Cropp](https://avatars.githubusercontent.com/u/122666?v=4&s=39 "Simon Cropp")](https://github.com/SimonCropp)
[![agileworks-eu](https://avatars.githubusercontent.com/u/5989304?v=4&s=39 "agileworks-eu")](https://github.com/agileworks-eu)
[![Zheyu Shen](https://avatars.githubusercontent.com/u/4067473?v=4&s=39 "Zheyu Shen")](https://github.com/arsdragonfly)
[![Vezel](https://avatars.githubusercontent.com/u/87844133?v=4&s=39 "Vezel")](https://github.com/vezel-dev)
[![ChilliCream](https://avatars.githubusercontent.com/u/16239022?v=4&s=39 "ChilliCream")](https://github.com/ChilliCream)
[![4OTC](https://avatars.githubusercontent.com/u/68428092?v=4&s=39 "4OTC")](https://github.com/4OTC)
[![domischell](https://avatars.githubusercontent.com/u/66068846?u=0a5c5e2e7d90f15ea657bc660f175605935c5bea&v=4&s=39 "domischell")](https://github.com/DominicSchell)
[![Adrian Alonso](https://avatars.githubusercontent.com/u/2027083?u=129cf516d99f5cb2fd0f4a0787a069f3446b7522&v=4&s=39 "Adrian Alonso")](https://github.com/adalon)
[![torutek](https://avatars.githubusercontent.com/u/33917059?v=4&s=39 "torutek")](https://github.com/torutek)
[![mccaffers](https://avatars.githubusercontent.com/u/16667079?u=110034edf51097a5ee82cb6a94ae5483568e3469&v=4&s=39 "mccaffers")](https://github.com/mccaffers)
[![Seika Logiciel](https://avatars.githubusercontent.com/u/2564602?v=4&s=39 "Seika Logiciel")](https://github.com/SeikaLogiciel)
[![Andrew Grant](https://avatars.githubusercontent.com/devlooped-user?s=39 "Andrew Grant")](https://github.com/wizardness)
[![Lars](https://avatars.githubusercontent.com/u/1727124?v=4&s=39 "Lars")](https://github.com/latonz)


<!-- sponsors.md -->
[![Sponsor this project](https://avatars.githubusercontent.com/devlooped-sponsor?s=118 "Sponsor this project")](https://github.com/sponsors/devlooped)

[Learn more about GitHub Sponsors](https://github.com/sponsors)

<!-- https://github.com/devlooped/sponsors/raw/main/footer.md -->
