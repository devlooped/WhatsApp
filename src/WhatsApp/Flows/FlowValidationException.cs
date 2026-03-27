namespace Devlooped.WhatsApp.Flows;

/// <summary>
/// Exception thrown when local Flow JSON validation fails.
/// </summary>
public class FlowValidationException : Exception
{
    /// <summary>
    /// Creates a new instance with the given validation result.
    /// </summary>
    /// <param name="result">The validation result containing errors.</param>
    public FlowValidationException(FlowValidationResult result)
        : base($"Flow JSON validation failed with {result.Errors.Count} error(s): {result.Errors.FirstOrDefault()?.Message}")
    {
        Result = result;
    }

    /// <summary>
    /// The validation result containing all errors.
    /// </summary>
    public FlowValidationResult Result { get; }
}
