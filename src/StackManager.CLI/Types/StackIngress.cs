using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Types;

public class StackIngress
{
    [YamlMember(Alias = "isSecured")] public bool IsSecured { get; set; }
    [YamlMember(Alias = "host")] public required string Host { get; init; }
    [YamlMember(Alias = "service")] public required string Service { get; init; }
    [YamlMember(Alias = "port")] public required short Port { get; init; }
    [YamlMember(Alias = "redirectTo")] public string? RedirectTo { get; init; }
    [YamlMember(Alias = "annotations")] public Dictionary<string, string>? Annotations { get; set; } = [];
}