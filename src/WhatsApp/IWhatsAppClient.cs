using System.Threading.Tasks;

namespace Devlooped.WhatsApp;

/// <summary>
/// Interface for WhatsApp for Business client API.
/// </summary>
public interface IWhatsAppClient
{
    /// <summary>
    /// Sends a raw payload object that must match the WhatsApp API.
    /// </summary>
    /// <param name="from">The phone identifier to send the message from.</param>
    /// <param name="payload">The message payload.</param>
    /// <returns>Whether the message was successfully sent.</returns>
    /// <see cref="https://developers.facebook.com/docs/whatsapp/cloud-api/reference/messages"/>
    Task<bool> SendAync(string from, object payload);
}