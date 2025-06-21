using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Devlooped.WhatsApp;
using DotNetConfig;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Spectre.Console;

// Some users reported not getting emoji on Windows, so we force UTF-8 encoding.
// This not great, but I couldn't find a better way to do it.
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    Console.InputEncoding = Console.OutputEncoding = Encoding.UTF8;

var debug = false;
var help = false;
var version = false;
var url = default(string);

var options = new ConsoleOption
{
    { "?|h|help", "Display this help.", h => help = h != null },
    { "d|debug", "Debug the WhatsApp CLI.", d => debug = d != null, true },
    { "v|version", "Render tool version and updates.", v => version = v != null },
};

options.Parse(args);

if (debug)
    Debugger.Launch();

if (help)
{
    AnsiConsole.MarkupLine("Usage: [green]whatsapp[/] [grey][[OPTIONS]]+[/]");
    AnsiConsole.WriteLine("Options:");
    options.WriteOptionDescriptions(Console.Out);
    return 0;
}

if (url != null || options.Number != null || options.Format != null)
{
    var config = Config.Build(ConfigLevel.Global);
    if (url != null)
        config = config.SetString("whatsapp", "endpoint", url);
    if (options.Number != null)
        config = config.SetNumber("whatsapp", "number", (long)options.Number);
    if (options.Format != null)
        config = config.SetString("whatsapp", "format", options.Format.ToString()!.ToLowerInvariant());
}

var host = Host.CreateApplicationBuilder(args);
host.Logging.ClearProviders();

host.Configuration.AddDotNetConfig();
host.Configuration.AddUserSecrets<Program>();

var http = host.Services.AddHttpClient("whatsapp");
if (Debugger.IsAttached)
{
    http.ConfigureHttpClient(http => http.Timeout = TimeSpan.FromHours(1));
}
else
{
    http.AddStandardResilienceHandler();
}

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) => cts.Cancel();
host.Services.AddSingleton(cts);

host.Services.AddServices();

var app = host.Build();

if (version)
{
    app.ShowVersion();
    await app.ShowUpdatesAsync();
    return 0;
}

#if DEBUG
await app.RunWithUpdatesAsync(cts.Token);
#else
await app.RunAsync(cts.Token);
#endif

return 0;