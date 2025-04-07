using System.Threading.Tasks;

namespace Devlooped.WhatsApp;

public interface IWhatsAppClient
{
    Task<bool> SendAync(string from, object payload);
}