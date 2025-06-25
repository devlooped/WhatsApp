using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;

namespace Devlooped.WhatsApp;

/// <summary>
/// Extensions for configuring a console handler in the WhatsApp handler pipeline.
/// </summary>
public static class ConsoleHandlerExtensions
{
    /// <summary>
    /// A development-only handler that marks messages sent via WhatsApp as coming from the 
    /// CLI if there is any user message in the conversation that came from the console.
    /// </summary>
    /// <returns>
    /// This handler is very useful for testing scenarios where the console falls short, such 
    /// as sending media files, contacts, locations, etc. It allows sending these messages 
    /// from WhatsApp to the same number used by the console, so that the console can receive 
    /// the responses.
    /// The corresponding handler is only added to the pipeline if the application is not 
    /// running in production mode as determined by the <see cref="IHostEnvironment"/>.
    /// </returns>
    public static WhatsAppHandlerBuilder UseConsole(this WhatsAppHandlerBuilder builder)
    {
        _ = Throw.IfNull(builder);

        return builder.Use((inner, services) =>
        {
            // In production environments, we have ZERO impact since we're not even added to the pipeline.
            if (services.GetRequiredService<IHostEnvironment>().IsProduction())
                return WhatsAppHandler.Continue;

            return new ConsoleHandler(inner);
        });
    }

    class ConsoleHandler(IWhatsAppHandler inner) : DelegatingWhatsAppHandler(inner)
    {
        public override IAsyncEnumerable<Response> HandleAsync(IEnumerable<IMessage> messages, CancellationToken cancellation = default)
        {
            Service? console = null;

            messages = [.. messages.Select(message =>
            {
                var user = message as UserMessage;
                if (message.FromConsole && user is not null)
                    console ??= user.Service;

                // Mark non-console user messages as coming from the console by 
                // composing the service id with the console service id for dual reply
                if (console != null && !message.FromConsole && user is not null)
                {
                    return user with
                    {
                        Service = new CompositeService(user.Service, console),
                        FromConsole = true,
                    };
                }
                // Otherwise, just return the message as is.
                return message;
            })];

            return base.HandleAsync(messages, cancellation);
        }
    }
}