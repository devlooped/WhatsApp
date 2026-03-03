using System.Text.Json;
using Devlooped.WhatsApp.Flows;

namespace Devlooped.WhatsApp;

public class FlowJsonValidatorTests(ITestOutputHelper output)
{
    // VALID FLOW TESTS - one [Theory] with [MemberData] that discovers all .json files in Content/Flows/Valid/
    [Theory]
    [MemberData(nameof(GetValidFlows))]
    public void ValidFlow(string name)
    {
        var json = File.ReadAllText(Path.Combine("Content", "Flows", "Valid", name + ".json"));
        var result = FlowJsonValidator.Instance.Validate(json);

        if (!result.IsValid)
        {
            foreach (var error in result.Errors)
                output.WriteLine($"{error.Path}: {error.Message}");
        }

        Assert.True(result.IsValid, $"Flow '{name}' should be valid but had {result.Errors.Count} error(s)");
    }

    // INVALID FLOW TESTS - one [Theory] with [MemberData] that discovers all .json files in Content/Flows/Invalid/
    [Theory]
    [MemberData(nameof(GetInvalidFlows))]
    public void InvalidFlow(string name)
    {
        var json = File.ReadAllText(Path.Combine("Content", "Flows", "Invalid", name + ".json"));
        var result = FlowJsonValidator.Instance.Validate(json);

        Assert.False(result.IsValid, $"Flow '{name}' should be invalid but passed validation");
        Assert.NotEmpty(result.Errors);

        foreach (var error in result.Errors)
            output.WriteLine($"{error.Path}: {error.Message}");
    }

    [Fact]
    public void ValidateJsonElement()
    {
        var json = File.ReadAllText(Path.Combine("Content", "Flows", "Valid", "MinimalFlow.json"));
        var element = JsonSerializer.Deserialize<JsonElement>(json);
        var result = FlowJsonValidator.Instance.Validate(element);
        Assert.True(result.IsValid);
    }

    [Fact]
    public void ValidateEmptyObject()
    {
        var result = FlowJsonValidator.Instance.Validate("{}");
        Assert.False(result.IsValid);
    }

    [Fact]
    public void ValidateInvalidJson()
    {
        Assert.ThrowsAny<JsonException>(() =>
            FlowJsonValidator.Instance.Validate("not json"));
    }

    [Fact]
    public void SingletonInstanceIsSame()
    {
        Assert.Same(FlowJsonValidator.Instance, FlowJsonValidator.Instance);
    }

    public static TheoryData<string> GetValidFlows()
    {
        var data = new TheoryData<string>();
        var dir = Path.Combine("Content", "Flows", "Valid");
        if (Directory.Exists(dir))
        {
            foreach (var file in Directory.GetFiles(dir, "*.json"))
                data.Add(Path.GetFileNameWithoutExtension(file));
        }
        return data;
    }

    public static TheoryData<string> GetInvalidFlows()
    {
        var data = new TheoryData<string>();
        var dir = Path.Combine("Content", "Flows", "Invalid");
        if (Directory.Exists(dir))
        {
            foreach (var file in Directory.GetFiles(dir, "*.json"))
                data.Add(Path.GetFileNameWithoutExtension(file));
        }
        return data;
    }
}
