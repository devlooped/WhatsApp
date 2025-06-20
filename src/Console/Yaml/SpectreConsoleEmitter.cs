using System.Globalization;
using YamlDotNet.Core;
using YamlDotNet.Core.Events;

namespace Devlooped.WhatsApp;

/// <summary>
/// An IEmitter that formats YAML output for Spectre.Console consumption.
/// </summary>
class SpectreConsoleEmitter : IEmitter
{
    readonly IEmitter inner;
    // Tracks whether the next scalar is a mapping key (true) or value (false)
    int mappingDepth = 0;
    bool expectingKey = false;

    public SpectreConsoleEmitter(IEmitter inner) => this.inner = inner;

    public void Emit(ParsingEvent @event)
    {
        if (@event is MappingStart)
        {
            mappingDepth++;
            expectingKey = true;
            inner.Emit(@event);
        }
        else if (@event is MappingEnd)
        {
            mappingDepth--;
            expectingKey = mappingDepth > 0;
            inner.Emit(@event);
        }
        else if (@event is Scalar scalar)
        {
            if (mappingDepth > 0 && expectingKey)
            {
                // Format as Spectre grey key
                var key = scalar.Value ?? string.Empty;
                inner.Emit(new Scalar(null, null, $"[grey]{key}[/]", ScalarStyle.ForcePlain, true, false));
                expectingKey = false;
            }
            else if (mappingDepth > 0)
            {
                // Format value
                var value = scalar.Value;
                var style = DetectStyle(value);
                inner.Emit(new Scalar(null, null, style, ScalarStyle.ForcePlain, true, false));
                expectingKey = true;
            }
            else
            {
                // Not in mapping, just emit as-is
                inner.Emit(@event);
            }
        }
        else
        {
            inner.Emit(@event);
        }
    }

    static string DetectStyle(string? value)
    {
        if (value == null)
            return string.Empty;
        if (bool.TryParse(value, out var b))
            return $"[green]{value.ToLowerInvariant()}[/]";
        if (double.TryParse(value, NumberStyles.Any, CultureInfo.InvariantCulture, out _))
            return $"[blue]{value}[/]";
        return $"[red]{value}[/]";
    }
}
