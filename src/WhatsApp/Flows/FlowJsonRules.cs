using System.Text.Json.Nodes;
using System.Text.RegularExpressions;

namespace Devlooped.WhatsApp.Flows;

/// <summary>
/// Semantic validation rules for WhatsApp Flow JSON that cannot be expressed
/// in JSON Schema (cross-references, routing, component counts, etc.).
/// </summary>
static partial class FlowJsonRules
{
    // Component count limits per screen
    const int MaxComponentsPerScreen = 50;
    const int MaxEmbeddedLinksPerScreen = 2;
    const int MaxOptInsPerScreen = 5;
    const int MaxImagesPerScreen = 3;
    const int MaxPhotoPickersPerScreen = 1;
    const int MaxDocumentPickersPerScreen = 1;
    const int MaxNavigationListsPerScreen = 2;
    const int MaxImageCarouselsPerScreen = 2;
    const int MaxImageCarouselsPerFlow = 3;
    const int MaxIfNestingDepth = 3;
    const int MaxRoutingBranches = 10;

    [GeneratedRegex(@"\$\{data\.(\w+)\}", RegexOptions.Compiled)]
    private static partial Regex DataRefRegex();

    [GeneratedRegex(@"\$\{form\.(\w+)\}", RegexOptions.Compiled)]
    private static partial Regex FormRefRegex();

    [GeneratedRegex(@"\$\{screen\.(\w+)\.form\.(\w+)\}", RegexOptions.Compiled)]
    private static partial Regex GlobalFormRefRegex();

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

        ValidateScreenIdUniqueness(screens, errors, json);
        ValidateTerminalScreens(screenMap, errors, json);
        ValidateNavigateTargets(screenMap, errors, json);
        ValidateCompleteActionPlacement(screenMap, errors, json);
        ValidateComponentCounts(screenMap, errors, json);
        ValidatePickerExclusion(screenMap, errors, json);
        ValidateNavigationListPlacement(screenMap, errors, json);
        ValidateIfComponentRules(screenMap, errors, json);
        ValidateRoutingModel(root, screenMap, errors, json);
        ValidateImageCarouselFlowLimit(screenMap, errors, json);
        ValidateFooterCaptionConstraints(screenMap, errors, json);

        return errors;
    }

    static void ValidateScreenIdUniqueness(JsonArray screens, List<ValidationError> errors, string json)
    {
        var seen = new HashSet<string>();
        foreach (var screen in screens)
        {
            if (screen is JsonObject s && s["id"]?.GetValue<string>() is string id)
            {
                if (!seen.Add(id))
                {
                    var (line, col) = FlowJsonValidator.ResolveLocation(json, GetScreenPath(screens, s));
                    errors.Add(new ValidationError(
                        "DUPLICATE_SCREEN_ID",
                        "SEMANTIC_VALIDATION",
                        $"Duplicate screen ID '{id}'.",
                        line, line, col, col));
                }
            }
        }
    }

    static void ValidateTerminalScreens(Dictionary<string, JsonObject> screenMap, List<ValidationError> errors, string json)
    {
        var terminals = screenMap.Where(s => IsTerminal(s.Value)).ToList();
        if (terminals.Count == 0)
        {
            errors.Add(new ValidationError(
                "MISSING_TERMINAL_SCREEN",
                "SEMANTIC_VALIDATION",
                "Flow must have at least one terminal screen.",
                1, 1, 1, 1));
        }

        // Terminal screens must have a Footer
        foreach (var (id, screen) in terminals)
        {
            if (!ScreenHasFooter(screen))
            {
                errors.Add(new ValidationError(
                    "MISSING_FOOTER_ON_TERMINAL",
                    "SEMANTIC_VALIDATION",
                    $"Terminal screen '{id}' must have a Footer component.",
                    null, null, null, null));
            }
        }
    }

    static void ValidateNavigateTargets(Dictionary<string, JsonObject> screenMap, List<ValidationError> errors, string json)
    {
        foreach (var (screenId, screen) in screenMap)
        {
            foreach (var action in EnumerateActions(screen))
            {
                if (action["name"]?.GetValue<string>() is "navigate" &&
                    action["next"]?["name"]?.GetValue<string>() is string target)
                {
                    if (target == screenId)
                    {
                        errors.Add(new ValidationError(
                            "INVALID_NAVIGATE_ACTION_NEXT_SCREEN_NAME",
                            "SEMANTIC_VALIDATION",
                            $"Screen '{screenId}' cannot navigate to itself.",
                            null, null, null, null));
                    }
                    else if (!screenMap.ContainsKey(target))
                    {
                        errors.Add(new ValidationError(
                            "INVALID_NAVIGATE_ACTION_NEXT_SCREEN_NAME",
                            "SEMANTIC_VALIDATION",
                            $"Navigate action on screen '{screenId}' references non-existent screen '{target}'.",
                            null, null, null, null));
                    }
                }
            }
        }
    }

    static void ValidateCompleteActionPlacement(Dictionary<string, JsonObject> screenMap, List<ValidationError> errors, string json)
    {
        foreach (var (screenId, screen) in screenMap)
        {
            if (!IsTerminal(screen))
            {
                foreach (var action in EnumerateActions(screen))
                {
                    if (action["name"]?.GetValue<string>() is "complete")
                    {
                        errors.Add(new ValidationError(
                            "INVALID_COMPLETE_ACTION",
                            "SEMANTIC_VALIDATION",
                            $"'complete' action on screen '{screenId}' is only allowed on terminal screens.",
                            null, null, null, null));
                    }
                }
            }
        }
    }

    static void ValidateComponentCounts(Dictionary<string, JsonObject> screenMap, List<ValidationError> errors, string json)
    {
        foreach (var (screenId, screen) in screenMap)
        {
            var components = CollectAllComponents(screen);
            var counts = new Dictionary<string, int>();

            foreach (var comp in components)
            {
                var type = comp["type"]?.GetValue<string>() ?? "unknown";
                counts[type] = counts.GetValueOrDefault(type) + 1;
            }

            if (components.Count > MaxComponentsPerScreen)
            {
                errors.Add(new ValidationError(
                    "MAX_COMPONENTS_EXCEEDED",
                    "SEMANTIC_VALIDATION",
                    $"Screen '{screenId}' has {components.Count} components, maximum is {MaxComponentsPerScreen}.",
                    null, null, null, null));
            }

            CheckLimit(counts, "EmbeddedLink", MaxEmbeddedLinksPerScreen, screenId, errors);
            CheckLimit(counts, "OptIn", MaxOptInsPerScreen, screenId, errors);
            CheckLimit(counts, "Image", MaxImagesPerScreen, screenId, errors);
            CheckLimit(counts, "PhotoPicker", MaxPhotoPickersPerScreen, screenId, errors);
            CheckLimit(counts, "DocumentPicker", MaxDocumentPickersPerScreen, screenId, errors);
            CheckLimit(counts, "NavigationList", MaxNavigationListsPerScreen, screenId, errors);
            CheckLimit(counts, "ImageCarousel", MaxImageCarouselsPerScreen, screenId, errors);
        }
    }

    static void CheckLimit(Dictionary<string, int> counts, string type, int max, string screenId, List<ValidationError> errors)
    {
        if (counts.TryGetValue(type, out var count) && count > max)
        {
            errors.Add(new ValidationError(
                "MAX_COMPONENT_COUNT_EXCEEDED",
                "SEMANTIC_VALIDATION",
                $"Screen '{screenId}' has {count} {type} components, maximum is {max}.",
                null, null, null, null));
        }
    }

    static void ValidatePickerExclusion(Dictionary<string, JsonObject> screenMap, List<ValidationError> errors, string json)
    {
        foreach (var (screenId, screen) in screenMap)
        {
            var components = CollectAllComponents(screen);
            var hasPhoto = components.Any(c => c["type"]?.GetValue<string>() == "PhotoPicker");
            var hasDoc = components.Any(c => c["type"]?.GetValue<string>() == "DocumentPicker");

            if (hasPhoto && hasDoc)
            {
                errors.Add(new ValidationError(
                    "INCOMPATIBLE_COMPONENTS",
                    "SEMANTIC_VALIDATION",
                    $"Screen '{screenId}' cannot have both PhotoPicker and DocumentPicker.",
                    null, null, null, null));
            }
        }
    }

    static void ValidateNavigationListPlacement(Dictionary<string, JsonObject> screenMap, List<ValidationError> errors, string json)
    {
        foreach (var (screenId, screen) in screenMap)
        {
            if (IsTerminal(screen))
            {
                var components = CollectAllComponents(screen);
                if (components.Any(c => c["type"]?.GetValue<string>() == "NavigationList"))
                {
                    errors.Add(new ValidationError(
                        "INVALID_COMPONENT_PLACEMENT",
                        "SEMANTIC_VALIDATION",
                        $"NavigationList cannot be placed on terminal screen '{screenId}'.",
                        null, null, null, null));
                }
            }
        }
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

    static void ValidateImageCarouselFlowLimit(Dictionary<string, JsonObject> screenMap, List<ValidationError> errors, string json)
    {
        var total = 0;
        foreach (var (_, screen) in screenMap)
        {
            var components = CollectAllComponents(screen);
            total += components.Count(c => c["type"]?.GetValue<string>() == "ImageCarousel");
        }

        if (total > MaxImageCarouselsPerFlow)
        {
            errors.Add(new ValidationError(
                "MAX_COMPONENT_COUNT_EXCEEDED",
                "SEMANTIC_VALIDATION",
                $"Flow has {total} ImageCarousel components, maximum per flow is {MaxImageCarouselsPerFlow}.",
                null, null, null, null));
        }
    }

    static void ValidateFooterCaptionConstraints(Dictionary<string, JsonObject> screenMap, List<ValidationError> errors, string json)
    {
        foreach (var (screenId, screen) in screenMap)
        {
            foreach (var comp in CollectAllComponents(screen))
            {
                if (comp["type"]?.GetValue<string>() != "Footer")
                    continue;

                var hasCenter = comp["center-caption"] is not null;
                var hasLeft = comp["left-caption"] is not null;
                var hasRight = comp["right-caption"] is not null;

                if (hasCenter && (hasLeft || hasRight))
                {
                    errors.Add(new ValidationError(
                        "INCOMPATIBLE_FOOTER_CAPTIONS",
                        "SEMANTIC_VALIDATION",
                        $"Footer on screen '{screenId}' has center-caption combined with left/right-caption. These are mutually exclusive.",
                        null, null, null, null));
                }
            }
        }
    }

    // === Helper Methods ===

    static bool IsTerminal(JsonObject screen)
    {
        try { return screen["terminal"]?.GetValue<bool>() == true; }
        catch { return false; }
    }

    static JsonArray? GetLayoutChildren(JsonObject screen) =>
        screen["layout"]?["children"]?.AsArray();

    static bool ScreenHasFooter(JsonObject screen)
    {
        var children = GetLayoutChildren(screen);
        return children is not null && HasFooterInBranch(children);
    }

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

    static List<JsonObject> CollectAllComponents(JsonObject screen)
    {
        var result = new List<JsonObject>();
        var children = GetLayoutChildren(screen);
        if (children is not null)
            CollectComponents(children, result);
        return result;
    }

    static void CollectComponents(JsonArray children, List<JsonObject> result)
    {
        foreach (var child in children)
        {
            if (child is not JsonObject comp)
                continue;

            result.Add(comp);

            // Recurse into structural components
            if (comp["type"]?.GetValue<string>() == "Form" && comp["children"] is JsonArray formChildren)
                CollectComponents(formChildren, result);

            if (comp["type"]?.GetValue<string>() == "If")
            {
                if (comp["then"] is JsonArray t)
                    CollectComponents(t, result);
                if (comp["else"] is JsonArray e)
                    CollectComponents(e, result);
            }

            if (comp["type"]?.GetValue<string>() == "Switch" && comp["cases"] is JsonObject cases)
            {
                foreach (var prop in cases)
                {
                    if (prop.Value is JsonArray caseChildren)
                        CollectComponents(caseChildren, result);
                }
            }
        }
    }

    static IEnumerable<JsonObject> EnumerateActions(JsonObject screen)
    {
        var components = CollectAllComponents(screen);
        foreach (var comp in components)
        {
            if (comp["on-click-action"] is JsonObject action)
                yield return action;
            if (comp["on-select-action"] is JsonObject selectAction)
                yield return selectAction;
            if (comp["on-unselect-action"] is JsonObject unselectAction)
                yield return unselectAction;
        }
    }

    static string GetScreenPath(JsonArray screens, JsonObject screen)
    {
        for (var i = 0; i < screens.Count; i++)
        {
            if (screens[i] == screen)
                return $"/screens/{i}";
        }
        return "/screens";
    }
}
