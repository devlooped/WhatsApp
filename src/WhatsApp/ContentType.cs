using System.ComponentModel;

namespace Devlooped.WhatsApp;

/// <summary>
/// Defines the type of content.
/// </summary>
public enum ContentType
{
    Audio,
    [EditorBrowsable(EditorBrowsableState.Never)]
    Contact, // Legacy single-contact type
    Contacts,
    Document,
    Image,
    Location,
    Text,
    Video,
    Unknown // For the 'raw' case
}