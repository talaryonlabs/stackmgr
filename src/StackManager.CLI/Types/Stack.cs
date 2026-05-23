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
        var path = Path.Combine(env.LocalDirectory.FullName, name, FileName); 
        var file = new FileInfo(path);
        
        if (!file.Exists) throw new StackNotFoundException(name);

        var stack = StackResource.Load<Stack>(file);

        stack.Environment = env;
        stack.ApplyParent();

        return stack;
    }

    public static async Task<Stack> CreateAsync(StackEnvironment env, string name)
    {
        var stack = new Stack
        {
            Name = name,
            Environment = env,
            Namespace = $"{env.Name.ToLower()}-{name.ToLower().Replace(".", "-")}",
            Images = [],
            Apps = [],
            Ingresses = [],
            Volumes = [],
        };

        if (stack.LocalFile.Exists)
            throw new StackAlreadyExistsException(stack);
        
        if(!stack.LocalDirectory.Exists)
            stack.LocalDirectory.Create();
        
        stack.SaveConfig();
        await stack.BuildAsync();
        
        return stack;
    }

    public static Stack Create(StackEnvironment env, string name)
    {
        return CreateAsync(env, name).GetAwaiter().GetResult();
    }

    public async Task BuildAsync()
    {
        await new StackBuilder(this).BuildAsync();
    }

    [Obsolete("Use BuildAsync instead")]
    public Task Build()
    {
        return BuildAsync();
    }

    public void Delete(bool complete = false)
    {
        if (!complete)
        {
            if(IsDeleted) throw new StackAlreadyDeletedException(Name);
            IsDeleted = true;
            SaveConfig();
            return;
        }
        LocalDirectory.Delete(true);
    }
    
    public void SaveConfig() => StackResource.Save(this, LocalFile);

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
        
        lock(Volumes)
        {
            Volumes.ForEach(v => v.Stack = this);
        }
    }
    
    [YamlIgnore] public FileInfo LocalFile => new(Path.Combine(LocalDirectory.FullName, FileName));
    [YamlIgnore] public DirectoryInfo LocalDirectory => new (Path.Combine(Environment.LocalDirectory.FullName, Name));
    [YamlIgnore] public required StackEnvironment Environment { get; set; }
    [YamlIgnore] public V1alpha1Application? Application { get; set; }
    
    [YamlMember(Alias = "isDeleted")] public bool IsDeleted { get; set; }
    [YamlMember(Alias = "name")] public required string Name { get; set; }
    [YamlMember(Alias = "version")] public string? Version { get; set; } = "stack.talaryon.io/v1beta";
    [YamlMember(Alias = "namespace")] public required string Namespace { get; set; }
    [YamlMember(Alias = "enableAutoSync")] public bool EnableAutoSync { get; set; }
    [YamlMember(Alias = "images")] public List<StackImage> Images { get; init; } = [];
    [YamlMember(Alias = "apps")] public List<StackApp> Apps { get; init; } = [];
    [YamlMember(Alias = "ingresses")] public List<StackIngress> Ingresses { get; init; } = [];
    [YamlMember(Alias = "volumes")] public List<StackVolume> Volumes { get; init; } = [];
}