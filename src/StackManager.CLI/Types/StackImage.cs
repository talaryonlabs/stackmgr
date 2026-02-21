using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Types;

public class StackImage
{
    [YamlMember(Alias = "name")] public required string Name { get; init; }
    [YamlMember(Alias = "image")] public required string Image { get; set; }
}