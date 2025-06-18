using DotNetConfig;
using NuGet.Configuration;
using NuGet.Protocol.Core.Types;
using NuGet.Versioning;
using Spectre.Console;

namespace Microsoft.Extensions.Hosting;

static class HostExtensions
{
    /// <summary>
    /// Runs the app and update check in paralell, and renders any update messages if available 
    /// after the command app completes.
    /// </summary>
    public static async Task RunWithUpdatesAsync(this IHost app, CancellationToken cancellation)
    {
        var updates = Task.Run(() => GetUpdatesAsync());

        await app.RunAsync(cancellation);

        if (await updates is { Length: > 0 } messages)
        {
            foreach (var message in messages)
                AnsiConsole.MarkupLine(message);
        }
    }

    /// <summary>
    /// Checks for updates to the tool and shows them if available.
    /// </summary>
    public static async Task ShowUpdatesAsync(this IHost app)
    {
        if (await GetUpdatesAsync(true) is { Length: > 0 } messages)
        {
            foreach (var message in messages)
                AnsiConsole.MarkupLine(message);
        }
    }

    /// <summary>
    /// Shows the app version, build date and release link.
    /// </summary>
    /// <param name="app"></param>
    public static void ShowVersion(this IHost app)
    {
        AnsiConsole.MarkupLine($"{ThisAssembly.Project.ToolCommandName} version [lime]{ThisAssembly.Project.Version}[/] ({ThisAssembly.Project.BuildDate})");
        AnsiConsole.MarkupLine($"[link]{ThisAssembly.Git.Url}/releases/tag/{ThisAssembly.Project.BuildRef}[/]");
    }

    static async Task<string[]> GetUpdatesAsync(bool forced = false)
    {
        var config = Config.Build(ConfigLevel.Global).GetSection(ThisAssembly.Project.ToolCommandName);

        // Check once a day max
        if (!forced)
        {
            var lastCheck = config.GetDateTime("checked") ?? DateTime.UtcNow.AddDays(-2);
            // if it's been > 24 hours since the last check, we'll check again
            if (lastCheck > DateTime.UtcNow.AddDays(-1))
                return [];
        }

        // We check from a different feed in this case.
        var civersion = ThisAssembly.Project.VersionPrefix.StartsWith("42.42.");

        var providers = Repository.Provider.GetCoreV3();
        var repository = new SourceRepository(new PackageSource(
            // use CI feed rather than production feed depending on which version we're using
            civersion && !string.IsNullOrEmpty(ThisAssembly.Project.SLEET_FEED_URL) ?
            ThisAssembly.Project.SLEET_FEED_URL :
            "https://api.nuget.org/v3/index.json"), providers);
        var resource = await repository.GetResourceAsync<PackageMetadataResource>();
        var localVersion = new NuGetVersion(ThisAssembly.Project.Version);
        // Only update to stable versions, not pre-releases
        var metadata = await resource.GetMetadataAsync(ThisAssembly.Project.PackageId, includePrerelease: false, false,
            new SourceCacheContext
            {
                NoCache = true,
                RefreshMemoryCache = true,
            },
            NuGet.Common.NullLogger.Instance, CancellationToken.None);

        var update = metadata
            .Select(x => x.Identity)
            .Where(x => x.Version > localVersion)
            .OrderByDescending(x => x.Version)
            .Select(x => x.Version)
            .FirstOrDefault();

        config.SetDateTime("checked", DateTime.UtcNow);

        if (update != null)
        {
            return [
                $"There is a new version of [yellow]{ThisAssembly.Project.PackageId}[/]: [dim]v{localVersion.ToNormalizedString()}[/] -> [lime]v{update.ToNormalizedString()}[/]",
                $"Update with: [yellow]dotnet[/] tool update -g {ThisAssembly.Project.PackageId}" +
                (civersion ? $" --source {ThisAssembly.Project.SLEET_FEED_URL}" : ""),
            ];
        }

        return [];
    }
}
