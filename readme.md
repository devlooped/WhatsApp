![Icon](assets/img/icon.png) WhatsApp agents for Azure Functions
============

[![Version](https://img.shields.io/nuget/vpre/Devlooped.WhatsApp.svg?color=royalblue)](https://www.nuget.org/packages/Devlooped.WhatsApp)
[![Downloads](https://img.shields.io/nuget/dt/Devlooped.WhatsApp.svg?color=green)](https://www.nuget.org/packages/Devlooped.WhatsApp)
[![License](https://img.shields.io/github/license/devlooped/WhatsApp.svg?color=blue)](https://github.com//devlooped/WhatsApp/blob/main/license.txt)
[![Build](https://github.com/devlooped/WhatsApp/actions/workflows/build.yml/badge.svg?branch=main)](https://github.com/devlooped/WhatsApp/actions/workflows/build.yml)

<!-- #content -->

Create agents for WhatsApp using Azure Functions.

## Usage

```csharp
var builder = FunctionsApplication.CreateBuilder(args);
builder.ConfigureFunctionsWebApplication();

builder.UseWhatsApp<MyWhatsAppHandler>();

builder.Build().Run();
```

Instead of providing an `IWhatsAppHandler` implementation, you can also 
register an inline handler using minimal API style:

```csharp
builder.UseWhatsApp(message =>
{
    // MessageType: Content | Error | Interactive | Status
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
            Console.WriteLine($"Interactive: {interactive.Button.Title} ({interactive.Button.Id})");
            break;
        case StatusMessage status:
            Console.WriteLine($"Status: {status.Status}");
            break;
    }
    return Task.CompletedTask;
});
```

If the handler needs additional services, they can be provided directly 
as generic parameters of the `UseWhatsApp` method, such as:

```csharp
builder.UseWhatsApp<IWhatsAppClient, ILogger<Program>>(async (client, logger, message) =>
{
    logger.LogInformation($"Got message type {message.Type}");
    // Reply to an incoming content message, for example.
    if (message is ContentMessage content)
        await client.ReplyAsync(message, $"☑️ Got your {content.Content.Type}");
}
```

You can also specify the parameter types in the delegate itself and avoid the 
generic parameters:

```csharp
builder.UseWhatsApp(async (IWhatsAppClient client, ILogger<Program> logger, Message message) =>
```

The provided `IWhatsAppClient` provides a very thin abstraction allowing you to send 
arbitrary payloads to WhatsApp for Business:

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
    await client.ReplyAsync(message, $"☑️ Got your {content.Content.Type}");
}
```

## Configuration

You need to register an app in the Meta [App Dashboard](https://developers.facebook.com/apps/]. 
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

## Scalability and Performance

In order to quickly and efficiently process incoming messages, the library uses 
Azure Storage Queues to queue incoming messages from WhatsApp, which provides 
a reliable and scalable way to handle incoming messages. It also uses Azure Table Storage 
to detect duplicate messages and avoid processing the same message multiple times.

If `QueueServiceClient` and `TableServiceClient` are registered in the DI container 
before invoking `UseWhatsApp`, the library will automatically use them. Otherwise, 
it will register both services using the `AzureWebJobsStorage` connection string, 
therefore sharing storage with the Azure Functions runtime.

## License

We offer this project under a dual licensing model, tailored to the needs 
of commercial distributors and open-source projects.

**For open-source projects and free software developers:**

If you develop free software (FOSS) and want to leverage this project, 
the open-source version under AGPLv3 is ideal. 
If you use a FOSS license other than AGPLv3, Devlooped offers an exception, 
allowing usage without requiring the entire derivative work to fall under 
AGPLv3, under certain conditions.

See [AGPLv3](https://opensource.org/license/agpl-v3) and 
[Universal FOSS Exception](https://oss.oracle.com/licenses/universal-foss-exception/).

**For OEMs, ISVs, VARs, and other commercial application distributors:**

If you use this project and distribute or host commercial software without 
sharing the code under AGPLv3, you must obtain a commercial license from 
[Devlooped](mailto:hello@devlooped.com).

<!-- #content -->
<!-- include https://github.com/devlooped/sponsors/raw/main/footer.md -->
# Sponsors 

<!-- sponsors.md -->
[![Clarius Org](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/clarius.png "Clarius Org")](https://github.com/clarius)
[![MFB Technologies, Inc.](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/MFB-Technologies-Inc.png "MFB Technologies, Inc.")](https://github.com/MFB-Technologies-Inc)
[![Torutek](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/torutek-gh.png "Torutek")](https://github.com/torutek-gh)
[![DRIVE.NET, Inc.](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/drivenet.png "DRIVE.NET, Inc.")](https://github.com/drivenet)
[![Keith Pickford](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Keflon.png "Keith Pickford")](https://github.com/Keflon)
[![Thomas Bolon](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/tbolon.png "Thomas Bolon")](https://github.com/tbolon)
[![Kori Francis](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/kfrancis.png "Kori Francis")](https://github.com/kfrancis)
[![Toni Wenzel](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/twenzel.png "Toni Wenzel")](https://github.com/twenzel)
[![Uno Platform](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/unoplatform.png "Uno Platform")](https://github.com/unoplatform)
[![Dan Siegel](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/dansiegel.png "Dan Siegel")](https://github.com/dansiegel)
[![Reuben Swartz](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/rbnswartz.png "Reuben Swartz")](https://github.com/rbnswartz)
[![Jacob Foshee](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/jfoshee.png "Jacob Foshee")](https://github.com/jfoshee)
[![](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Mrxx99.png "")](https://github.com/Mrxx99)
[![Eric Johnson](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/eajhnsn1.png "Eric Johnson")](https://github.com/eajhnsn1)
[![Ix Technologies B.V.](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/IxTechnologies.png "Ix Technologies B.V.")](https://github.com/IxTechnologies)
[![David JENNI](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/davidjenni.png "David JENNI")](https://github.com/davidjenni)
[![Jonathan ](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Jonathan-Hickey.png "Jonathan ")](https://github.com/Jonathan-Hickey)
[![Charley Wu](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/akunzai.png "Charley Wu")](https://github.com/akunzai)
[![Jakob Tikjøb Andersen](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/jakobt.png "Jakob Tikjøb Andersen")](https://github.com/jakobt)
[![Ken Bonny](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/KenBonny.png "Ken Bonny")](https://github.com/KenBonny)
[![Simon Cropp](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/SimonCropp.png "Simon Cropp")](https://github.com/SimonCropp)
[![agileworks-eu](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/agileworks-eu.png "agileworks-eu")](https://github.com/agileworks-eu)
[![sorahex](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/sorahex.png "sorahex")](https://github.com/sorahex)
[![Zheyu Shen](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/arsdragonfly.png "Zheyu Shen")](https://github.com/arsdragonfly)
[![Vezel](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/vezel-dev.png "Vezel")](https://github.com/vezel-dev)
[![ChilliCream](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/ChilliCream.png "ChilliCream")](https://github.com/ChilliCream)
[![4OTC](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/4OTC.png "4OTC")](https://github.com/4OTC)
[![Vincent Limo](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/v-limo.png "Vincent Limo")](https://github.com/v-limo)
[![Jordan S. Jones](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/jordansjones.png "Jordan S. Jones")](https://github.com/jordansjones)
[![domischell](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/DominicSchell.png "domischell")](https://github.com/DominicSchell)


<!-- sponsors.md -->

[![Sponsor this project](https://raw.githubusercontent.com/devlooped/sponsors/main/sponsor.png "Sponsor this project")](https://github.com/sponsors/devlooped)
&nbsp;

[Learn more about GitHub Sponsors](https://github.com/sponsors)

<!-- https://github.com/devlooped/sponsors/raw/main/footer.md -->
