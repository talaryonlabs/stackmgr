using Talaryon.StackManager.Exceptions;
using Talaryon.StackManager.Serialization;

namespace Talaryon.StackManager;

public static class StackResource
{
    public static T Load<T>(FileInfo file)
    {
        if (!file.Exists)
        {
            if(typeof(T) == typeof(Stack))
            {
                throw new StackNotFoundException(file.Name);
            }
            
            if(typeof(T) == typeof(StackEnvironment))
            {
                throw new EnvironmentNotFoundException(file.Name);
            }

            if (typeof(T) == typeof(StackTemplate))
            {
                throw new TemplateNotFoundException(file.Name);
            }

            throw new FileNotFoundException(file.FullName);
        }
        
        using var stream = file.OpenText();
        return YamlSerializer.Deserialize<T>(stream);
    }
    
    public static void Save<T>(T resource, FileInfo file)
    {
        using var stream = file.OpenWrite();
        using var writer = new StreamWriter(stream);
        YamlSerializer.Serializer.Serialize(writer, resource);
    }
}