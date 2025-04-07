namespace Devlooped.WhatsApp;

/// <summary>
/// Defines the type of content.
/// </summary>
public enum ContentType
{
    Document,
    Contact,
    Text,
    Location,
    Image,
    Video,
    Audio,
    Unknown // For the 'raw' case
}