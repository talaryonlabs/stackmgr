using System.Net.Mail;
using stackmgr.Exceptions;
using YamlDotNet.Serialization;

namespace stackmgr;

public class StackEnvironment : IStackManagerEntity
{
    public const string FileName = ".env.yaml";
    
    public static StackEnvironment Load(string name, bool includeDeleted = false)
    {
        var file = Path.Combine(Environment.CurrentDirectory, name, FileName);
        
        if (!File.Exists(file)) throw new EnvironmentNotFoundException(name);
        var env = new Deserializer().Deserialize<StackEnvironment>(File.ReadAllText(file));
        
        return !includeDeleted && env.IsDeleted ? throw new EnvironmentNotFoundException(name) : env;
    }

    public static StackEnvironment New(string name)
    {
        var env = new StackEnvironment
        {
            Name = name,
            Vault = "",
            Outpost = "",
            RegistryCredentials = "",
        };
        return env;
    }
    
    public void SaveConfig()
    {
        var file = Path.Combine(LocalDirectory.FullName, FileName);
        File.WriteAllText(file, new Serializer().Serialize(this));
    }
    
    [YamlIgnore] public DirectoryInfo LocalDirectory => new(Path.Combine(Environment.CurrentDirectory, Name));

    [YamlMember(Alias = "isDeleted")] public bool IsDeleted { get; set; }
    [YamlMember(Alias = "name")] public required string Name { get; init; }
    [YamlMember(Alias = "vault")] public required string Vault { get; set; }
    [YamlMember(Alias = "outpost")] public required string Outpost { get; set; }
    [YamlMember(Alias = "registryCredentials")] public required string RegistryCredentials { get; set; }
    [YamlMember(Alias = "rke2")] public StackEnvironmentRancher Rancher { get; set; } = new();
    [YamlMember(Alias = "argocd")] public StackEnvironmentArgo Argo { get; set; } = new();
}

public class StackEnvironmentRancher
{
    [YamlMember(Alias = "projectId")] public string ProjectId { get; set; } = "";
    [YamlMember(Alias = "url")] public string Url { get; set; } = "";
}

public class StackEnvironmentArgo
{
    [YamlMember(Alias = "url")] public string Url { get; set; } = "";
    [YamlMember(Alias = "project")] public string Project { get; set; } = "";
    [YamlMember(Alias = "repository")] public string Repository { get; set; } = "";
}