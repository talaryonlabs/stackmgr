using Talaryon.StackManager.Exceptions;
using Talaryon.Toolbox.Services.ArgoCD.Models;
using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Types;

public interface IStackObject
{
    Stack Stack { get; }
}

public class Stack
{
    public const string FileName = ".stack.yaml";
    
    public static Stack Load(StackEnvironment env, string name)
    {
        var file = Path.Combine(env.LocalDirectory.FullName, name, FileName);
        
        if (!File.Exists(file)) throw new StackNotFoundException(name);
        var stack = new Deserializer().Deserialize<Stack>(File.ReadAllText(file));

        stack.Environment = env;
        stack.ApplyParent();

        return stack;
    }

    public static Stack Create(StackEnvironment env, string name)
    {
        var stack = new Stack
        {
            Name = name,
            Environment = env,
            Namespace = $"{env.Name.ToLower()}-{name.ToLower()}",
            Images = [],
            Apps = [],
            Ingresses = [],
            Redirects = [],
            Volumes = [],
        };

        if (stack.LocalFile.Exists)
            throw new StackAlreadyExistsException(stack);
        
        if(!stack.LocalDirectory.Exists)
            stack.LocalDirectory.Create();
        
        stack.SaveConfig();
        stack.Build();
        
        return stack;
    }

    public Task Build()
    {
        return new StackBuilder(this).Build();
    }
    
    public void SaveConfig()
    {
        var file = Path.Combine(LocalDirectory.FullName, FileName);
        File.WriteAllText(file, new Serializer().Serialize(this));
    }

    private void ApplyParent()
    {
        lock (Ingresses)
        {
            Ingresses.ForEach(v => v.Stack = this);
        }
        
        lock(Apps)
        {
            Apps.ForEach(v => v.Stack = this);
        }

        lock (Images)
        {
            Images.ForEach(v => v.Stack = this);
        }

        lock (Redirects)
        {
            Redirects.ForEach(v => v.Stack = this);
        }
        
        lock(Volumes)
        {
            Volumes.ForEach(v => v.Stack = this);
        }
    }
    
    [YamlIgnore] public FileInfo LocalFile => new(Path.Combine(LocalDirectory.FullName, FileName));
    [YamlIgnore] public DirectoryInfo LocalDirectory => new (Path.Combine(Environment.LocalDirectory.FullName, Name));
    [YamlIgnore] public required StackEnvironment Environment { get; set; }
    [YamlIgnore] public V1alpha1Application? Application { get; set; }
    
    [YamlMember(Alias = "name")] public required string Name { get; set; }
    [YamlMember(Alias = "namespace")] public required string Namespace { get; set; }
    [YamlMember(Alias = "enableAutoSync")] public bool EnableAutoSync { get; set; }
    [YamlMember(Alias = "images")] public List<StackImage> Images { get; init; } = [];
    [YamlMember(Alias = "apps")] public List<StackApp> Apps { get; init; } = [];
    [YamlMember(Alias = "ingresses")] public List<StackIngress> Ingresses { get; init; } = [];
    [YamlMember(Alias = "redirects")] public List<StackRedirect> Redirects { get; init; } = [];
    [YamlMember(Alias = "volumes")] public List<StackVolume> Volumes { get; init; } = [];
}