using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Devlooped.WhatsApp;

public class YamlDateOnlyConverter : IYamlTypeConverter
{
    const string format = "yyyy-MM-dd";

    public bool Accepts(Type type) => type == typeof(DateOnly) || type == typeof(DateOnly?);

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer)
    {
        var scalar = parser.Consume<Scalar>();
        if (string.IsNullOrEmpty(scalar.Value))
        {
            return type == typeof(DateOnly?) ? null : default(DateOnly);
        }
        return DateOnly.ParseExact(scalar.Value, format);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        var dateOnly = value as DateOnly?;
        if (dateOnly.HasValue)
        {
            emitter.Emit(new Scalar(dateOnly.Value.ToString(format)));
        }
        else if (type == typeof(DateOnly?))
        {
            emitter.Emit(new Scalar("null"));
        }
    }
}