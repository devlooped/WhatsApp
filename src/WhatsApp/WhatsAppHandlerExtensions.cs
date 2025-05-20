namespace Devlooped.WhatsApp;

/// <summary>
/// Provides the <see cref="AsBuilder"/> extension method to build a pipeline 
/// around a given handler.
/// </summary>
public static class WhatsAppHandlerExtensions
{
    /// <summary>
    /// Creates a new <see cref="WhatsAppHandlerBuilder"/> using <paramref name="handler"/> as its inner handler.
    /// </summary>
    /// <remarks>
    /// This method is equivalent to using the <see cref="WhatsAppHandlerBuilder"/> constructor directly.
    /// </remarks>
    /// <exception cref="ArgumentNullException"><paramref name="handler"/> is <see langword="null"/>.</exception>
    public static WhatsAppHandlerBuilder AsBuilder(this IWhatsAppHandler handler)
    {
        Throw.IfNull(handler);
        return new(_ => handler);
    }
}
