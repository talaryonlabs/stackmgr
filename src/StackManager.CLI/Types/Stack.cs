using Talaryon.StackManager.Exceptions;
using Talaryon.Toolbox.Services.ArgoCD.Models;
using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Types;

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
            Images = [new() { Name = "nginx", Image = "docker.io/library/nginx:latest" }],
            Apps = [new() { Name = "web", Template = "", Config = [] }],
            Ingresses = []
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
    [YamlIgnore] public required StackEnvironment Environment { get; set; }
    [YamlIgnore] public V1alpha1Application? Application { get; set; }
    
    [YamlMember(Alias = "name")] public required string Name { get; set; }
    [YamlMember(Alias = "namespace")] public required string Namespace { get; set; }
    [YamlMember(Alias = "enableAutoSync")] public bool EnableAutoSync { get; set; }
    [YamlMember(Alias = "images")] public List<StackImage> Images { get; init; } = [];
    [YamlMember(Alias = "apps")] public List<StackApp> Apps { get; init; } = [];
    [YamlMember(Alias = "ingresses")] public List<StackIngress> Ingresses { get; set; } = [];
}