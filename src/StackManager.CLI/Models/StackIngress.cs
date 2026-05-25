using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Models;

public class StackIngress : IStackObject
{
    [YamlIgnore] public required Stack Stack { get; set; }
    
    [YamlMember(Alias = "name")] public required string Name { get; set; }
    [YamlMember(Alias = "isSecured")] public bool IsSecured { get; set; }
    [YamlMember(Alias = "hostname")] public required string Hostname { get; set; }
    [YamlMember(Alias = "app")] public string? Application { get; set; }
    [YamlMember(Alias = "port")] public int Port { get; set; }
    [YamlMember(Alias = "annotations")] public Dictionary<string, string>? Annotations { get; init; } = [];
}