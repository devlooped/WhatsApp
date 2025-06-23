using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;

namespace Devlooped.WhatsApp;

/// <summary>
/// Extensions for configuring conversation handling in the WhatsApp handler pipeline.
/// </summary>
public static class ConversationHandlerExtensions
{
    /// <summary>
    /// Persists and restores messages in a conversation context.
    /// </summary>
    /// <param name="builder">The builder pipeline</param>
    /// <param name="conversationWindowSeconds">The timespan in seconds that defines the duration of an implicit conversation window.</param>
    public static WhatsAppHandlerBuilder UseConversation(this WhatsAppHandlerBuilder builder, int conversationWindowSeconds = 5 * 60 /* 5' */)
    {
        _ = Throw.IfNull(builder);

        // Make sure the storage was not already configured
        if (builder.Services.FirstOrDefault(x => x.ServiceType == typeof(IConversationStorage)) == null)
        {
            // By adding the conversation service, the conversation handlers will be automatically added to the pipeline
            builder.Services.AddSingleton<IConversationStorage, ConversationStorage>(services
                => new ConversationStorage(services.GetRequiredService<CloudStorageAccount>()));
        }

        return builder.Use((inner, services) => new ConversationHandler(inner,
            services.GetRequiredService<IConversationStorage>(),
            services.GetRequiredService<IOptions<WhatsAppOptions>>())
        {
            ConversationWindowSeconds = conversationWindowSeconds
        });
    }
}