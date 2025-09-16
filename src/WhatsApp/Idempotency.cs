using System.Numerics;
using System.Security.Cryptography;
using System.Text;
using Azure;
using Azure.Data.Tables;
using Microsoft.Extensions.Caching.Hybrid;

namespace Devlooped.WhatsApp;

static class IdempotencyExtensions
{
    public static ValueTask<bool> IsProcessedAsync(this Idempotency idempotency, Message message, string json, CancellationToken token = default)
        => idempotency.IsProcessedAsync(message.User.Number, RowKey(message, json), token);

    public static async ValueTask<ETag?> TrySetProcessedAsync(this Idempotency idempotency, Message message, string json, CancellationToken token = default)
        => await idempotency.TrySetProcessedAsync(message.User.Number, RowKey(message, json), token);

    public static async ValueTask ResetProcessedAsync(this Idempotency idempotency, Message message, string json, ETag etag, CancellationToken token = default)
        => await idempotency.ResetProcessedAsync(message.User.Number, RowKey(message, json), etag, token);

    static string RowKey(Message message, string payload)
        => message.Id.StartsWith("wamid.", StringComparison.Ordinal) ? message.Id : Base62.Encode(new BigInteger(MD5.HashData(Encoding.UTF8.GetBytes(payload)), isUnsigned: true, isBigEndian: true));
}

class Idempotency(TableServiceClient client, HybridCache cache)
{
    static readonly HybridCacheEntryOptions options = new()
    {
        LocalCacheExpiration = TimeSpan.FromDays(3),
        Expiration = TimeSpan.FromDays(30)
    };

    readonly AsyncLazy<TableClient> table = new(async () =>
        {
            var table = client.GetTableClient("WhatsAppWebhook");
            await table.CreateIfNotExistsAsync();
            return table;
        });

    public ValueTask<bool> IsProcessedAsync(string partitionKey, string rowKey, CancellationToken token = default)
        => cache.GetOrCreateAsync(Key(partitionKey, rowKey),
            async key => await (await table).GetEntityIfExistsAsync<TableEntity>(partitionKey, rowKey, cancellationToken: token) is { HasValue: true },
            options, cancellationToken: token);

    public async ValueTask<ETag?> TrySetProcessedAsync(string partitionKey, string rowKey, CancellationToken token = default)
    {
        var key = Key(partitionKey, rowKey);
        try
        {
            var entity = await (await table).AddEntityAsync(new TableEntity(partitionKey, rowKey), token);
            await cache.SetAsync(key, true, options, cancellationToken: token);
            return entity.Headers.ETag ?? ETag.All;
        }
        catch (RequestFailedException ex) when (ex.Status == 409)
        {
            await cache.SetAsync(key, true, options, cancellationToken: token);
            return null;
        }
    }

    public async ValueTask ResetProcessedAsync(string partitionKey, string rowKey, ETag etag, CancellationToken token = default)
    {
        // If actual processing of a previously marked item failed, we want to return its unprocessed state
        var key = Key(partitionKey, rowKey);
        await (await table).DeleteEntityAsync(partitionKey, rowKey, etag, token);
        await cache.RemoveAsync(key, token);
    }

    static string Key(string partitionKey, string rowKey) => $"wa:dup:{partitionKey}/{rowKey}";
}
