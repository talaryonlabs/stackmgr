using YamlDotNet.Serialization;

namespace Talaryon.StackManager;

public static class StackResource
{
    public static T Load<T>(FileInfo file)
    {
        if(!file.Exists) throw new FileNotFoundException(file.FullName);
        
        var deserializer = new DeserializerBuilder()
            .IgnoreUnmatchedProperties()
            .Build();

        using var stream = file.OpenText();
        return deserializer.Deserialize<T>(stream);
    }
    
    public static void Save<T>(T resource, FileInfo file)
    {
        var serializer = new SerializerBuilder()
            .Build();
        
        using var stream = file.OpenWrite();
        var writer = new StreamWriter(stream);
        serializer.Serialize(writer, resource);
    }
    
}