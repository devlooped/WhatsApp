namespace Devlooped.WhatsApp;

/// <summary>
/// Allows responding to console and WhatsApp simultaneously.
/// </summary>
record CompositeService : Service
{
    public CompositeService(Service primary, Service secondary)
        : base(primary.Id, primary.Number)
    {
        Secondary = secondary;
    }

    public Service Secondary { get; init; }
}
