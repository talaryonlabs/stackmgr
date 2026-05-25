using Talaryon.Toolbox.Services.ArgoCD.Models;
using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Types;

public class Stack
{
    public const string FileName = ".stack.yaml";
    
    [YamlIgnore] public FileInfo LocalFile => new(Path.Combine(LocalDirectory.FullName, FileName));
    [YamlIgnore] public DirectoryInfo LocalDirectory => new (Path.Combine(Environment.LocalDirectory.FullName, Name));
    [YamlIgnore] public StackEnvironment Environment { get; set; }
    [YamlIgnore] public V1alpha1Application? Application { get; set; }
    
    [YamlMember(Alias = "isDeleted")] public bool IsDeleted { get; set; }
    [YamlMember(Alias = "name")] public required string Name { get; set; }
    [YamlMember(Alias = "version")] public string? Version { get; set; } = "stack.talaryon.io/v1beta";
    [YamlMember(Alias = "namespace")] public string? Namespace { get; set; }
    [YamlMember(Alias = "enableAutoSync")] public bool EnableAutoSync { get; set; }
    [YamlMember(Alias = "images")] public List<StackImage> Images { get; init; } = [];
    [YamlMember(Alias = "apps")] public List<StackApp> Apps { get; init; } = [];
    [YamlMember(Alias = "ingresses")] public List<StackIngress> Ingresses { get; init; } = [];
    [YamlMember(Alias = "volumes")] public List<StackVolume> Volumes { get; init; } = [];
}