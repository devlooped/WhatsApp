using System.Text.Json;
using System.Text.Json.Serialization;

namespace Devlooped.WhatsApp;

/// <summary>
/// Base class for all content types.
/// </summary>
[JsonPolymorphic]
[JsonDerivedType(typeof(DocumentContent), "document")]
[JsonDerivedType(typeof(ContactContent), "contacts")]
[JsonDerivedType(typeof(TextContent), "text")]
[JsonDerivedType(typeof(LocationContent), "location")]
[JsonDerivedType(typeof(ImageContent), "image")]
[JsonDerivedType(typeof(VideoContent), "video")]
[JsonDerivedType(typeof(AudioContent), "audio")]
[JsonDerivedType(typeof(UnknownContent), "unknown")]
public abstract record Content
{
    /// <summary>
    /// Get the type of content.
    /// </summary>
    [JsonIgnore]
    public abstract ContentType Type { get; }
}

/// <summary>
/// Content contains a document.
/// </summary>
/// <param name="Id">Document identifier.</param>
/// <param name="Name">Document name.</param>
/// <param name="Mime">Mime type of the document content.</param>
/// <param name="Sha256">Hash of the document content.</param>
public record DocumentContent(string Id, string Name, string Mime, string Sha256) : Content
{
    /// <inheritdoc/>
    public override ContentType Type => ContentType.Document;
}

/// <summary>
/// Content contains contact information.
/// </summary>
/// <param name="Name">Name of the contact.</param>
/// <param name="Surname">Surname of the contact.</param>
/// <param name="Numbers">Phone numbers of the contact.</param>
public record ContactContent(string Name, string Surname, string[] Numbers) : Content
{
    /// <inheritdoc/>
    public override ContentType Type => ContentType.Contact;
}

/// <summary>
/// Content contains a text message.
/// </summary>
/// <param name="Text">Message text.</param>
public record TextContent(string Text) : Content
{
    /// <inheritdoc/>
    public override ContentType Type => ContentType.Text;
}

/// <summary>
/// Represents a geographical location defined by its coordinates.
/// </summary>
/// <param name="Latitude">Specifies the north-south position of a point on the Earth's surface.</param>
/// <param name="Longitude">Specifies the east-west position of a point on the Earth's surface.</param>
public record Location(double Latitude, double Longitude);

/// <summary>
/// Content contains a location.
/// </summary>
/// <param name="Location">The location provided as content.</param>
/// <param name="Address">Optional address of the shared location.</param>
/// <param name="Name">Optional name of the shared location.</param>
/// <param name="Url">Optional URL of the shared location.</param>
public record LocationContent(Location Location, string? Address, string? Name, string? Url) : Content
{
    /// <inheritdoc/>
    public override ContentType Type => ContentType.Location;
}

/// <summary>
/// Base type containing shared info about media content.
/// </summary>
/// <param name="Id">The media identifier.</param>
/// <param name="Mime">The mime type of the media.</param>
/// <param name="Sha256">Hash of the media.</param>
public abstract record MediaContent(string Id, string Mime, string Sha256) : Content;

/// <summary>
/// Content contains audio.
/// </summary>
/// <param name="Id">The audio identifier.</param>
/// <param name="Mime">The mime type of the audio.</param>
/// <param name="Sha256">Hash of the audio.</param>
public record AudioContent(string Id, string Mime, string Sha256) : MediaContent(Id, Mime, Sha256)
{
    /// <inheritdoc/>
    public override ContentType Type => ContentType.Audio;
}

/// <summary>
/// Content contains an image.
/// </summary>
/// <param name="Id">The image identifier.</param>
/// <param name="Mime">The mime type of the image.</param>
/// <param name="Sha256">Hash of the image.</param>
public record ImageContent(string Id, string Mime, string Sha256) : MediaContent(Id, Mime, Sha256)
{
    /// <inheritdoc/>
    public override ContentType Type => ContentType.Image;
}

/// <summary>
/// Content contains video.
/// </summary>
/// <param name="Id">The video identifier.</param>
/// <param name="Mime">The mime type of the video.</param>
/// <param name="Sha256">Hash of the video.</param>
public record VideoContent(string Id, string Mime, string Sha256) : MediaContent(Id, Mime, Sha256)
{
    /// <inheritdoc/>
    public override ContentType Type => ContentType.Video;
}

/// <summary>
/// Content has an unknown type, so it's represented by its raw JSON data.
/// </summary>
/// <param name="Raw">JSON data.</param>
public record UnknownContent(JsonElement Raw) : Content
{
    /// <inheritdoc/>
    public override ContentType Type => ContentType.Unknown;
}
