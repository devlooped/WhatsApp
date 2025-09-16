using Azure.Data.Tables;
using Microsoft.Extensions.Caching.Hybrid;
using Microsoft.Extensions.DependencyInjection;

namespace Devlooped.WhatsApp;

public class IdempotencyTests
{
    [Fact]
    public async Task CanAddProcessedItem()
    {
        var client = CloudStorageAccount.DevelopmentStorageAccount.CreateTableServiceClient();
        var table = client.GetTableClient("WhatsAppWebhook");
        var collection = new ServiceCollection();
        collection.AddHybridCache();
        var cache = collection.BuildServiceProvider().GetRequiredService<HybridCache>();

        var idempotent = new Idempotency(client, cache);
        var pk = nameof(CanAddProcessedItem);
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
}
