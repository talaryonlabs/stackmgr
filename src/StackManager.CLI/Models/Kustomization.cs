using Talaryon.StackManager.Serialization;
using Talaryon.StackManager.Types;
using YamlDotNet.Core;
using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Models;

public class Kustomization
{
    public const string FileName = "kustomization.yaml";
    
    [YamlMember(Alias = "apiVersion")] public string ApiVersion { get; set; } = "kustomize.config.k8s.io/v1beta1";
    [YamlMember(Alias = "images")] public List<KustomizationImage>? Images { get; set; }
    [YamlMember(Alias = "resources")] public List<string>? Resources { get; set;}
    [YamlMember(Alias = "namespace")] public string? Namespace { get; set; }
    
    public void Save(Stack stack)
    {
        var file = Path.Combine(stack.LocalDirectory.FullName, FileName);
        File.WriteAllText(file, YamlSerializer.Serialize(this));
    }
}

public class KustomizationImage
{
    public static implicit operator KustomizationImage(StackImage image) =>
        new()
        {
            Name = image.Name,
            NewName = image.Image.Contains(':') ? image.Image.Split(":")[0] : image.Image,
            NewTag = image.Image.Contains(':') ? image.Image.Split(":")[1] : "latest"
        };

    [YamlMember(Alias = "name")] public string? Name { get; set; }
    [YamlMember(Alias = "newName")] public string? NewName { get; set; }
    [YamlMember(Alias = "newTag", ScalarStyle = ScalarStyle.DoubleQuoted)] public string? NewTag { get; set; }
}