namespace Devlooped.WhatsApp;

/// <summary>
/// Defines the type of content.
/// </summary>
public enum ContentType
{
    Audio,
    Contact,
    Document,
    Image,
    Location,
    Text,
    Video,
    Unknown // For the 'raw' case
}