using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Types;

public class StackVolume
{
    public static StackVolume Create(Stack stack, string name, string storageSize, string accessMode = "ReadWriteOnce", int replicas = 0)
    {
        if (stack.Volumes.Any(x => x.Name.Equals(name, StringComparison.CurrentCultureIgnoreCase)))
        {
            throw new Exception($"Volume with name '{name}' already exists in stack '{stack.Name}'.");
        }

        var volume = new StackVolume
        {
            Stack = stack,
            Name = name,
            StorageSize = storageSize,
            AccessMode = accessMode,
            Replicas = replicas
        };

        lock (stack.Volumes)
        {
            stack.Volumes.Add(volume);
        }
        stack.SaveConfig();

        return volume;
    }

    public void Delete()
    {
        lock (Stack.Volumes)
        {
            Stack.Volumes.Remove(this);
        }
        Stack.SaveConfig();
    }
    
    [YamlIgnore] public required Stack Stack { get; set; }
    
    [YamlMember(Alias = "name")] public required string Name { get; set; }
    [YamlMember(Alias = "storageSize")] public required string StorageSize { get; set; }
    [YamlMember(Alias = "accessMode")] public required string AccessMode { get; set; }
    [YamlMember(Alias = "replicas")] public int Replicas { get; set; }
}