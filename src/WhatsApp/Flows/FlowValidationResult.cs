namespace Devlooped.WhatsApp.Flows;

/// <summary>
/// Result of validating a Flow JSON document against the schema.
/// </summary>
/// <param name="IsValid">Whether the document passed validation.</param>
/// <param name="Errors">List of validation errors, if any.</param>
public record FlowValidationResult(bool IsValid, IReadOnlyList<FlowValidationError> Errors);

/// <summary>
/// A single validation error with its JSON path and message.
/// </summary>
/// <param name="Path">JSON Pointer path to the invalid element.</param>
/// <param name="Message">Description of the validation error.</param>
public record FlowValidationError(string Path, string Message);
