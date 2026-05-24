using Talaryon.StackManager.Exceptions;
using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Types;

public class StackTemplate
{
    public static DirectoryInfo AppDirectory => new(Path.Combine(Environment.CurrentDirectory, ".apps"));
    
    public const string FileName = ".app.yaml";
    public DirectoryInfo LocalDirectory => new(Path.Combine(AppDirectory.FullName, Name));
    
    public static StackTemplate Load(string name)
    {
        var path = Path.Combine(AppDirectory.FullName, name, FileName);
        var file = new FileInfo(path);
        
        return !file.Exists ? throw new TemplateNotFoundException(name) : StackConfig.Load<StackTemplate>(file);
    }

    [YamlMember(Alias = "name")] public required string Name { get; init; }
    [YamlMember(Alias = "version")] public string? Version { get; init; } = "template.talaryon.io/v1beta";
    [YamlMember(Alias = "port")] public short Port { get; init; }
    [YamlMember(Alias = "requirements")] public List<string> Requirements { get; init; } = [];
    [YamlMember(Alias = "volumes")] public List<string> Volumes { get; init; } = [];
    [YamlMember(Alias = "images")] public List<string> Images { get; init; } = [];
    [YamlMember(Alias = "params")] public List<string> Params { get; init; } = [];
    [YamlMember(Alias = "secrets")] public List<string> Secrets { get; init; } = [];
}