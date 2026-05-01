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
        => idempotency.IsProcessedAsync(message.User.Id, RowKey(message, json), token);

    public static async ValueTask<ETag?> TrySetProcessedAsync(this Idempotency idempotency, Message message, string json, CancellationToken token = default)
        => await idempotency.TrySetProcessedAsync(message.User.Id, RowKey(message, json), token);

    public static async ValueTask ResetProcessedAsync(this Idempotency idempotency, Message message, string json, ETag etag, CancellationToken token = default)
        => await idempotency.ResetProcessedAsync(message.User.Id, RowKey(message, json), etag, token);

    static string RowKey(Message message, string payload)
        => message.Id.StartsWith("wamid.", StringComparison.Ordinal) ? message.Id : Base62.Encode(new BigInteger(MD5.HashData(Encoding.UTF8.GetBytes(payload)), isUnsigned: true, isBigEndian: true));
}

/// <summary>
/// Tracks which messages have already been processed to avoid duplicate handling.
/// When a <see cref="TableServiceClient"/> is provided (via
/// <see cref="IdempotencyStorageExtensions.UseIdempotencyStorage(WhatsAppHandlerBuilder, string)"/>),
/// idempotency is backed by Azure Table Storage for durability and atomic cross-instance claims.
/// Otherwise, it operates in cache-only mode using <see cref="HybridCache"/>, suitable for
/// single-process deployments or when a distributed cache (e.g. Redis) is configured.
/// </summary>
class Idempotency(HybridCache cache, TableServiceClient? table = null)
{
    static readonly HybridCacheEntryOptions options = new()
    {
        LocalCacheExpiration = TimeSpan.FromDays(3),
        Expiration = TimeSpan.FromDays(30)
    };

    readonly AsyncLazy<TableClient>? tableClient = table is null ? null : new(async () =>
        {
            var t = table.GetTableClient("WhatsAppWebhook");
            await t.CreateIfNotExistsAsync();
            return t;
        });

    public ValueTask<bool> IsProcessedAsync(string partitionKey, string rowKey, CancellationToken token = default)
    {
        var key = Key(partitionKey, rowKey);
        if (tableClient is null)
            return cache.GetOrCreateAsync(key, _ => ValueTask.FromResult(false), options, cancellationToken: token);

        return cache.GetOrCreateAsync(key,
            async _ => await (await tableClient).GetEntityIfExistsAsync<TableEntity>(partitionKey, rowKey, cancellationToken: token) is { HasValue: true },
            options, cancellationToken: token);
    }

    public async ValueTask<ETag?> TrySetProcessedAsync(string partitionKey, string rowKey, CancellationToken token = default)
    {
        var key = Key(partitionKey, rowKey);

        if (tableClient is null)
        {
            // Cache-only mode: best-effort claim. Suitable for single-process or distributed-cache scenarios.
            // A small TOCTOU race exists in multi-instance deployments without a distributed cache.
            if (await IsProcessedAsync(partitionKey, rowKey, token))
                return null;

            await cache.SetAsync(key, true, options, cancellationToken: token);
            return ETag.All;
        }

        try
        {
            var entity = await (await tableClient).AddEntityAsync(new TableEntity(partitionKey, rowKey), token);
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

        if (tableClient is null)
        {
            await cache.RemoveAsync(key, token);
            return;
        }

        await (await tableClient).DeleteEntityAsync(partitionKey, rowKey, etag, token);
        await cache.RemoveAsync(key, token);
    }

    static string Key(string partitionKey, string rowKey) => $"wa:dup:{partitionKey}/{rowKey}";
}
