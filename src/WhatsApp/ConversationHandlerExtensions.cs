using Microsoft.Extensions.DependencyInjection;

namespace Devlooped.WhatsApp;

public static class ConversationHandlerExtensions
{
    public static WhatsAppHandlerBuilder UseConversation(this WhatsAppHandlerBuilder builder)
    {
        _ = Throw.IfNull(builder);

        // This handle requires the storage dependency
        builder.UseStorage();

        // Make sure the storage was not already configured
        if (builder.Services.FirstOrDefault(x => x.ServiceType == typeof(ConversationService)) == null)
        {
            // By adding the conversation service, the conversation handlers will be automatically added to the pipeline
            builder.Services.AddSingleton<IConversationService, ConversationService>(services
                => new ConversationService(services.GetRequiredService<IStorageService>()));
        }

        return builder;
    }
}