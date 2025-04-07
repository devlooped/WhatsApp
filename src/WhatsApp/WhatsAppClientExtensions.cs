using System;
using System.Threading.Tasks;

namespace Devlooped.WhatsApp;

public static class WhatsAppClientExtensions
{
    public static Task MarkReadAsync(this IWhatsAppClient client, string from, string messageId)
        => client.SendAync(from, new
        {
            messaging_product = "whatsapp",
            status = "read",
            message_id = messageId,
        });


    public static Task<bool> ReactAsync(this IWhatsAppClient client, string from, string to, string messageId, string reaction)
        => client.SendAync(from, new
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

    public static Task<bool> SendTextAync(this IWhatsAppClient client, string from, string to, string message)
        => client.SendAync(from, new
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