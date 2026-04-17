using Azure.Data.Tables;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace Devlooped.WhatsApp;

public class IdempotencyTests
{
    static HybridCache BuildCache()
    {
        var services = new ServiceCollection();
        services.AddHybridCache();
        return services.BuildServiceProvider().GetRequiredService<HybridCache>();
    }

    [Fact]
    public async Task TableBackedMode_TracksDurably()
    {
        var serviceClient = CloudStorageAccount.DevelopmentStorageAccount.CreateTableServiceClient();
        var table = serviceClient.GetTableClient("WhatsAppWebhook");

        var idempotent = new Idempotency(BuildCache(), serviceClient);
        var pk = nameof(TableBackedMode_TracksDurably);
        var rk = Ulid.NewUlid().ToString();

        // Initially unprocessed
        Assert.False(await idempotent.IsProcessedAsync(pk, rk));

        // The etag is used for optimistic concurrency on resetting
        var etag = await idempotent.TrySetProcessedAsync(pk, rk);

        Assert.NotNull(etag);
        Assert.True(await idempotent.IsProcessedAsync(pk, rk));

        // Can't set again, the 409 conflict will mark it as true if it isn't already.
        Assert.Null(await idempotent.TrySetProcessedAsync(pk, rk));

        // Simulates a failure in processing so we're returning the item to the processing pool
        await idempotent.ResetProcessedAsync(pk, rk, etag.Value);

        Assert.False((await table.GetEntityIfExistsAsync<TableEntity>(pk, rk)).HasValue);

        // Simulate another process picking the item up at this point and writing back to storage
        await table.AddEntityAsync(new TableEntity(pk, rk));

        // The check would now re-read from storage and see it since we restored.
        Assert.True(await idempotent.IsProcessedAsync(pk, rk));
    }

    [Fact]
    public async Task CacheOnlyMode_TracksDuplicates()
    {
        var idempotent = new Idempotency(BuildCache());
        var pk = nameof(CacheOnlyMode_TracksDuplicates);
        var rk = Ulid.NewUlid().ToString();

        Assert.False(await idempotent.IsProcessedAsync(pk, rk));

        var etag = await idempotent.TrySetProcessedAsync(pk, rk);

        Assert.NotNull(etag);
        Assert.True(await idempotent.IsProcessedAsync(pk, rk));

        // Duplicate claim returns null
        Assert.Null(await idempotent.TrySetProcessedAsync(pk, rk));
    }

    [Fact]
    public async Task CacheOnlyMode_ResetAllowsReprocessing()
    {
        var idempotent = new Idempotency(BuildCache());
        var pk = nameof(CacheOnlyMode_ResetAllowsReprocessing);
        var rk = Ulid.NewUlid().ToString();

        var etag = await idempotent.TrySetProcessedAsync(pk, rk);
        Assert.NotNull(etag);

        await idempotent.ResetProcessedAsync(pk, rk, etag.Value);

        // After reset the item can be claimed again
        Assert.False(await idempotent.IsProcessedAsync(pk, rk));
        Assert.NotNull(await idempotent.TrySetProcessedAsync(pk, rk));
    }
}
