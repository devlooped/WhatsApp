using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Devlooped.WhatsApp;

public class YamlDictionaryConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => typeof(Dictionary<string, object?>).IsAssignableFrom(type);

    public object? ReadYaml(IParser parser, Type type, ObjectDeserializer rootDeserializer) => throw new NotImplementedException();

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        if (value == null)
            return;

        var dictionary = (Dictionary<string, object?>)value;
        emitter.Emit(new MappingStart());

        foreach (var kvp in dictionary)
        {
            if (kvp.Value == null)
                continue;

            if (kvp.Value is string str)
            {
                // Don't emit empty strings, for conciseness.
                if (str.Length == 0)
                    continue;

                emitter.Emit(new Scalar(kvp.Key));
                if (str.Contains('\n'))
                    emitter.Emit(new Scalar(null, null, str, ScalarStyle.Literal, true, false));
                else
                    emitter.Emit(new Scalar(str));
            }
            else
            {
                emitter.Emit(new Scalar(kvp.Key));
                serializer.Invoke(kvp.Value);
            }
        }

        emitter.Emit(new MappingEnd());
    }
}