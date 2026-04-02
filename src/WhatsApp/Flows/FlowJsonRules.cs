using System.Text.Json.Nodes;

namespace Devlooped.WhatsApp.Flows;

/// <summary>
/// Semantic validation rules for WhatsApp Flow JSON that cannot be expressed
/// in JSON Schema or the JQ-based rules engine (recursive depth tracking, graph cycle detection).
/// </summary>
static partial class FlowJsonRules
{
    const int MaxIfNestingDepth = 3;
    const int MaxRoutingBranches = 10;

    /// <summary>
    /// Runs all programmatic validation rules against the parsed Flow JSON.
    /// </summary>
    public static List<ValidationError> Validate(JsonNode node, string json)
    {
        var errors = new List<ValidationError>();

        if (node is not JsonObject root)
            return errors;

        var screensNode = root["screens"];
        if (screensNode is not JsonArray screens || screens.Count == 0)
            return errors; // Schema validation handles this

        var screenMap = new Dictionary<string, JsonObject>();
        foreach (var screen in screens)
        {
            if (screen is JsonObject s && s["id"]?.GetValue<string>() is string id)
                screenMap[id] = s;
        }

        ValidateIfComponentRules(screenMap, errors, json);
        ValidateRoutingModel(root, screenMap, errors, json);

        return errors;
    }

    static void ValidateIfComponentRules(Dictionary<string, JsonObject> screenMap, List<ValidationError> errors, string json)
    {
        foreach (var (screenId, screen) in screenMap)
        {
            var children = GetLayoutChildren(screen);
            if (children is null)
                continue;

            ValidateIfNesting(children, 0, screenId, errors);
        }
    }

    static void ValidateIfNesting(JsonArray children, int depth, string screenId, List<ValidationError> errors)
    {
        foreach (var child in children)
        {
            if (child is not JsonObject comp)
                continue;

            if (comp["type"]?.GetValue<string>() == "If")
            {
                if (depth >= MaxIfNestingDepth)
                {
                    errors.Add(new ValidationError(
                        "MAX_NESTING_EXCEEDED",
                        "SEMANTIC_VALIDATION",
                        $"If component on screen '{screenId}' exceeds maximum nesting depth of {MaxIfNestingDepth}.",
                        null, null, null, null));
                    continue;
                }

                if (comp["then"] is JsonArray thenChildren)
                    ValidateIfNesting(thenChildren, depth + 1, screenId, errors);
                if (comp["else"] is JsonArray elseChildren)
                    ValidateIfNesting(elseChildren, depth + 1, screenId, errors);

                // Check Footer presence in both branches for terminal screens
                var thenHasFooter = comp["then"] is JsonArray t && HasFooterInBranch(t);
                var elseHasFooter = comp["else"] is JsonArray e && HasFooterInBranch(e);
                if (thenHasFooter != elseHasFooter)
                {
                    errors.Add(new ValidationError(
                        "MISSING_FOOTER_IN_BRANCH",
                        "SEMANTIC_VALIDATION",
                        $"If component on screen '{screenId}' must have Footer in both 'then' and 'else' branches, or neither.",
                        null, null, null, null));
                }
            }

            // Recurse into Form children
            if (comp["type"]?.GetValue<string>() == "Form" && comp["children"] is JsonArray formChildren)
                ValidateIfNesting(formChildren, depth, screenId, errors);

            // Recurse into Switch cases
            if (comp["type"]?.GetValue<string>() == "Switch" && comp["cases"] is JsonObject cases)
            {
                foreach (var prop in cases)
                {
                    if (prop.Value is JsonArray caseChildren)
                        ValidateIfNesting(caseChildren, depth, screenId, errors);
                }
            }
        }
    }

    static void ValidateRoutingModel(JsonObject root, Dictionary<string, JsonObject> screenMap, List<ValidationError> errors, string json)
    {
        if (root["routing_model"] is not JsonObject routingModel)
            return;

        // Count total branches
        var totalBranches = 0;
        foreach (var prop in routingModel)
        {
            if (prop.Value is JsonArray targets)
                totalBranches += targets.Count;
        }

        if (totalBranches > MaxRoutingBranches)
        {
            errors.Add(new ValidationError(
                "INVALID_ROUTING_MODEL",
                "SEMANTIC_VALIDATION",
                $"Routing model has {totalBranches} branches, maximum is {MaxRoutingBranches}.",
                null, null, null, null));
        }

        // Validate all referenced screens exist
        foreach (var prop in routingModel)
        {
            var sourceScreen = prop.Key;
            if (!screenMap.ContainsKey(sourceScreen))
            {
                errors.Add(new ValidationError(
                    "INVALID_ROUTING_MODEL",
                    "SEMANTIC_VALIDATION",
                    $"Routing model references non-existent screen '{sourceScreen}'.",
                    null, null, null, null));
            }

            if (prop.Value is JsonArray targets)
            {
                foreach (var target in targets)
                {
                    var targetId = target?.GetValue<string>();
                    if (targetId is not null && !screenMap.ContainsKey(targetId))
                    {
                        errors.Add(new ValidationError(
                            "INVALID_ROUTING_MODEL",
                            "SEMANTIC_VALIDATION",
                            $"Routing model references non-existent target screen '{targetId}'.",
                            null, null, null, null));
                    }
                }
            }
        }

        // Detect self-navigation in routing model
        foreach (var prop in routingModel)
        {
            if (prop.Value is JsonArray targets)
            {
                foreach (var target in targets)
                {
                    if (target?.GetValue<string>() == prop.Key)
                    {
                        errors.Add(new ValidationError(
                            "INVALID_ROUTING_MODEL",
                            "SEMANTIC_VALIDATION",
                            $"Routing model has self-referencing route for screen '{prop.Key}'.",
                            null, null, null, null));
                    }
                }
            }
        }

        // Detect cycles (A→B→...→A)
        DetectRoutingCycles(routingModel, screenMap, errors);
    }

    static void DetectRoutingCycles(JsonObject routingModel, Dictionary<string, JsonObject> screenMap, List<ValidationError> errors)
    {
        var graph = new Dictionary<string, List<string>>();
        foreach (var prop in routingModel)
        {
            if (prop.Value is JsonArray targets)
            {
                var list = new List<string>();
                foreach (var t in targets)
                {
                    if (t?.GetValue<string>() is string target)
                        list.Add(target);
                }
                graph[prop.Key] = list;
            }
        }

        var visited = new HashSet<string>();
        var inStack = new HashSet<string>();

        foreach (var node in graph.Keys)
        {
            if (HasCycle(node, graph, visited, inStack))
            {
                errors.Add(new ValidationError(
                    "INVALID_ROUTING_MODEL",
                    "SEMANTIC_VALIDATION",
                    "Routing model contains a cycle.",
                    null, null, null, null));
                return;
            }
        }
    }

    static bool HasCycle(string node, Dictionary<string, List<string>> graph, HashSet<string> visited, HashSet<string> inStack)
    {
        if (inStack.Contains(node)) return true;
        if (visited.Contains(node)) return false;

        visited.Add(node);
        inStack.Add(node);

        if (graph.TryGetValue(node, out var neighbors))
        {
            foreach (var neighbor in neighbors)
            {
                if (HasCycle(neighbor, graph, visited, inStack))
                    return true;
            }
        }

        inStack.Remove(node);
        return false;
    }

    // === Helper Methods ===

    static JsonArray? GetLayoutChildren(JsonObject screen) =>
        screen["layout"]?["children"]?.AsArray();

    static bool HasFooterInBranch(JsonArray children)
    {
        foreach (var child in children)
        {
            if (child is not JsonObject comp)
                continue;

            if (comp["type"]?.GetValue<string>() == "Footer")
                return true;

            // Check inside Form
            if (comp["type"]?.GetValue<string>() == "Form" && comp["children"] is JsonArray formChildren)
            {
                if (HasFooterInBranch(formChildren))
                    return true;
            }

            // Check inside If branches
            if (comp["type"]?.GetValue<string>() == "If")
            {
                if (comp["then"] is JsonArray t && HasFooterInBranch(t))
                    return true;
                if (comp["else"] is JsonArray e && HasFooterInBranch(e))
                    return true;
            }

            // Check inside Switch cases
            if (comp["type"]?.GetValue<string>() == "Switch" && comp["cases"] is JsonObject cases)
            {
                foreach (var prop in cases)
                {
                    if (prop.Value is JsonArray caseChildren && HasFooterInBranch(caseChildren))
                        return true;
                }
            }
        }

        return false;
    }
}
