using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Models;

public class Ingress
{
    [YamlMember(Alias = "apiVersion")] public string ApiVersion { get; set; } = "networking.k8s.io/v1";
    [YamlMember(Alias = "kind")] public string Kind { get; set; } = "Ingress";
    [YamlMember(Alias = "metadata")] public IngressMetadata Metadata { get; set; } = new();
    [YamlMember(Alias = "spec")] public IngressSpec Spec { get; set; } = new();

    public void SaveTo(string path)
    {
        File.WriteAllText(path, new Serializer().Serialize(this));
    }
}

public class IngressMetadata
{
    [YamlMember(Alias = "annotations")] public Dictionary<string, string> Annotations { get; set; } = [];
    [YamlMember(Alias = "name")] public string? Name { get; set; }
}

public class IngressSpec
{
    [YamlMember(Alias = "rules")] public List<IngressSpecRule> Rules { get; set; } = [];
    [YamlMember(Alias = "tls")] public List<IngressSpecTls> Tls { get; set; } = [];
}

public class IngressSpecTls
{
    [YamlMember(Alias = "hosts")] public List<string> Hosts { get; set; } = [];
    [YamlMember(Alias = "secretName")] public string? SecretName { get; set; }
}

public class IngressSpecRule
{
    [YamlMember(Alias = "host")] public string? Host { get; set; }
    [YamlMember(Alias = "http")] public IngressSpecRuleHttp Http { get; set; } = new();
    
}

public class IngressSpecRuleHttp
{
    [YamlMember(Alias = "paths")] public List<IngressSpecRulePath> Paths { get; set; } = [];
}

public class IngressSpecRulePath
{
    [YamlMember(Alias = "path")] public string Path { get; set; } = "/";
    [YamlMember(Alias = "pathType")] public string PathType { get; set; } = "Prefix";
    [YamlMember(Alias = "backend")] public IngressSpecRuleBackend Backend { get; set; } = new();
}

public class IngressSpecRuleBackend
{
    [YamlMember(Alias = "service")] public IngressSpecRuleService Service { get; set; } = new();
}

public class IngressSpecRuleService
{
    [YamlMember(Alias = "name")] public string? Name { get; set; }
    [YamlMember(Alias = "port")] public IngressSpecRulePort Port { get; set; } = new() { Number = 80 };
}

public class IngressSpecRulePort
{
    [YamlMember(Alias = "number")] public int Number { get; set; }
}

