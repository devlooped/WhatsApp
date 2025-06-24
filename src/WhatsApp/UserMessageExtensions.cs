namespace Devlooped.WhatsApp;

static class UserMessageExtensions
{
    public static async Task SendProgress(this UserMessage message, IWhatsAppClient client, bool markRead, bool sendTyping)
    {
        // These actions are ignored for exceptions since they may be triggered for an old, deleted message, for example.
        if (sendTyping is true)
        {
            if (markRead)
            {
                await client.SendAsync(message.Service.Id, new
                {
                    messaging_product = "whatsapp",
                    status = "read",
                    message_id = message.Id,
                    typing_indicator = new
                    {
                        type = "text"
                    }
                }).Ignore();
            }
            else
            {
                await client.SendAsync(message.Service.Id, new
                {
                    messaging_product = "whatsapp",
                    message_id = message.Id,
                    typing_indicator = new
                    {
                        type = "text"
                    }
                }).Ignore();
            }
        }
        else
        {
            await client.MarkReadAsync(message.Service.Id, message.Id).Ignore();
        }
    }
}
