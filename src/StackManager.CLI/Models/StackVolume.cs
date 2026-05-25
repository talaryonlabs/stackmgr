using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Models;

public class StackVolume : IStackObject
{
    [YamlIgnore] public required Stack Stack { get; set; }
    
    [YamlMember(Alias = "name")] public required string Name { get; set; }
    [YamlMember(Alias = "storageSize")] public required string StorageSize { get; set; }
    [YamlMember(Alias = "accessMode")] public required string AccessMode { get; set; }
    [YamlMember(Alias = "replicas")] public int Replicas { get; set; }
}