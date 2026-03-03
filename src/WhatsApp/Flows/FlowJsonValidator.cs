using System.Text.Json;
using Json.Schema;

namespace Devlooped.WhatsApp.Flows;

/// <summary>
/// Validates Flow JSON documents against the WhatsApp Flows JSON Schema.
/// </summary>
public class FlowJsonValidator
{
    static readonly Lazy<FlowJsonValidator> instance = new(() => new FlowJsonValidator());
    readonly JsonSchema schema;

    FlowJsonValidator()
    {
        // ThisAssembly.Resources generates a property from the embedded resource path.
        // "Flows/FlowJson.schema.json" -> ThisAssembly.Resources.Flows.FlowJson_schema.Text
        var schemaText = ThisAssembly.Resources.Flows.FlowJson_schema.Text;
        schema = JsonSchema.FromText(schemaText);
    }

    /// <summary>
    /// Gets the singleton validator instance.
    /// </summary>
    public static FlowJsonValidator Instance => instance.Value;

    /// <summary>
    /// Validates a Flow JSON string.
    /// </summary>
    public FlowValidationResult Validate(string flowJson)
    {
        var element = JsonDocument.Parse(flowJson).RootElement;
        return Validate(element);
    }

    /// <summary>
    /// Validates a Flow JSON element.
    /// </summary>
    public FlowValidationResult Validate(JsonElement flowJson)
    {
        var result = schema.Evaluate(flowJson, new EvaluationOptions
        {
            OutputFormat = OutputFormat.List
        });

        if (result.IsValid)
            return new FlowValidationResult(true, []);

        var errors = new List<FlowValidationError>();
        CollectErrors(result, errors);
        return new FlowValidationResult(false, errors);
    }

    static void CollectErrors(EvaluationResults results, List<FlowValidationError> errors)
    {
        if (results.Errors != null)
        {
            foreach (var error in results.Errors)
            {
                errors.Add(new FlowValidationError(
                    results.InstanceLocation.ToString(),
                    error.Value));
            }
        }

        if (results.Details == null || results.Details.Count == 0)
            return;

        foreach (var detail in results.Details)
        {
            if (!detail.IsValid)
                CollectErrors(detail, errors);
        }
    }
}
