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
    /// User's phone number (normalized).
    /// </summary>
    public string Number { get; init; }

    /// <summary>
    /// Initializes a new instance of the <see cref="User"/> class.
    /// </summary>
    /// <param name="name">User's name.</param>
    /// <param name="number">Users's number.</param>
    public User(string name, string number)
    {
        Name = name;
        Number = NormalizeNumber(number);
    }

    static string NormalizeNumber(string number) =>
        // On the web, we don't get the 9 after 54 \o/
        // so for Argentina numbers, we need to remove the 9.
        number.StartsWith("549", StringComparison.Ordinal) ? "54" + number[3..] : number;
}
