using System.Text.Json;
using System.Text.Json.Serialization;

namespace Devlooped.WhatsApp;

/// <summary>
/// A single rule in a <see cref="JsonRulesFile"/>.
/// </summary>
/// <param name="Id">Unique identifier for this rule.</param>
/// <param name="Context">JQ expression selecting 0..N nodes from the document to test.</param>
/// <param name="Test">JQ expression evaluated on each context node; must return <c>true</c> (pass) or <c>false</c> (fail).
/// Use <c>.</c> for the current context node and <c>$root</c> for the full document.</param>
/// <param name="Message">Human-readable description of the violation.</param>
/// <param name="ErrorCode">Optional error code for the violation. Defaults to the rule <see cref="Id"/>.</param>
public record JsonRule(
    string Id,
    string Context,
    string Test,
    string Message,
    [property: JsonPropertyName("errorCode")] string? ErrorCode = null);

/// <summary>
/// A collection of <see cref="JsonRule"/> instances loaded from a JSON rules file.
/// </summary>
/// <param name="Rules">The rules to apply.</param>
public record JsonRulesFile(IReadOnlyList<JsonRule> Rules);

/// <summary>
/// A violation found by a <see cref="JsonRulesEngine"/> rule.
/// </summary>
/// <param name="RuleId">The ID of the rule that was violated.</param>
/// <param name="ErrorCode">Error code for the violation.</param>
/// <param name="Message">Human-readable description.</param>
/// <param name="LineStart">Start line in the source JSON, if available.</param>
/// <param name="LineEnd">End line in the source JSON, if available.</param>
/// <param name="ColumnStart">Start column in the source JSON, if available.</param>
/// <param name="ColumnEnd">End column in the source JSON, if available.</param>
public record RuleViolation(
    string RuleId,
    string ErrorCode,
    string Message,
    int? LineStart = null,
    int? LineEnd = null,
    int? ColumnStart = null,
    int? ColumnEnd = null);

/// <summary>
/// Applies a set of JQ-based rules to a JSON document, returning all violations.
/// </summary>
/// <remarks>
/// Rules use a Schematron-inspired format: each rule declares a <c>context</c> (JQ expression
/// selecting 0..N nodes) and a <c>test</c> (JQ expression returning <c>true</c>/<c>false</c>
/// per context node). Rules that return <c>false</c> produce a <see cref="RuleViolation"/>.
///
/// The <c>test</c> expression always has access to:
/// <list type="bullet">
///   <item><c>.</c> — the current context node</item>
///   <item><c>$root</c> — the full document (for cross-document references)</item>
/// </list>
///
/// JQ <c>def</c> functions can be inlined in any <c>test</c> expression.
/// Parsed filter expressions are immutable and thread-safe.
/// </remarks>
public class JsonRulesEngine
{
    static readonly JsonSerializerOptions rulesSerializerOptions = new()
    {
        PropertyNameCaseInsensitive = true,
        ReadCommentHandling = JsonCommentHandling.Skip,
        AllowTrailingCommas = true,
    };

    readonly JsonRulesFile rulesFile;
    // Pre-compiled JqExpression per rule ID — thread-safe and reusable
    readonly IReadOnlyDictionary<string, JqExpression> compiledFilters;

    JsonRulesEngine(JsonRulesFile rulesFile, IReadOnlyDictionary<string, JqExpression> compiledFilters)
    {
        this.rulesFile = rulesFile;
        this.compiledFilters = compiledFilters;
    }

    /// <summary>
    /// Loads a rules engine from a JSON rules string.
    /// </summary>
    /// <param name="rulesJson">JSON content of the rules file.</param>
    /// <exception cref="JsonException">Thrown if <paramref name="rulesJson"/> is not valid rules JSON.</exception>
    /// <exception cref="JqException">Thrown if any rule contains an invalid JQ filter expression.</exception>
    public static JsonRulesEngine Load(string rulesJson)
    {
        Throw.IfNullOrEmpty(rulesJson);
        var file = JsonSerializer.Deserialize<JsonRulesFile>(rulesJson, rulesSerializerOptions)
            ?? throw new JsonException("Rules JSON deserialized to null.");

        var filters = new Dictionary<string, JqExpression>(file.Rules.Count);
        foreach (var rule in file.Rules)
        {
            // Combined expression: $root for full-doc access, select() yields only failing nodes
            var filter = $". as $root | {rule.Context} | select(({rule.Test}) | not)";
            filters[rule.Id] = Jq.Parse(filter);
        }

        return new JsonRulesEngine(file, filters);
    }

    /// <summary>
    /// Loads a rules engine from a JSON rules file on disk.
    /// </summary>
    /// <param name="path">Path to the rules JSON file.</param>
    /// <exception cref="JqException">Thrown if any rule contains an invalid JQ filter expression.</exception>
    public static JsonRulesEngine LoadFromFile(string path)
    {
        Throw.IfNullOrEmpty(path);
        return Load(File.ReadAllText(path));
    }

    /// <summary>
    /// Validates a JSON string against all rules, returning any violations found.
    /// </summary>
    /// <param name="json">The JSON string to validate.</param>
    public IReadOnlyList<RuleViolation> Validate(string json)
    {
        Throw.IfNullOrEmpty(json);
        using var doc = JsonDocument.Parse(json, new JsonDocumentOptions
        {
            CommentHandling = JsonCommentHandling.Skip,
            AllowTrailingCommas = true,
        });
        return Validate(doc.RootElement, json);
    }

    /// <summary>
    /// Validates a <see cref="JsonElement"/> against all rules, returning any violations found.
    /// Location info (line/column) is not available when using this overload.
    /// </summary>
    /// <param name="element">The JSON element to validate.</param>
    public IReadOnlyList<RuleViolation> Validate(JsonElement element) =>
        Validate(element, sourceJson: null);

    IReadOnlyList<RuleViolation> Validate(JsonElement root, string? sourceJson)
    {
        var violations = new List<RuleViolation>();

        foreach (var rule in rulesFile.Rules)
        {
            var expression = compiledFilters[rule.Id];
            IReadOnlyList<JsonElement> failingNodes;
            try
            {
                // Materialize eagerly so any JqException during lazy evaluation is caught here.
                failingNodes = expression.Evaluate(root).ToList();
            }
            catch (JqException ex)
            {
                violations.Add(new RuleViolation(
                    rule.Id,
                    "RULES_ENGINE_ERROR",
                    $"Rule '{rule.Id}' evaluation failed: {ex.Message}"));
                continue;
            }

            foreach (var node in failingNodes)
            {
                var (line, col) = sourceJson is not null
                    ? ResolveLocation(sourceJson, node)
                    : (null, null);

                violations.Add(new RuleViolation(
                    rule.Id,
                    rule.ErrorCode ?? rule.Id,
                    rule.Message,
                    line, line, col, col));
            }
        }

        return violations;
    }

    /// <summary>
    /// Resolves the approximate line/column of a <see cref="JsonElement"/> within a source JSON string.
    /// </summary>
    static (int? line, int? col) ResolveLocation(string json, JsonElement node)
    {
        var rawText = JsonSerializer.Serialize(node);
        // Use a prefix to avoid false matches on repeated short values
        var search = rawText.Length > 80 ? rawText[..80] : rawText;
        var offset = json.IndexOf(search, StringComparison.Ordinal);
        if (offset < 0)
            return (null, null);

        var line = 1;
        var col = 1;
        for (var i = 0; i < offset; i++)
        {
            if (json[i] == '\n') { line++; col = 1; }
            else col++;
        }
        return (line, col);
    }
}
