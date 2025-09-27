<!-- include ../../readme.md#cli -->
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
<!-- ../../readme.md#cli -->
<!-- include https://github.com/devlooped/sponsors/raw/main/footer.md -->
# Sponsors 

<!-- sponsors.md -->
[![Clarius Org](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/clarius.png "Clarius Org")](https://github.com/clarius)
[![MFB Technologies, Inc.](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/MFB-Technologies-Inc.png "MFB Technologies, Inc.")](https://github.com/MFB-Technologies-Inc)
[![DRIVE.NET, Inc.](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/drivenet.png "DRIVE.NET, Inc.")](https://github.com/drivenet)
[![Keith Pickford](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Keflon.png "Keith Pickford")](https://github.com/Keflon)
[![Thomas Bolon](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/tbolon.png "Thomas Bolon")](https://github.com/tbolon)
[![Kori Francis](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/kfrancis.png "Kori Francis")](https://github.com/kfrancis)
[![Toni Wenzel](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/twenzel.png "Toni Wenzel")](https://github.com/twenzel)
[![Uno Platform](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/unoplatform.png "Uno Platform")](https://github.com/unoplatform)
[![Reuben Swartz](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/rbnswartz.png "Reuben Swartz")](https://github.com/rbnswartz)
[![Jacob Foshee](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/jfoshee.png "Jacob Foshee")](https://github.com/jfoshee)
[![](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Mrxx99.png "")](https://github.com/Mrxx99)
[![Eric Johnson](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/eajhnsn1.png "Eric Johnson")](https://github.com/eajhnsn1)
[![David JENNI](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/davidjenni.png "David JENNI")](https://github.com/davidjenni)
[![Jonathan ](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Jonathan-Hickey.png "Jonathan ")](https://github.com/Jonathan-Hickey)
[![Charley Wu](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/akunzai.png "Charley Wu")](https://github.com/akunzai)
[![Ken Bonny](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/KenBonny.png "Ken Bonny")](https://github.com/KenBonny)
[![Simon Cropp](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/SimonCropp.png "Simon Cropp")](https://github.com/SimonCropp)
[![agileworks-eu](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/agileworks-eu.png "agileworks-eu")](https://github.com/agileworks-eu)
[![Zheyu Shen](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/arsdragonfly.png "Zheyu Shen")](https://github.com/arsdragonfly)
[![Vezel](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/vezel-dev.png "Vezel")](https://github.com/vezel-dev)
[![ChilliCream](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/ChilliCream.png "ChilliCream")](https://github.com/ChilliCream)
[![4OTC](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/4OTC.png "4OTC")](https://github.com/4OTC)
[![Vincent Limo](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/v-limo.png "Vincent Limo")](https://github.com/v-limo)
[![Jordan S. Jones](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/jordansjones.png "Jordan S. Jones")](https://github.com/jordansjones)
[![domischell](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/DominicSchell.png "domischell")](https://github.com/DominicSchell)
[![Justin Wendlandt](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/jwendl.png "Justin Wendlandt")](https://github.com/jwendl)
[![Adrian Alonso](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/adalon.png "Adrian Alonso")](https://github.com/adalon)
[![Michael Hagedorn](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/Eule02.png "Michael Hagedorn")](https://github.com/Eule02)
[![torutek](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/torutek.png "torutek")](https://github.com/torutek)
[![mccaffers](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/mccaffers.png "mccaffers")](https://github.com/mccaffers)
[![Alex Wiese](https://raw.githubusercontent.com/devlooped/sponsors/main/.github/avatars/alexwiese.png "Alex Wiese")](https://github.com/alexwiese)


<!-- sponsors.md -->

[![Sponsor this project](https://raw.githubusercontent.com/devlooped/sponsors/main/sponsor.png "Sponsor this project")](https://github.com/sponsors/devlooped)
&nbsp;

[Learn more about GitHub Sponsors](https://github.com/sponsors)

<!-- https://github.com/devlooped/sponsors/raw/main/footer.md -->
