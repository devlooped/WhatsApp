using System.Text.Json;

namespace Devlooped.WhatsApp.Tests;

public class JsonRulesEngineTests
{
    // Minimal valid Flow JSON for use in rule tests
    static string MinimalFlow(bool terminal = true, string screenId = "WELCOME") => $$"""
        {
          "version": "7.3",
          "screens": [
            {
              "id": "{{screenId}}",
              "terminal": {{(terminal ? "true" : "false")}},
              "layout": {
                "type": "SingleColumnLayout",
                "children": [
                  {
                    "type": "Footer",
                    "label": "Submit",
                    "on-click-action": { "name": "complete", "payload": {} }
                  }
                ]
              }
            }
          ]
        }
        """;

    [Fact]
    public void Load_InvalidJson_Throws()
    {
        Assert.ThrowsAny<Exception>(() => JsonRulesEngine.Load("not json"));
    }

    [Fact]
    public void Load_ValidRulesJson_Succeeds()
    {
        var engine = JsonRulesEngine.Load("""{ "rules": [] }""");
        Assert.NotNull(engine);
    }

    [Fact]
    public void Validate_EmptyRules_ReturnsNoViolations()
    {
        var engine = JsonRulesEngine.Load("""{ "rules": [] }""");
        var violations = engine.Validate(MinimalFlow());
        Assert.Empty(violations);
    }

    [Fact]
    public void Validate_UniqueScreenIds_Pass()
    {
        var engine = JsonRulesEngine.Load("""
            {
              "rules": [{
                "id": "unique_screen_ids",
                "context": ".screens",
                "test": "map(.id) | length == (unique | length)",
                "message": "Screen IDs must be unique.",
                "errorCode": "DUPLICATE_SCREEN_ID"
              }]
            }
            """);

        var json = """
            {
              "version": "7.3",
              "screens": [
                { "id": "A", "terminal": true, "layout": { "type": "SingleColumnLayout", "children": [] } },
                { "id": "B", "terminal": false, "layout": { "type": "SingleColumnLayout", "children": [] } }
              ]
            }
            """;

        Assert.Empty(engine.Validate(json));
    }

    [Fact]
    public void Validate_UniqueScreenIds_Fail()
    {
        var engine = JsonRulesEngine.Load("""
            {
              "rules": [{
                "id": "unique_screen_ids",
                "context": ".screens",
                "test": "map(.id) | length == (unique | length)",
                "message": "Screen IDs must be unique.",
                "errorCode": "DUPLICATE_SCREEN_ID"
              }]
            }
            """);

        var json = """
            {
              "version": "7.3",
              "screens": [
                { "id": "SAME", "terminal": true, "layout": { "type": "SingleColumnLayout", "children": [] } },
                { "id": "SAME", "terminal": false, "layout": { "type": "SingleColumnLayout", "children": [] } }
              ]
            }
            """;

        var violations = engine.Validate(json);
        Assert.Single(violations);
        Assert.Equal("DUPLICATE_SCREEN_ID", violations[0].ErrorCode);
    }

    [Fact]
    public void Validate_NavigateTargetExists_Pass()
    {
        var engine = JsonRulesEngine.Load("""
            {
              "rules": [{
                "id": "navigate_target_exists",
                "context": ".screens[].layout.children[] | select(.[\"on-click-action\"]?.name == \"navigate\")",
                "test": ".[\"on-click-action\"].next.name as $t | $root.screens | any(.id == $t)",
                "message": "Navigate target must exist.",
                "errorCode": "INVALID_NAVIGATE_TARGET"
              }]
            }
            """);

        var json = """
            {
              "version": "7.3",
              "screens": [
                {
                  "id": "SCREEN1",
                  "terminal": false,
                  "layout": {
                    "type": "SingleColumnLayout",
                    "children": [{
                      "type": "Footer",
                      "label": "Next",
                      "on-click-action": { "name": "navigate", "next": { "name": "SCREEN2" }, "payload": {} }
                    }]
                  }
                },
                {
                  "id": "SCREEN2",
                  "terminal": true,
                  "layout": { "type": "SingleColumnLayout", "children": [] }
                }
              ]
            }
            """;

        Assert.Empty(engine.Validate(json));
    }

    [Fact]
    public void Validate_NavigateTargetExists_Fail()
    {
        var engine = JsonRulesEngine.Load("""
            {
              "rules": [{
                "id": "navigate_target_exists",
                "context": ".screens[].layout.children[] | select(.[\"on-click-action\"]?.name == \"navigate\")",
                "test": ".[\"on-click-action\"].next.name as $t | $root.screens | any(.id == $t)",
                "message": "Navigate target must exist.",
                "errorCode": "INVALID_NAVIGATE_TARGET"
              }]
            }
            """);

        var json = """
            {
              "version": "7.3",
              "screens": [{
                "id": "SCREEN1",
                "terminal": false,
                "layout": {
                  "type": "SingleColumnLayout",
                  "children": [{
                    "type": "Footer",
                    "label": "Next",
                    "on-click-action": { "name": "navigate", "next": { "name": "GHOST" }, "payload": {} }
                  }]
                }
              }]
            }
            """;

        var violations = engine.Validate(json);
        Assert.Single(violations);
        Assert.Equal("INVALID_NAVIGATE_TARGET", violations[0].ErrorCode);
    }

    [Fact]
    public void Validate_NoSelfNavigation_Pass()
    {
        var engine = JsonRulesEngine.Load("""
            {
              "rules": [{
                "id": "no_self_navigation",
                "context": ".screens[] | . as $s | .layout.children[] | select(.[\"on-click-action\"]?.name == \"navigate\" and .[\"on-click-action\"].next.name == $s.id)",
                "test": "false",
                "message": "Cannot navigate to self.",
                "errorCode": "NO_SELF_NAVIGATION"
              }]
            }
            """);

        // Navigates to a DIFFERENT screen — should pass
        var json = """
            {
              "version": "7.3",
              "screens": [
                {
                  "id": "A",
                  "terminal": false,
                  "layout": {
                    "type": "SingleColumnLayout",
                    "children": [{
                      "type": "Footer", "label": "Go",
                      "on-click-action": { "name": "navigate", "next": { "name": "B" }, "payload": {} }
                    }]
                  }
                },
                { "id": "B", "terminal": true, "layout": { "type": "SingleColumnLayout", "children": [] } }
              ]
            }
            """;

        Assert.Empty(engine.Validate(json));
    }

    [Fact]
    public void Validate_NoSelfNavigation_Fail()
    {
        var engine = JsonRulesEngine.Load("""
            {
              "rules": [{
                "id": "no_self_navigation",
                "context": ".screens[] | . as $s | .layout.children[] | select(.[\"on-click-action\"]?.name == \"navigate\" and .[\"on-click-action\"].next.name == $s.id)",
                "test": "false",
                "message": "Cannot navigate to self.",
                "errorCode": "NO_SELF_NAVIGATION"
              }]
            }
            """);

        var json = """
            {
              "version": "7.3",
              "screens": [{
                "id": "LOOP",
                "terminal": false,
                "layout": {
                  "type": "SingleColumnLayout",
                  "children": [{
                    "type": "Footer", "label": "Again",
                    "on-click-action": { "name": "navigate", "next": { "name": "LOOP" }, "payload": {} }
                  }]
                }
              }]
            }
            """;

        var violations = engine.Validate(json);
        Assert.Single(violations);
        Assert.Equal("NO_SELF_NAVIGATION", violations[0].ErrorCode);
    }

    [Fact]
    public void Validate_TerminalHasFooter_Pass()
    {
        var engine = JsonRulesEngine.Load("""
            {
              "rules": [{
                "id": "terminal_has_footer",
                "context": ".screens[] | select(.terminal == true)",
                "test": "[.. | objects | select(.type == \"Footer\")] | length > 0",
                "message": "Terminal screens must have a Footer.",
                "errorCode": "MISSING_FOOTER"
              }]
            }
            """);

        Assert.Empty(engine.Validate(MinimalFlow(terminal: true)));
    }

    [Fact]
    public void Validate_TerminalHasFooter_Fail()
    {
        var engine = JsonRulesEngine.Load("""
            {
              "rules": [{
                "id": "terminal_has_footer",
                "context": ".screens[] | select(.terminal == true)",
                "test": "[.. | objects | select(.type == \"Footer\")] | length > 0",
                "message": "Terminal screens must have a Footer.",
                "errorCode": "MISSING_FOOTER"
              }]
            }
            """);

        var json = """
            {
              "version": "7.3",
              "screens": [{
                "id": "DONE",
                "terminal": true,
                "layout": {
                  "type": "SingleColumnLayout",
                  "children": [{ "type": "TextBody", "text": "Done" }]
                }
              }]
            }
            """;

        var violations = engine.Validate(json);
        Assert.Single(violations);
        Assert.Equal("MISSING_FOOTER", violations[0].ErrorCode);
    }

    [Fact]
    public void Validate_InlineDefFunction_Works()
    {
        // Verifies that a JQ `def` inlined in a test expression executes correctly
        var engine = JsonRulesEngine.Load("""
            {
              "rules": [{
                "id": "screen_count_check",
                "context": ".",
                "test": "def count_screens: .screens | length; count_screens >= 1",
                "message": "Must have at least one screen.",
                "errorCode": "NO_SCREENS"
              }]
            }
            """);

        Assert.Empty(engine.Validate(MinimalFlow()));
    }

    [Fact]
    public void Validate_RootAccessibleInTest_Works()
    {
        // Verifies $root is accessible when context is a sub-node
        var engine = JsonRulesEngine.Load("""
            {
              "rules": [{
                "id": "version_check",
                "context": ".screens[]",
                "test": "$root.version == \"7.3\"",
                "message": "All screens must belong to a v7.3 flow.",
                "errorCode": "WRONG_VERSION"
              }]
            }
            """);

        Assert.Empty(engine.Validate(MinimalFlow()));
    }

    [Fact]
    public void Load_BadJqFilter_Throws()
    {
        // JQSharp compiles filters at load time — invalid JQ throws immediately.
        Assert.ThrowsAny<Exception>(() => JsonRulesEngine.Load("""
            {
              "rules": [{
                "id": "broken_rule",
                "context": ".",
                "test": "this is not valid jq @@@@",
                "message": "This rule has a broken filter.",
                "errorCode": "BROKEN"
              }]
            }
            """));
    }

    [Fact]
    public void Validate_JsonElement_WorksLikeString()
    {
        var engine = JsonRulesEngine.Load("""
            {
              "rules": [{
                "id": "unique_screen_ids",
                "context": ".screens",
                "test": "map(.id) | length == (unique | length)",
                "message": "Screen IDs must be unique.",
                "errorCode": "DUPLICATE_SCREEN_ID"
              }]
            }
            """);

        var element = JsonDocument.Parse(MinimalFlow()).RootElement;
        Assert.Empty(engine.Validate(element));
    }
}
