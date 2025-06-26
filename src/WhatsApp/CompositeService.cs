namespace Devlooped.WhatsApp;

/// <summary>
/// Allows responding to console and WhatsApp simultaneously.
/// </summary>
/// <remarks>
/// This combined service is only ever created whenever a WhatsApp message is sent 
/// within the context window of a console-driven conversation. It allows complementing 
/// the CLI thread by sending messages that can only be sent via WhatsApp, such as 
/// media, documents or contacts.
/// </remarks>
record CompositeService : Service
{
    public CompositeService(Service primary, Service secondary)
        : base(primary.Id, primary.Number)
    {
        Secondary = secondary;
    }

    public Service Secondary { get; init; }
}
