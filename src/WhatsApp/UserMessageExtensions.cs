namespace Devlooped.WhatsApp;

static class UserMessageExtensions
{
    /// <summary>
    /// Sends progress update for the given message. 
    /// <paramref name="sendTyping"/> implies <paramref name="markRead"/>. But you can 
    /// mark read without sending typing indicator.
    /// </summary>
    /// <remarks>
    /// Both actions (sending typing indicator and marking read) are ignored for exceptions since 
    /// the message may have been deleted by the user in the meantime.
    /// </remarks>
    public static async Task SendProgress(this UserMessage message, IWhatsAppClient client, bool markRead, bool sendTyping)
    {
        if (sendTyping is true)
        {
            await client.SendTyping(message).Ignore();
        }
        else if (markRead)
        {
            await client.MarkReadAsync(message.Service.Id, message.Id).Ignore();
        }
    }
}
