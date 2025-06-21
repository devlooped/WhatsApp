using Mono.Options;

namespace Devlooped.WhatsApp;

class ConsoleOption : OptionSet
{
    public ConsoleOption() =>
        Add("u|url", "WhatsApp functions endpoint", u => Endpoint = u)
       .Add("n|number=", "Your WhatsApp user phone number", n => Number = ParseNumber(n))
       .Add("j|json", "Format output as JSON", _ => Format = OutputFormat.Json)
       .Add("t|text", "Format output as text", _ => Format = OutputFormat.Text)
       .Add("y|yaml", "Format output as YAML", _ => Format = OutputFormat.Yaml);

    public string? Endpoint { get; private set; }

    public OutputFormat? Format { get; private set; }

    public int? Number { get; private set; }

    static int ParseNumber(string value) => int.Parse([.. value.Where(char.IsDigit)]);
}
