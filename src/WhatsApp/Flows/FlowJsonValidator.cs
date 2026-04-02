using System.Text.Json;
using System.Text.Json.Nodes;
using Json.Schema;

namespace Devlooped.WhatsApp.Flows;

/// <summary>
/// Result of validating Flow JSON.
/// </summary>
/// <param name="IsValid">Whether the Flow JSON is valid.</param>
/// <param name="Errors">List of validation errors, if any.</param>
public record FlowValidationResult(bool IsValid, IReadOnlyList<ValidationError> Errors)
{
    /// <summary>
    /// A successful validation result with no errors.
    /// </summary>
    public static FlowValidationResult Success { get; } = new(true, []);
}

/// <summary>
/// Validates WhatsApp Flow JSON against the v7.3 specification,
/// combining JSON Schema structural validation with programmatic semantic rules,
/// and a JQ-based rules engine for declarative cross-reference checks.
/// </summary>
public class FlowJsonValidator
{
    static readonly Lazy<JsonSchema> schema = new(() =>
    {
        var schemaJson = ThisAssembly.Resources.Flows.FlowJsonSchema.Text;
        return JsonSchema.FromText(schemaJson);
    });

    static readonly Lazy<JsonRulesEngine> rulesEngine = new(() =>
        JsonRulesEngine.Load(ThisAssembly.Resources.Flows.FlowRules.Text));

    /// <summary>
    /// Validates Flow JSON, returning all structural and semantic errors.
    /// </summary>
    /// <param name="json">The Flow JSON string to validate.</param>
    /// <returns>A validation result containing any errors found.</returns>
    public FlowValidationResult Validate(string json)
    {
        Throw.IfNullOrEmpty(json);

        JsonNode? node;
        try
        {
            node = JsonNode.Parse(json, documentOptions: new JsonDocumentOptions
            {
                CommentHandling = JsonCommentHandling.Skip,
                AllowTrailingCommas = true,
            });
        }
        catch (JsonException ex)
        {
            return new FlowValidationResult(false,
            [
                new ValidationError(
                    "INVALID_JSON",
                    "SYNTAX_ERROR",
                    ex.Message,
                    (int?)(ex.LineNumber + 1),
                    (int?)(ex.LineNumber + 1),
                    (int?)(ex.BytePositionInLine + 1),
                    (int?)(ex.BytePositionInLine + 1))
            ]);
        }

        if (node is null)
        {
            return new FlowValidationResult(false,
            [
                new ValidationError("INVALID_JSON", "SYNTAX_ERROR", "Flow JSON must not be null or empty.")
            ]);
        }

        var errors = new List<ValidationError>();

        // Tier 1: JSON Schema validation
        var schemaErrors = ValidateSchema(node, json);
        errors.AddRange(schemaErrors);

        // Tier 2: Programmatic semantic validation
        var semanticErrors = FlowJsonRules.Validate(node, json);
        errors.AddRange(semanticErrors);

        // Tier 3: JQ-based declarative rules engine
        var ruleViolations = rulesEngine.Value.Validate(json);
        foreach (var violation in ruleViolations)
        {
            errors.Add(new ValidationError(
                violation.ErrorCode,
                "RULES_VALIDATION",
                violation.Message,
                violation.LineStart, violation.LineEnd,
                violation.ColumnStart, violation.ColumnEnd));
        }

        return new FlowValidationResult(errors.Count == 0, errors);
    }

    static List<ValidationError> ValidateSchema(JsonNode node, string json)
    {
        var errors = new List<ValidationError>();

        var result = schema.Value.Evaluate(node, new EvaluationOptions
        {
            OutputFormat = OutputFormat.List,
            RequireFormatValidation = true,
        });

        if (result.IsValid)
            return errors;

        if (result.Details is not null)
        {
            foreach (var detail in result.Details)
            {
                if (detail.IsValid || !detail.HasErrors)
                    continue;

                // Skip if-condition evaluation failures from allOf+if/then type discrimination.
                // These are normal schema evaluation: when an if-condition doesn't match,
                // it's not an error, it just means that branch's then-clause doesn't apply.
                var evalPath = detail.EvaluationPath.ToString();
                if (evalPath.Contains("/if/") || evalPath.EndsWith("/if"))
                    continue;

                // Skip inner failures inside successful `not` blocks.
                // When not: { const: "X" } is applied to a value != "X", the inner const
                // correctly fails but the not itself succeeds. Only report the not node itself.
                if (evalPath.Contains("/not/"))
                    continue;

                // Skip individual oneOf branch failures. When oneOf has multiple branches,
                // non-matching branches report errors at paths like .../oneOf/1.
                // These are noise unless the oneOf itself fails (which produces its own error).
                if (IsOneOfBranchFailure(evalPath))
                    continue;

                var path = detail.InstanceLocation.ToString();

                if (detail.Errors is not { } detailErrors)
                    continue;

                foreach (var error in detailErrors)
                {
                    var errorCode = MapSchemaErrorCode(error.Key, detail);
                    var message = error.Value;

                    // Try to get line/column info from JSON path
                    var (line, col) = ResolveLocation(json, path);

                    errors.Add(new ValidationError(
                        errorCode,
                        "SCHEMA_VALIDATION",
                        message,
                        line, line, col, col));
                }
            }
        }

        return errors;
    }

    static string MapSchemaErrorCode(string keyword, EvaluationResults detail) => keyword switch
    {
        "required" => "MISSING_REQUIRED_TYPE_PROPERTY",
        "additionalProperties" or "false" or "" => "INVALID_PROPERTY_KEY",
        "enum" => "INVALID_ENUM_VALUE",
        "const" => "INVALID_PROPERTY_VALUE",
        "type" => "INVALID_PROPERTY_TYPE",
        "maxLength" => "MAX_CHARS_EXCEEDED",
        "minLength" => "MIN_CHARS_REQUIRED",
        "minItems" => "MIN_ITEMS_REQUIRED",
        "maxItems" => "MAX_ITEMS_EXCEEDED",
        "pattern" => "PATTERN_MISMATCH",
        "minimum" => "VALUE_BELOW_MINIMUM",
        "maximum" => "VALUE_ABOVE_MAXIMUM",
        "format" => "INVALID_FORMAT",
        "not" => "NOT_KEYWORD_SCHEMA_VALIDATION_FAILED",
        "oneOf" => "INVALID_PROPERTY_VALUE",
        "if" or "then" or "else" => "INVALID_PROPERTY_VALUE",
        "dependentRequired" => "INVALID_DEPENDENCIES",
        "minProperties" => "MIN_ITEMS_REQUIRED",
        _ => "SCHEMA_VALIDATION_FAILED",
    };

    /// <summary>
    /// Checks if the evaluation path represents a failing branch inside a oneOf.
    /// Paths like .../oneOf/0 or .../oneOf/1/... are individual branch evaluations
    /// that are noise unless the oneOf keyword itself fails.
    /// </summary>
    static bool IsOneOfBranchFailure(string evalPath)
    {
        var idx = evalPath.IndexOf("/oneOf/", StringComparison.Ordinal);
        if (idx < 0) return false;

        // Check that after /oneOf/ there's a branch index (digit)
        var afterOneOf = evalPath.AsSpan(idx + "/oneOf/".Length);
        return afterOneOf.Length > 0 && char.IsDigit(afterOneOf[0]);
    }

    /// <summary>
    /// Resolves a JSON Pointer path to approximate line/column in the JSON string.
    /// </summary>
    internal static (int? line, int? col) ResolveLocation(string json, string jsonPointerPath)
    {
        if (string.IsNullOrEmpty(jsonPointerPath) || jsonPointerPath == "/")
            return (1, 1);

        try
        {
            // Navigate JSON using System.Text.Json to find the byte offset
            using var doc = JsonDocument.Parse(json);
            var segments = jsonPointerPath.TrimStart('/').Split('/');
            var current = doc.RootElement;

            foreach (var segment in segments)
            {
                if (int.TryParse(segment, out var index) && current.ValueKind == JsonValueKind.Array)
                {
                    if (index < current.GetArrayLength())
                        current = current[index];
                    else
                        return (null, null);
                }
                else if (current.ValueKind == JsonValueKind.Object && current.TryGetProperty(segment, out var prop))
                {
                    current = prop;
                }
                else
                {
                    return (null, null);
                }
            }

            // Use the raw text to find the approximate location
            // We search for the property path in the JSON
            var rawText = current.GetRawText();
            var offset = json.IndexOf(rawText, StringComparison.Ordinal);
            if (offset >= 0)
            {
                var line = 1;
                var col = 1;
                for (var i = 0; i < offset; i++)
                {
                    if (json[i] == '\n')
                    {
                        line++;
                        col = 1;
                    }
                    else
                    {
                        col++;
                    }
                }
                return (line, col);
            }
        }
        catch
        {
            // Best-effort location resolution
        }

        return (null, null);
    }
}
