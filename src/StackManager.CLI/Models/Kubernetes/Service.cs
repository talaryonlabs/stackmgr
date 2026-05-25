using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Models.Kubernetes;

public class Service
{
    [YamlMember(Alias = "apiVersion")] public string ApiVersion { get; set; } = "v1";
    [YamlMember(Alias = "kind")] public string Kind { get; set; } = "Service";
    [YamlMember(Alias = "metadata")] public ServiceMetadata Metadata { get; set; } = new();
    [YamlMember(Alias = "spec")] public ServiceSpec Spec { get; set; } = new();
}

public class ServiceMetadata
{
    [YamlMember(Alias = "name")] public string? Name { get; set; }
}

public class ServiceSpec
{
    [YamlMember(Alias = "type")] public string Type { get; set; } = "ClusterIP";
    [YamlMember(Alias = "externalName")] public string? ExternalName { get; set; }
}