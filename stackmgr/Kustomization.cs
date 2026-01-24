using stackmgr.Options;
using YamlDotNet.Serialization;

namespace stackmgr;

public class Kustomization
{
    public const string FileName = "kustomization.yaml";
    
    [YamlMember(Alias = "apiVersion")] public string ApiVersion { get; set; } = "kustomize.config.k8s.io/v1beta1";
    [YamlMember(Alias = "images")] public List<KustomizationImage>? Images { get; set; }
    [YamlMember(Alias = "resources")] public List<string>? Resources { get; set;}
    
    public void Save(StackEnvironment env, string stack)
    {
        var path = env.GetStackPath(stack);
        var file = Path.Combine(path, FileName);
        File.WriteAllText(file, new Serializer().Serialize(this));
    }
}

public class KustomizationImage
{
    public static implicit operator KustomizationImage(StackConfigImage image) =>
        new()
        {
            Name = image.Name,
            NewName = image.Image,
            NewTag = image.Tag
        };

    [YamlMember(Alias = "name")] public string? Name { get; set; }
    [YamlMember(Alias = "newName")] public string? NewName { get; set; }
    [YamlMember(Alias = "newTag")] public string? NewTag { get; set; }
}