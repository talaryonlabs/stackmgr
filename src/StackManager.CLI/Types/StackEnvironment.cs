using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Types;

public class StackEnvironment
{
    public const string FileName = ".env.yaml";
    
    [YamlIgnore] public FileInfo LocalFile => new(Path.Combine(LocalDirectory.FullName, FileName));
    [YamlIgnore] public DirectoryInfo LocalDirectory => new(Path.Combine(Environment.CurrentDirectory, Name ?? "default"));

    [YamlMember(Alias = "isDeleted")] public bool IsDeleted { get; set; }
    [YamlMember(Alias = "name")] public string? Name { get; set; }
    [YamlMember(Alias = "version")] public string? Version { get; init; } = "environment.talaryon.io/v1beta";
    [YamlMember(Alias = "vault")] public string? Vault { get; set; }
    [YamlMember(Alias = "outpost")] public string? Outpost { get; set; }
    [YamlMember(Alias = "certIssuer")] public string? CertIssuer { get; set; }
    [YamlMember(Alias = "registryCredentials")] public string? RegistryCredentials { get; set; }
    [YamlMember(Alias = "repository")] public string? Repository { get; set; }
    [YamlMember(Alias = "remote")] public string? Remote { get; set; }
}