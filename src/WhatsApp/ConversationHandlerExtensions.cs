using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;

namespace Devlooped.WhatsApp;

public static class ConversationHandlerExtensions
{
    public static WhatsAppHandlerBuilder UseConversation(this WhatsAppHandlerBuilder builder)
    {
        _ = Throw.IfNull(builder);

        // This handle requires the storage dependency
        builder.UseStorage();

        // By adding the conversation service, the conversation handlers will be automatically added to the pipeline
        builder.Services.AddSingleton<IConversationService, ConversationService>(services
            => new ConversationService(
                services.GetRequiredService<IStorageService>(),
                services.GetRequiredService<ILogger<ConversationService>>()));

        return builder;
    }
}