using YamlDotNet.Core;
using YamlDotNet.Core.Events;
using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Serialization;

/// <summary>
/// Type converter for StackAppTemplate that ensures it's always serialized as object format.
/// This provides backward compatibility: old YAML files with string format like "name@branch"
/// will be deserialized correctly (using the string constructor), and will be saved
/// in the new object format.
/// </summary>
public class StackAppTemplateTypeConverter : IYamlTypeConverter
{
    public bool Accepts(Type type) => type == typeof(StackAppTemplate);

    public object ReadYaml(IParser parser, Type type, ObjectDeserializer nestedDeserializer)
    {
        // Use the nested deserializer which will handle both:
        // - Scalar: calls the string constructor
        // - Mapping: calls the default constructor and sets properties
        return nestedDeserializer(type);
    }

    public void WriteYaml(IEmitter emitter, object? value, Type type, ObjectSerializer serializer)
    {
        if (value is StackAppTemplate template)
        {
            // Always serialize as object format for consistency
            // This ensures that even if loaded from string format,
            // it will be saved as object format
            emitter.Emit(new MappingStart());
            emitter.Emit(new Scalar("name"));
            emitter.Emit(new Scalar(template.Name));
            emitter.Emit(new Scalar("branch"));
            emitter.Emit(new Scalar(template.Branch));
            emitter.Emit(new MappingEnd());
        }
    }
}