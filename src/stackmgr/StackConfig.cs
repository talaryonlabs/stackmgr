using YamlDotNet.Serialization;

namespace stackmgr;

public class StackConfig
{
    public const string FileName = ".stack.yaml";
    
    public static void Generate(StackEnvironment env, string stack)
    {
        var config = new StackConfig
        {
            Images = [new() { Name = "nginx", Image = "docker.io/library/nginx", Tag = "latest" }],
            Apps = [new() { Name = "web", Template = "", Config = [] }]
        };
        var path = env.GetStackPath(stack);
        var file = Path.Combine(path, FileName);
        File.WriteAllText(file, new Serializer().Serialize(config));
    }
    
    public static StackConfig? Load(StackEnvironment env, string stack)
    {
        var path = env.GetStackPath(stack);
        var file = Path.Combine(path, FileName);
        if (!File.Exists(file)) return null;
        return new Deserializer().Deserialize<StackConfig>(File.ReadAllText(file));
    }
    
    [YamlMember(Alias = "autoSync")] public bool AutoSync { get; set; } = false;
    [YamlMember(Alias = "namespace")] public string? Namespace { get; set; }
    [YamlMember(Alias = "images")] public List<StackConfigImage>? Images { get; set; } = [];
    [YamlMember(Alias = "apps")] public List<StackConfigApp>? Apps { get; set; } = [];
}

public class StackConfigImage
{
    [YamlMember(Alias = "name")] public string? Name { get; set; }
    [YamlMember(Alias = "tag")] public string? Tag { get; set; }
    [YamlMember(Alias = "image")] public string? Image { get; set; }
}

public class StackConfigApp
{
    [YamlMember(Alias = "name")] public string? Name { get; set; }
    [YamlMember(Alias = "template")] public string? Template { get; set; }
    [YamlMember(Alias = "config")] public List<StackConfigAppConfig>? Config { get; set; } = [];
}

public class StackConfigAppConfig
{
    [YamlMember(Alias = "name")] public string? Name { get; set; }
    [YamlMember(Alias = "value")] public string? Template { get; set; }
}