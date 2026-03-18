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
        var file = Path.Combine(AppDirectory.FullName, name, FileName);
        
        if (!File.Exists(file)) throw new TemplateNotFoundException(name);
        var template = new Deserializer().Deserialize<StackTemplate>(File.ReadAllText(file));
        
        return template;
    }

    [YamlMember(Alias = "name")] public required string Name { get; init; }
    [YamlMember(Alias = "port")] public short Port { get; init; }
    [YamlMember(Alias = "requirements")] public List<string> Requirements { get; init; } = [];
    [YamlMember(Alias = "volumes")] public List<string> Volumes { get; init; } = [];
    [YamlMember(Alias = "images")] public List<string> Images { get; init; } = [];
    [YamlMember(Alias = "params")] public List<string> Params { get; init; } = [];
    [YamlMember(Alias = "secrets")] public List<string> Secrets { get; init; } = [];
}