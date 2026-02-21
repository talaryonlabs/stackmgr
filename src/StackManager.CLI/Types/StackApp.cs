using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Types;

public class StackApp
{
    [YamlMember(Alias = "name")] public required string Name { get; init; }
    [YamlMember(Alias = "volume")] public string Volume { get; init; } = "";
    [YamlMember(Alias = "template")] public string Template { get; init; } = "";
    [YamlMember(Alias = "host")] public string Host { get; init; } = "";
    [YamlMember(Alias = "port")] public short Port { get; init; }
    [YamlMember(Alias = "config")] public List<StackAppConfig> Config { get; init; } = [];
}

public class StackAppConfig
{
    [YamlMember(Alias = "name")] public required string Name { get; set; }
    [YamlMember(Alias = "value")] public required string Value { get; set; }
}