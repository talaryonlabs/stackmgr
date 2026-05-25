using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Models;

public class StackApp : IStackObject
{
    [YamlIgnore] public required Stack Stack { get; set; }
    [YamlIgnore]
    public DirectoryInfo LocalDirectory => new(
        Path.Combine(Stack.LocalDirectory.FullName, Name)
    );

    [YamlMember(Alias = "name")] public required string Name { get; set; }
    [YamlMember(Alias = "images")] public Dictionary<string, string> Images { get; init; } = [];
    [YamlMember(Alias = "volumes")] public Dictionary<string, string>  Volumes { get; init; } = [];
    [YamlMember(Alias = "requirements")] public Dictionary<string, string> Requirements { get; init; } = [];
    [YamlMember(Alias = "params")] public Dictionary<string, string> Params { get; init; } = [];
    [YamlMember(Alias = "template")] public StackAppTemplate? Template { get; set; }
}

public class StackAppTemplate
{
    [YamlMember(Alias = "name")] public required string Name { get; init; }
    [YamlMember(Alias = "branch")] public required string Branch { get; init; }
}