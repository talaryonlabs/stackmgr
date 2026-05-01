using Talaryon.StackManager.Exceptions;
using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Types;

public class StackEnvironment
{
    public const string FileName = ".env.yaml";
    
    public static StackEnvironment Load(string name)
    {
        var file = Path.Combine(Environment.CurrentDirectory, name, FileName);
        
        if (!File.Exists(file)) throw new EnvironmentNotFoundException(name);
        var env = new Deserializer().Deserialize<StackEnvironment>(File.ReadAllText(file));
        
        return env;
    }

    public static StackEnvironment Create(string name)
    {
        var env = new StackEnvironment
        {
            Name = name,
            Vault = "",
            Outpost = "",
            CertIssuer = "",
            RegistryCredentials = "",
            Repository = "",
            Remote = ""
        };
        
        if (env.LocalFile.Exists)
        {
            throw new EnvironmentAlreadyExistsException(env);
        }

        if (!env.LocalDirectory.Exists)
        {
            env.LocalDirectory.Create();
        }
        
        env.SaveConfig();
        
        return env;
    }
    
    public void SaveConfig()
    {
        var file = Path.Combine(LocalDirectory.FullName, FileName);
        File.WriteAllText(file, new Serializer().Serialize(this));
    }
    
    [YamlIgnore] public FileInfo LocalFile => new(Path.Combine(LocalDirectory.FullName, FileName));
    [YamlIgnore] public DirectoryInfo LocalDirectory => new(Path.Combine(Environment.CurrentDirectory, Name));

    [YamlMember(Alias = "isDeleted")] public bool IsDeleted { get; set; }
    [YamlMember(Alias = "name")] public required string Name { get; init; }
    [YamlMember(Alias = "version")] public string? Version { get; set; }
    [YamlMember(Alias = "vault")] public required string Vault { get; set; }
    [YamlMember(Alias = "outpost")] public required string Outpost { get; set; }
    [YamlMember(Alias = "certIssuer")] public required string CertIssuer { get; set; }
    [YamlMember(Alias = "registryCredentials")] public required string RegistryCredentials { get; set; }
    [YamlMember(Alias = "repository")] public string? Repository { get; set; }
    [YamlMember(Alias = "remote")] public required string Remote { get; set; }
    
}