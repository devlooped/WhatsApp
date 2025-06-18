using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

// Some users reported not getting emoji on Windows, so we force UTF-8 encoding.
// This not great, but I couldn't find a better way to do it.
if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
    Console.InputEncoding = Console.OutputEncoding = Encoding.UTF8;

// Alias -? to -h for help
if (args.Contains("-?"))
    args = [.. args.Select(x => x == "-?" ? "-h" : x)];

if (args.Contains("--debug"))
{
    Debugger.Launch();
    args = [.. args.Where(x => x != "--debug")];
}

var host = Host.CreateApplicationBuilder(args);
host.Configuration.AddDotNetConfig();
host.Configuration.AddUserSecrets<Program>();

var cts = new CancellationTokenSource();
Console.CancelKeyPress += (s, e) => cts.Cancel();
host.Services.AddSingleton(cts);

host.Services.AddServices();

var app = host.Build();

if (args.Contains("--version"))
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