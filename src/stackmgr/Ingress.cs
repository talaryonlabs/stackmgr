using YamlDotNet.Serialization;

namespace stackmgr;

public class Ingress
{
    public static Ingress CreateAuthIngress(Stack stack, string host)
    {
        var name = host
            .Replace(".", "-")
            .ToLower();

        return new Ingress
        {
            Metadata = { Name = $"ingress-{name}-auth" },
            Spec =
            {
                Rules =
                [
                    new()
                    {
                        Host = host,
                        Http = new()
                        {
                            Paths =
                            [
                                new()
                                {
                                    Path = "/outpost.goauthentik.io",
                                    Backend = new()
                                    {
                                        Service = new()
                                        {
                                            Name = $"{stack}-auth",
                                            Port = new() { Number = 9000 }
                                        }
                                    }
                                }
                            ]
                        }

                    }
                ],
                Tls =
                [
                    new() { SecretName = $"letsencrypt-{name}", Hosts = [host] }
                ]
            }
        };
    }
    
    public static Ingress CreateSecuredIngress(string outpost, string host)
    {
        return new Ingress
        {
            Metadata =
            {
                Annotations = new Dictionary<string, string>
                {
                    { "nginx.ingress.kubernetes.io/auth-url", $"http://{outpost}:9000/outpost.goauthentik.io/auth/nginx" },
                    {
                        "nginx.ingress.kubernetes.io/auth-signin",
                        $"https://{host}/outpost.goauthentik.io/start?rd=$scheme://$http_host$escaped_request_uri"
                    },
                    {
                        "nginx.ingress.kubernetes.io/auth-response-headers",
                        "authorization,x-authentik-username,x-authentik-groups,x-authentik-email,x-authentik-name,x-authentik-uid"
                    },
                    {
                        "nginx.ingress.kubernetes.io/auth-snippet",
                        "proxy_set_header X-Forwarded-Host $http_host;proxy_set_header X-Original-URL $scheme://$http_host$request_uri;proxy_set_header X-Forwarded-Proto $scheme;"
                    },
                    {
                        "nginx.ingress.kubernetes.io/configuration-snippet",
                        "more_set_headers \"Access-Control-Allow-Origin: $http_origin\";"
                    },
                    {
                        "nginx.ingress.kubernetes.io/cors-allow-credentials",
                        "true"
                    },
                    {
                        "nginx.ingress.kubernetes.io/cors-allow-methods",
                        "PUT, GET, POST, OPTIONS, DELETE, PATCH"
                    },
                    {
                        "nginx.ingress.kubernetes.io/enable-cors",
                        "true"
                    }
                }
            }
        };
    }
    
    [YamlMember(Alias = "apiVersion")] public string ApiVersion { get; set; } = "networking.k8s.io/v1";
    [YamlMember(Alias = "kind")] public string Kind { get; set; } = "Ingress";
    [YamlMember(Alias = "metadata")] public IngressMetadata Metadata { get; set; } = new();
    [YamlMember(Alias = "spec")] public IngressSpec Spec { get; set; } = new();
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

