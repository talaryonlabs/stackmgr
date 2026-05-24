using Talaryon.StackManager.Models;

namespace Talaryon.StackManager.Builder;

public class IngressBuilder(StackIngress ingress)
{
    public DirectoryInfo LocalDirectory => new(
        Path.Combine(ingress.Stack.LocalDirectory.FullName, ".ingresses")
    );
    public FileInfo LocalFile => new(
        Path.Combine(LocalDirectory.FullName, $"ingress.[{ingress.Hostname}].yaml")
    );
    
    public Ingress GetAuthIngress()
    {
        return new Ingress
        {
            Metadata = { Name = $"ingress-{Name}-auth" },
            Spec =
            {
                Rules =
                [
                    new()
                    {
                        Host = Hostname,
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
                                            Name = $"{Stack.Name}-auth",
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
                    new() { SecretName = $"letsencrypt-{Name}", Hosts = [Hostname] }
                ]
            }
        };
    }

    public Ingress ToIngress()
    {
        var ingress = new Ingress()
        {
            Metadata = new()
            {
                Name = $"ingress-{Name}",
                Annotations = Annotations ?? []
            },
            Spec = new()
            {
                Rules =
                [
                    new()
                    {
                        Host = Hostname,
                        Http = new()
                        {
                            Paths =
                            [
                                new()
                                {
                                    Path = "/",
                                    Backend = new()
                                    {
                                        Service = new()
                                        {
                                            Name = Application,
                                            Port = new() { Number = Port }
                                        }
                                    }
                                },
                            ]
                        }
                    }
                ],
                Tls =
                [
                    new() { SecretName = $"letsencrypt-{Name}", Hosts = [Hostname] },
                ]
            }
        };

        if (Stack.Environment.CertIssuer is { Length: >0 })
        {
            ingress.Metadata.Annotations.TryAdd("cert-manager.io/cluster-issuer", Stack.Environment.CertIssuer);
        }

        if (IsSecured)
        {
            var securingAnnotations = new Dictionary<string, string>
            {
                {
                    "nginx.ingress.kubernetes.io/auth-url",
                    $"http://{Stack.Environment.Outpost}:9000/outpost.goauthentik.io/auth/nginx"
                },
                {
                    "nginx.ingress.kubernetes.io/auth-signin",
                    $"https://{Hostname}/outpost.goauthentik.io/start?rd=$scheme://$http_host$escaped_request_uri"
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
            };
            
            foreach(var (key, value) in securingAnnotations) 
                ingress.Metadata.Annotations.TryAdd(key, value);
        }

        return ingress;
    }
}