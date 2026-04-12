using System.CommandLine;
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

var urlOption = new Option<string?>("--url", "-u") { Description = "WhatsApp functions endpoint URL" };
var numberOption = new Option<string?>("--number", "-n") { Description = "Your WhatsApp user phone number" };
var jsonOption = new Option<bool>("--json", "-j") { Description = "Format output as JSON" };
var textOption = new Option<bool>("--text", "-t") { Description = "Format output as text" };
var yamlOption = new Option<bool>("--yaml", "-y") { Description = "Format output as YAML" };
var debugOption = new Option<bool>("--debug", "-d") { Description = "Launch debugger on start", Hidden = true };
var versionOption = new Option<bool>("--version", "-v") { Description = "Render tool version and updates." };

var rootCommand = new RootCommand("WhatsApp CLI simulator");
rootCommand.Options.Add(urlOption);
rootCommand.Options.Add(numberOption);
rootCommand.Options.Add(jsonOption);
rootCommand.Options.Add(textOption);
rootCommand.Options.Add(yamlOption);
rootCommand.Options.Add(debugOption);
rootCommand.Options.Add(versionOption);

var parsed = rootCommand.Parse(args);

// Let System.CommandLine handle --help and parse errors
if (args.Any(a => a is "--help" or "-h" or "-?" or "/?") || parsed.Errors.Count > 0)
    return await parsed.InvokeAsync();

if (parsed.GetValue(debugOption))
    Debugger.Launch();

var url = parsed.GetValue(urlOption);
var number = parsed.GetValue(numberOption);
OutputFormat? format = parsed.GetValue(jsonOption) ? OutputFormat.Json
                     : parsed.GetValue(textOption) ? OutputFormat.Text
                     : parsed.GetValue(yamlOption) ? OutputFormat.Yaml
                     : null;

// Apply to DotNetConfig BEFORE building host (so AddDotNetConfig reads updated values)
if (url != null || number != null || format != null)
{
    var config = Config.Build(ConfigLevel.Global);
    if (url != null)
        config = config.SetString("whatsapp", "endpoint", url);
    if (number != null)
        config = config.SetNumber("whatsapp", "number", long.Parse([.. number.Where(char.IsDigit)]));
    if (format != null)
        config = config.SetString("whatsapp", "format", format.ToString()!.ToLowerInvariant());
}

var host = Host.CreateApplicationBuilder(args);
host.Logging.ClearProviders();

host.Configuration.AddDotNetConfig();
host.Configuration.AddUserSecrets<Program>();

var http = host.Services.AddHttpClient("whatsapp");
http.ConfigureHttpClient(http => http.Timeout = TimeSpan.FromMinutes(30));

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) => cts.Cancel();
host.Services.AddSingleton(cts);

host.Services.AddServices();

var app = host.Build();

if (parsed.GetValue(versionOption))
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