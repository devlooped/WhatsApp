using Azure.Data.Tables;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Devlooped.WhatsApp;

/// <summary>
/// Extensions for configuring durable Azure Table Storage-backed idempotency tracking.
/// </summary>
public static class IdempotencyStorageExtensions
{
    /// <summary>
    /// The keyed service key used to register the <see cref="TableServiceClient"/> dedicated to
    /// idempotency storage, so it does not conflict with any app-wide <see cref="TableServiceClient"/>.
    /// </summary>
    public const string ServiceKey = "WhatsAppIdempotencyStorage";

    /// <summary>
    /// Configures Azure Table Storage for durable, atomic idempotency tracking using an already
    /// registered (unkeyed) <see cref="TableServiceClient"/> from the dependency injection container.
    /// </summary>
    /// <param name="builder">The handler pipeline builder.</param>
    /// <remarks>
    /// Use this overload when a <see cref="TableServiceClient"/> is already registered in the
    /// service collection and you want to reuse it for idempotency storage.
    /// <para>
    /// Without calling any <c>UseIdempotencyStorage</c> overload, idempotency operates in 
    /// cache-only mode using <see cref="Microsoft.Extensions.Caching.Hybrid.HybridCache"/>,
    /// which is suitable for single-process deployments or when a distributed cache (e.g. Redis)
    /// is configured.
    /// </para>
    /// <para>
    /// Calling this method multiple times replaces the previous registration — the last call wins.
    /// </para>
    /// </remarks>
    public static WhatsAppHandlerBuilder UseIdempotencyStorage(this WhatsAppHandlerBuilder builder)
    {
        _ = Throw.IfNull(builder);

        ReplaceKeyedTableServiceClient(builder.Services);
        builder.Services.AddKeyedSingleton<TableServiceClient>(ServiceKey,
            (sp, _) => sp.GetRequiredService<TableServiceClient>());

        return builder;
    }

    /// <summary>
    /// Configures Azure Table Storage for durable, atomic idempotency tracking, in addition to
    /// the in-memory and distributed cache layers provided by default.
    /// </summary>
    /// <param name="builder">The handler pipeline builder.</param>
    /// <param name="connectionStringOrKey">
    /// A configuration key (resolved via <see cref="IConfiguration"/>) or a literal Azure Storage 
    /// connection string. Configuration lookup is attempted first; if no matching key is found, 
    /// the value is used as a literal connection string.
    /// </param>
    /// <remarks>
    /// Without calling any <c>UseIdempotencyStorage</c> overload, idempotency operates in 
    /// cache-only mode using <see cref="Microsoft.Extensions.Caching.Hybrid.HybridCache"/>,
    /// which is suitable for single-process deployments or when a distributed cache (e.g. Redis) 
    /// is configured. Table Storage adds durability across restarts and atomic cross-instance 
    /// claim semantics.
    /// <para>
    /// Calling this method multiple times replaces the previous registration — the last call wins.
    /// </para>
    /// </remarks>
    public static WhatsAppHandlerBuilder UseIdempotencyStorage(this WhatsAppHandlerBuilder builder, string connectionStringOrKey)
    {
        _ = Throw.IfNull(builder);
        _ = Throw.IfNull(connectionStringOrKey);

        ReplaceKeyedTableServiceClient(builder.Services);

        builder.Services.AddKeyedSingleton<TableServiceClient>(ServiceKey, (sp, _) =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var connectionString = config[connectionStringOrKey] ?? connectionStringOrKey;
            return new TableServiceClient(connectionString, new TableClientOptions
            {
#if DEBUG
                Diagnostics =
                {
                    IsLoggingEnabled = true,
                    IsLoggingContentEnabled = true,
                },
#endif
            });
        });

        return builder;
    }

    /// <summary>
    /// Configures Azure Table Storage for durable, atomic idempotency tracking using a pre-configured
    /// <see cref="TableServiceClient"/> instance.
    /// </summary>
    /// <param name="builder">The handler pipeline builder.</param>
    /// <param name="client">The <see cref="TableServiceClient"/> to use for idempotency storage.</param>
    /// <remarks>
    /// The client is registered as a keyed service under <see cref="ServiceKey"/> and does not
    /// affect any app-wide (unkeyed) <see cref="TableServiceClient"/> registrations.
    /// <para>
    /// Without calling any <c>UseIdempotencyStorage</c> overload, idempotency operates in 
    /// cache-only mode using <see cref="Microsoft.Extensions.Caching.Hybrid.HybridCache"/>,
    /// which is suitable for single-process deployments or when a distributed cache (e.g. Redis) 
    /// is configured.
    /// </para>
    /// <para>
    /// Calling this method multiple times replaces the previous registration — the last call wins.
    /// </para>
    /// </remarks>
    public static WhatsAppHandlerBuilder UseIdempotencyStorage(this WhatsAppHandlerBuilder builder, TableServiceClient client)
    {
        _ = Throw.IfNull(builder);
        _ = Throw.IfNull(client);

        ReplaceKeyedTableServiceClient(builder.Services);
        builder.Services.AddKeyedSingleton<TableServiceClient>(ServiceKey, client);

        return builder;
    }

    static void ReplaceKeyedTableServiceClient(IServiceCollection services)
    {
        var existing = services.FirstOrDefault(x =>
            x.IsKeyedService &&
            x.ServiceType == typeof(TableServiceClient) &&
            ServiceKey.Equals(x.ServiceKey));
        if (existing != null)
            services.Remove(existing);
    }
}
