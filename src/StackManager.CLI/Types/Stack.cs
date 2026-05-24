using Talaryon.StackManager.Builder;
using Talaryon.StackManager.Exceptions;
using Talaryon.StackManager.Services;
using Talaryon.Toolbox.Services.ArgoCD.Models;
using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Types;

public interface IStackObject
{
    Stack Stack { get; set; }
    string Name { get; set; }
}

public class Stack
{
    public const string FileName = ".stack.yaml";

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

    public async Task BuildAsync(KustomizeService? kustomizeService = null)
    {
        var builder = new StackBuilder(this);
        if (kustomizeService != null)
        {
            builder.WithKustomizeValidation(kustomizeService);
        }
        await builder.BuildAsync();
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
    
    public void SaveConfig() => StackConfig.Save(this, LocalFile);
    
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