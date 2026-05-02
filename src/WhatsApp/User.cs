using System.Text.Json.Serialization;

namespace Devlooped.WhatsApp;

/// <summary>
/// WhatsApp end user that either originated a message or is the target of a message.
/// </summary>
public record User
{
    /// <summary>
    /// User's name.
    /// </summary>
    public string Name { get; init; }

    /// <summary>
    /// User identifier used to send messages back to this user. Either a phone number
    /// or a WhatsApp Business-Scoped User ID (BSUID) for users who opted for username privacy.
    /// </summary>
    public string Id { get; init; }

    /// <summary>
    /// User's phone number (normalized), or <see langword="null"/> when the user opted
    /// for username-only privacy and only a BSUID is available.
    /// </summary>
    public string? Number { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    /// <param name="name">User's name.</param>
    /// <param name="id">User identifier (phone number or BSUID) to send messages back to.</param>
    /// <param name="number">Optional phone number; <see langword="null"/> for privacy-enabled users.</param>
    public User(string name, string id, string? number = null)
    {
        Name = name;
        Id = id;
        Number = number?.NormalizeNumber();
        if (Number is null && !IsBSUID)
            Number = Id.NormalizeNumber();
    }

    /// <summary>
    /// Whether <see cref="Id"/> is a Business-Scoped User ID (BSUID) rather than a phone number.
    /// </summary>
    [JsonIgnore]
    public bool IsBSUID => IsBusinessScopedUserId(Id);

    /// <summary>
    /// Matches the BSUID format <c>{ISO 3166 alpha-2}.{1-128 alphanumeric}</c>,
    /// e.g. <c>US.13491208655302741918</c>. The two-letter country-code prefix
    /// is what distinguishes BSUIDs from phone numbers, which are all-digit.
    /// </summary>
    internal static bool IsBusinessScopedUserId(string id)
    {
        // Minimum: "XX.Y" = 4 chars; dot must be at position 2 (exactly 2-letter prefix).
        if (id.Length < 4 || id[2] != '.')
            return false;

        // Prefix: exactly 2 ASCII letters (ISO 3166 alpha-2).
        if (!char.IsAsciiLetter(id[0]) || !char.IsAsciiLetter(id[1]))
            return false;

        // Suffix: 1-128 ASCII alphanumeric characters.
        var suffixLength = id.Length - 3;
        if (suffixLength > 128)
            return false;

        for (var i = 3; i < id.Length; i++)
            if (!char.IsAsciiLetterOrDigit(id[i]))
                return false;

        return true;
    }
}
