using Microsoft.Extensions.DependencyInjection;

namespace Devlooped.WhatsApp;

/// <summary>
/// Provides extensions for configuring <see cref="MessageStorageHandler"/> instances.
/// </summary>
public static class StorageHandlerExtensions
{
    public static WhatsAppHandlerBuilder UseStorage(this WhatsAppHandlerBuilder builder)
    {
        _ = Throw.IfNull(builder);

        // By adding the storage service, the incoming and outgoing handlers will be automatically added to the pipeline
        builder.Services.AddSingleton<IStorageService, StorageService>(services => new StorageService(services.GetRequiredService<CloudStorageAccount>()));

        return builder;
    }
}