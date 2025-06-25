using System.Runtime.CompilerServices;
using System.Text.Json;
using Devlooped.WhatsApp;
using Microsoft.Extensions.Logging;

class ProcessHandler(ILogger<Program> logger, JsonSerializerOptions options) : IWhatsAppHandler
{
    public async IAsyncEnumerable<Response> HandleAsync(IEnumerable<IMessage> messages, [EnumeratorCancellation] CancellationToken cancellation = default)
    {
        // Avoid warning CS1998 // Async method lacks 'await' operators and will run synchronously
        await Task.Yield();

        var message = messages.Last();
        logger.LogInformation("💬 Received message: {Message}", message);

        if (message is ErrorMessage error)
        {
            // Reengagement error, we need to invite the user.
            if (error.Error.Code == 131047)
            {
                // Showcases how to use a pre-declared template response to reengage the user.
                yield return error.Template("reengagement", "es_AR");
            }
            else
            {
                logger.LogWarning("⚠️ Unknown error message received: {Error}", message);
            }
        }
        else if (message is InteractiveMessage interactive)
        {
            logger.LogWarning("👤 chose {Button} ({Title})", interactive.Button.Id, interactive.Button.Title);
            yield return interactive.Reply($"👤 chose: {interactive.Button.Title} ({interactive.Button.Id})");
        }
        else if (message is ReactionMessage reaction)
        {
            logger.LogInformation("👤 reaction: {Reaction}", reaction.Emoji);
            yield return reaction.Reply($"👤 reaction: {reaction.Emoji}");
        }
        else if (message is StatusMessage status)
        {
            logger.LogInformation("☑️ status: {Status}", status.Status);
        }
        else if (message is ContentMessage content)
        {
            yield return content.React("🧠");

            yield return content.Reply("Spinning my digital neurons...");
            yield return content.Typing();

            // simulate some hard work at hand, like doing some LLM-stuff :)
            await Task.Delay(2000);

            yield return content.Reply(
                $"""
                ☑️ Got your {content.Content.Type}
                ```
                {JsonSerializer.Serialize(content, options)}
                ```
                """,
                new Button("btn_good", "👍"),
                new Button("btn_bad", "👎"));

            yield return content.React("✅");
        }
        else if (message is UnsupportedMessage unsupported)
        {
            logger.LogWarning("⚠️ {Message}", unsupported);
        }
    }
}