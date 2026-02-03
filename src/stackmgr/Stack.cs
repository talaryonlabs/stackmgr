using stackmgr.Exceptions;
using Talaryon.Toolbox.Services.ArgoCD.Models;
using YamlDotNet.Serialization;

namespace stackmgr;

public class Stack : IStackManagerEntity
{
    private const string FileName = ".stack.yaml";
    
    public static Stack Load(StackEnvironment env, string name)
    {
        var file = Path.Combine(env.LocalDirectory.FullName, name, FileName);
        
        if (!File.Exists(file)) throw new StackNotFoundException(name);
        var stack = new Deserializer().Deserialize<Stack>(File.ReadAllText(file));

        stack.Environment = env;

        return stack;
    }

    public static Stack New(StackEnvironment env, string name)
    {
        var stack = new Stack
        {
            Name = name,
            Environment = env,
            Namespace = $"{env.Name.ToLower()}-{name.ToLower()}",
            Images = [new() { Name = "nginx", Image = "docker.io/library/nginx", Tag = "latest" }],
            Apps = [new() { Name = "web", Template = "", Config = [] }]
        };
        return stack;
    }
    
    public void Save()
    {
        var file = Path.Combine(LocalDirectory.FullName, FileName);
        File.WriteAllText(file, new Serializer().Serialize(this));
    }
    
    [YamlIgnore] public DirectoryInfo LocalDirectory => new (Path.Combine(Environment.LocalDirectory.FullName, Name));
    [YamlIgnore] public StackEnvironment Environment { get; set; }
    [YamlIgnore] public V1alpha1Application? Application { get; set; }
    
    [YamlMember(Alias = "name")] public string Name { get; set; }
    [YamlMember(Alias = "namespace")] public string Namespace { get; set; }
    [YamlMember(Alias = "images")] public List<StackImage>? Images { get; set; } = [];
    [YamlMember(Alias = "apps")] public List<StackApp>? Apps { get; set; } = [];
}

public class StackImage
{
    [YamlMember(Alias = "name")] public string? Name { get; set; }
    [YamlMember(Alias = "tag")] public string? Tag { get; set; }
    [YamlMember(Alias = "image")] public string? Image { get; set; }
}

public class StackApp
{
    [YamlMember(Alias = "name")] public required string Name { get; init; }
    [YamlMember(Alias = "template")] public string Template { get; set; } = "";
    [YamlMember(Alias = "config")] public List<StackAppConfig>? Config { get; set; } = [];
}

public class StackAppConfig
{
    [YamlMember(Alias = "name")] public string? Name { get; set; }
    [YamlMember(Alias = "value")] public string? Template { get; set; }
}