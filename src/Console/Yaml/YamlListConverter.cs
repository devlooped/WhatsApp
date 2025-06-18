using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Devlooped.WhatsApp;

public class YamlListConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => typeof(List<object?>).IsAssignableFrom(type);
    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer) => throw new NotImplementedException();
    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        if (value == null)
            return;

        var list = (List<object?>)value;
        emitter.Emit(new SequenceStart(null, null, false, SequenceStyle.Block));
        foreach (var item in list)
        {
            if (item == null)
                continue;

            if (item is string str)
            {
                if (str.Contains('\n'))
                    emitter.Emit(new Scalar(null, null, str, ScalarStyle.Literal, true, false));
                else
                    emitter.Emit(new Scalar(str));
            }
            else
            {
                serializer.Invoke(item);
            }
        }
        emitter.Emit(new SequenceEnd());
    }
}
