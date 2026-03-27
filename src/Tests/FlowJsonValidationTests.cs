using Devlooped.WhatsApp.Flows;

namespace Devlooped.WhatsApp.Tests;

/// <summary>
/// Data-driven validation tests using generated Flow JSON test data.
/// </summary>
public class FlowJsonValidationTests
{
    readonly FlowJsonValidator validator = new();

    [Theory]
    [MemberData(nameof(FlowJsonGenerator.ValidFlows), MemberType = typeof(FlowJsonGenerator))]
    public void ValidFlow_PassesValidation(string name, string json)
    {
        var result = validator.Validate(json);

        Assert.True(result.IsValid,
            $"Flow '{name}' should be valid but had {result.Errors.Count} error(s):\n" +
            string.Join("\n", result.Errors.Select(e => $"  [{e.Error}] {e.Message}")));
    }

    [Theory]
    [MemberData(nameof(FlowJsonGenerator.InvalidFlows), MemberType = typeof(FlowJsonGenerator))]
    public void InvalidFlow_FailsValidation(string name, string json, string expectedErrorCode)
    {
        var result = validator.Validate(json);

        Assert.False(result.IsValid,
            $"Flow '{name}' should be invalid but passed validation.");

        Assert.True(result.Errors.Any(e => e.Error == expectedErrorCode),
            $"Flow '{name}' should have error '{expectedErrorCode}' but had:\n" +
            string.Join("\n", result.Errors.Select(e => $"  [{e.Error}] {e.Message}")));
    }

    [Fact]
    public void NullJson_ThrowsArgumentException()
    {
        Assert.ThrowsAny<ArgumentException>(() => validator.Validate(null!));
    }

    [Fact]
    public void EmptyJson_ThrowsArgumentException()
    {
        Assert.Throws<ArgumentException>(() => validator.Validate(""));
    }

    [Fact]
    public void InvalidJson_ReturnsSyntaxError()
    {
        var result = validator.Validate("{ invalid json }");

        Assert.False(result.IsValid);
        Assert.Contains(result.Errors, e => e.Error == "INVALID_JSON");
    }

    [Fact]
    public void MinimalValidFlow_PassesValidation()
    {
        var json = """
        {
          "version": "7.3",
          "screens": [
            {
              "id": "MAIN",
              "terminal": true,
              "layout": {
                "type": "SingleColumnLayout",
                "children": [
                  { "type": "TextBody", "text": "Hello" },
                  {
                    "type": "Footer",
                    "label": "Done",
                    "on-click-action": {
                      "name": "complete",
                      "payload": {}
                    }
                  }
                ]
              }
            }
          ]
        }
        """;

        var result = validator.Validate(json);

        Assert.True(result.IsValid,
            "Minimal valid flow should pass:\n" +
            string.Join("\n", result.Errors.Select(e => $"  [{e.Error}] {e.Message}")));
    }
}
