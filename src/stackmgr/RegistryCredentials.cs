using YamlDotNet.Serialization;

namespace stackmgr;

public class RegistryCredentials
{
    [YamlMember(Alias = "apiVersion")] public string ApiVersion => "v1";
    [YamlMember(Alias = "kind")] public string Kind => "Secret";
    [YamlMember(Alias = "metadata")] public RegistryCredentialsMetadata Metadata { get; set; } = new();
    [YamlMember(Alias = "data")] public RegistryCredentialsData Data { get; set; } = new();
    [YamlMember(Alias = "type")] public string Type => "kubernetes.io/dockerconfigjson";
}

public class RegistryCredentialsMetadata
{
    [YamlMember(Alias = "name")] public string Name => "registry-credentials";
    [YamlMember(Alias = "annotations")] public RegistryCredentialsMetadataAnnotations Annotations { get; set; } = new();
}

public class RegistryCredentialsMetadataAnnotations
{
    [YamlMember(Alias = "avp.kubernetes.io/path")] public string? Path { get; set; }
}

public class RegistryCredentialsData
{
    [YamlMember(Alias = ".dockerconfigjson")] public string DockerConfigJson => "eyJhdXRocyI6eyJnaGNyLmlvIjp7InVzZXJuYW1lIjoiPHVzZXJuYW1lPiIsInBhc3N3b3JkIjoiPGFjY2Vzc190b2tlbj4ifX19";
}