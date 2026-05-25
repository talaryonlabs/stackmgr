using Talaryon.StackManager.Models.Kubernetes;

namespace Talaryon.StackManager.Builder;

public interface IIngressBuilder
{
    Ingress ToIngress();
    Ingress ToAuthIngress();
}

public class IngressBuilder(StackIngress stackIngress) : IIngressBuilder
{
    /*
    
    void IIngressServiceActions.Save(StackIngress stackIngress)
    {
        if(!_currentDirectory.Exists)
            _currentDirectory.Create();
        
        var path = Path.Combine(_currentDirectory.FullName, $"ingress.[{stackIngress.Hostname}].yaml");
        var file = new FileInfo(path);
        
        var ingress = ToIngress(stackIngress);
        StackResource.Save(ingress, file);

        if (stackIngress.IsSecured)
        {
            path = Path.Combine(_currentDirectory.FullName, $"ingress.[{stackIngress.Hostname}].auth.yaml");
            file = new FileInfo(path);
            ingress = ToAuthIngress(stackIngress);
            StackResource.Save(ingress, file);
        }
    }*/

    public Ingress ToIngress()
    {
        var name = stackIngress.Name ?? HelperMethods.HostToName(stackIngress.Hostname);
        var ingress = new Ingress
        {
            Metadata = new()
            {
                Name = $"ingress-{name}",
                Annotations = stackIngress.Annotations ?? []
            },
            Spec = new()
            {
                Rules =
                [
                    new()
                    {
                        Host = stackIngress.Hostname,
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
                                            Name = stackIngress.Application,
                                            Port = new() { Number = stackIngress.Port }
                                        }
                                    }
                                },
                            ]
                        }
                    }
                ],
                Tls =
                [
                    new() { SecretName = $"letsencrypt-{name}", Hosts = [stackIngress.Hostname] },
                ]
            }
        };

        if (stackIngress.Stack.Environment.CertIssuer is { Length: >0 })
        {
            ingress.Metadata.Annotations.TryAdd("cert-manager.io/cluster-issuer", stackIngress.Stack.Environment.CertIssuer);
        }

        if (stackIngress.IsSecured)
        {
            var securingAnnotations = new Dictionary<string, string>
            {
                {
                    "nginx.ingress.kubernetes.io/auth-url",
                    $"http://{stackIngress.Stack.Environment.Outpost}:9000/outpost.goauthentik.io/auth/nginx"
                },
                {
                    "nginx.ingress.kubernetes.io/auth-signin",
                    $"https://{stackIngress.Hostname}/outpost.goauthentik.io/start?rd=$scheme://$http_host$escaped_request_uri"
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

    public Ingress ToAuthIngress()
    {
        var name = stackIngress.Name ?? HelperMethods.HostToName(stackIngress.Hostname);
        return new Ingress
        {
            Metadata = { Name = $"ingress-{name}-auth" },
            Spec =
            {
                Rules =
                [
                    new()
                    {
                        Host = stackIngress.Hostname,
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
                                            Name = $"{stackIngress.Stack.Name}-auth",
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
                    new() { SecretName = $"letsencrypt-{name}", Hosts = [stackIngress.Hostname] }
                ]
            }
        };
    }
}