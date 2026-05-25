using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Models;

public class StackImage : IStackObject
{
    [YamlIgnore] public required Stack Stack { get; set; }
    
    [YamlMember(Alias = "name")] public required string Name { get; set; }
    [YamlMember(Alias = "image")] public required string Image { get; set; }
}