using System;
using System.Threading.Tasks;

namespace Devlooped.WhatsApp;

/// <summary>
/// Usability extensions for common messaging scenarios for WhatsApp.
/// </summary>
public static class WhatsAppClientExtensions
{
    /// <summary>
    /// Marks the message as read. Happens automatically when the <see cref="AzureFunctions.Message(Microsoft.AspNetCore.Http.HttpRequest)"/> 
    /// webhook endpoint is invoked with a message.
    /// </summary>
    public static Task MarkReadAsync(this IWhatsAppClient client, Message message)
        => MarkReadAsync(client, message.To.Id, message.Id);

    /// <summary>
    /// Marks the message as read. Happens automatically when the <see cref="AzureFunctions.Message(Microsoft.AspNetCore.Http.HttpRequest)"/> 
    /// webhook endpoint is invoked with a message.
    /// </summary>
    public static Task MarkReadAsync(this IWhatsAppClient client, string from, string messageId)
        => client.SendAsync(from, new
        {
            messaging_product = "whatsapp",
            status = "read",
            message_id = messageId,
        });

    /// <summary>
    /// Reacts to a message.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="message">The message to react to.</param>
    /// <param name="reaction">The reaction emoji.</param>
    public static Task ReactAsync(this IWhatsAppClient client, Message message, string reaction)
        => ReactAsync(client, message.To.Id, message.From.Number, message.Id, reaction);

    /// <summary>
    /// Reacts to a message.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="from">The service number to send the reaction through.</param>
    /// <param name="to">The user phone number to send the reaction to.</param>
    /// <param name="messageId">The message identifier to react to.</param>
    /// <param name="reaction">The reaction emoji.</param>
    public static Task ReactAsync(this IWhatsAppClient client, string from, string to, string messageId, string reaction)
        => client.SendAsync(from, new
        {
            messaging_product = "whatsapp",
            recipient_type = "individual",
            to = NormalizeNumber(to),
            type = "reaction",
            reaction = new
            {
                message_id = messageId,
                emoji = reaction
            }
        });

    /// <summary>
    /// Replies to a user message.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="message">The message to reply to.</param>
    /// <param name="reply">The text message to respond with.</param>
    public static Task ReplyAsync(this IWhatsAppClient client, Message message, string reply)
        => client.SendAsync(message.To.Id, new
        {
            messaging_product = "whatsapp",
            preview_url = false,
            recipient_type = "individual",
            to = NormalizeNumber(message.From.Number),
            type = "text",
            context = new
            {
                message_id = message.Id
            },
            text = new
            {
                body = reply
            }
        });

    /// <summary>
    /// Sends a text message a user given his incoming message, without making it a reply.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    public static Task SendAync(this IWhatsAppClient client, Message message, string text)
        => SendAync(client, message.To.Id, message.From.Number, text);

    /// <summary>
    /// Sends a text message a user.
    /// </summary>
    /// <param name="client">The WhatsApp client.</param>
    /// <param name="from">The service number to send the message through.</param>
    /// <param name="to">The user phone number to send the message to.</param>
    /// <param name="message">The text message to send.</param>
    public static Task SendAync(this IWhatsAppClient client, string from, string to, string message)
        => client.SendAsync(from, new
        {
            messaging_product = "whatsapp",
            preview_url = false,
            recipient_type = "individual",
            to = NormalizeNumber(to),
            type = "text",
            text = new
            {
                body = message
            }
        });

    static string NormalizeNumber(string number) =>
        // On the web, we don't get the 9 after 54 \o/
        // so for Argentina numbers, we need to remove the 9.
        number.StartsWith("549", StringComparison.Ordinal) ?
                "54" + number[3..] : number;
}