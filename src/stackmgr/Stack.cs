using stackmgr.Exceptions;
using Talaryon.Toolbox.Services.ArgoCD.Models;
using YamlDotNet.Serialization;

namespace stackmgr;

public class Stack : IStackManagerEntity
{
    public const string FileName = ".stack.yaml";
    
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
            Vault = "",
            RegistryCredentials = "",
            Images = [new() { Name = "nginx", Image = "docker.io/library/nginx:latest" }],
            Apps = [new() { Name = "web", Template = "", Config = [] }]
        };
        return stack;
    }
    
    public void SaveConfig()
    {
        var file = Path.Combine(LocalDirectory.FullName, FileName);
        File.WriteAllText(file, new Serializer().Serialize(this));
    }

    public void SaveKustomization()
    {
        var kustomization = new Kustomization
        {
            Namespace = Namespace,
            Images = Images.Select(i => (KustomizationImage)i).ToList(),
            Resources = LocalDirectory
                .GetFiles("*.yaml", SearchOption.AllDirectories)
                .Where(f => !new List<string> { Kustomization.FileName, FileName }.Contains(f.Name))
                .Select(f => f.FullName.Replace(LocalDirectory.FullName, "").Replace("\\", "/")[1..])
                .ToList()
        };
            
        kustomization.Save(this);
    }
    
    [YamlIgnore] public DirectoryInfo LocalDirectory => new (Path.Combine(Environment.LocalDirectory.FullName, Name));
    [YamlIgnore] public StackEnvironment Environment { get; set; } = new();
    [YamlIgnore] public V1alpha1Application? Application { get; set; }
    
    [YamlMember(Alias = "name")] public required string Name { get; set; }
    [YamlMember(Alias = "namespace")] public required string Namespace { get; set; }
    [YamlMember(Alias = "vault")] public required string Vault { get; set; }
    [YamlMember(Alias = "registryCredentials")] public required string RegistryCredentials { get; set; }
    [YamlMember(Alias = "enableAutoSync")] public bool EnableAutoSync { get; set; }
    [YamlMember(Alias = "images")] public List<StackImage> Images { get; set; } = [];
    [YamlMember(Alias = "apps")] public List<StackApp> Apps { get; set; } = [];
}

public class StackImage
{
    [YamlMember(Alias = "name")] public required string Name { get; init; }
    [YamlMember(Alias = "image")] public required string Image { get; set; }
}

public class StackApp
{
    [YamlMember(Alias = "name")] public required string Name { get; init; }
    [YamlMember(Alias = "volume")] public string Volume { get; init; } = "";
    [YamlMember(Alias = "template")] public string Template { get; init; } = "";
    [YamlMember(Alias = "host")] public string Host { get; init; } = "";
    [YamlMember(Alias = "config")] public List<StackAppConfig> Config { get; init; } = [];
}

public class StackAppConfig
{
    [YamlMember(Alias = "name")] public required string Name { get; set; }
    [YamlMember(Alias = "value")] public required string Value { get; set; }
}