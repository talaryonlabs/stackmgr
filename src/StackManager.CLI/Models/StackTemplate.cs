using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Models;

public class StackTemplate : IApiVersionItem
{
    public const string FileName = ".app.yaml";
    public const string DirectoryName = ".apps";

    [YamlIgnore] public FileInfo LocalFile => new(Path.Combine(LocalDirectory.FullName, FileName));
    [YamlIgnore] public DirectoryInfo LocalDirectory => new (Path.Combine(DirectoryName, Name));
    
    [YamlMember(Alias = "name")] public required string Name { get; init; }
    [YamlMember(Alias = "version")] public string? Version { get; set; } = "template.talaryon.io/v1beta";
    [YamlMember(Alias = "port")] public short Port { get; init; }
    [YamlMember(Alias = "requirements")] public List<string> Requirements { get; init; } = [];
    [YamlMember(Alias = "volumes")] public List<string> Volumes { get; init; } = [];
    [YamlMember(Alias = "images")] public List<string> Images { get; init; } = [];
    [YamlMember(Alias = "params")] public List<string> Params { get; init; } = [];
    [YamlMember(Alias = "secrets")] public List<string> Secrets { get; init; } = [];
}