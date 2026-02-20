namespace stackmgr.Services;

public class IngressService(Stack stack)
{
    private const string DirectoryName = ".ingress";
    
    public void SaveAll()
    {
        var directory = new DirectoryInfo(Path.Combine(stack.LocalDirectory.FullName, DirectoryName));
        if(!directory.Exists) directory.Create();
        
        foreach(var v in stack.Ingresses)
        {
            var ingress = CreateIngress(v);
            
            if (v.Secured)
            {
                ingress = CreateSecuredIngress(v);
                var auth = CreateAuthIngress(v);
            }

            if (v.Annotations is { Count: > 0 })
            {
                ingress.Metadata.Annotations = ingress.Metadata.Annotations
                    .Concat(v.Annotations)
                    .ToDictionary();
            }
            
            

        }
    }
    
    private string GetIngressName(StackIngress ingress) => ingress
        .Host
        .Replace(".", "-")
        .ToLower();

    private Ingress CreateIngress(StackIngress ingress)
    {
        var name = GetIngressName(ingress);
        
        return new Ingress()
        {
            Metadata = new() { Name = $"ingress-{name}" },
            Spec = new()
            {
                Rules =
                [
                    new()
                    {
                        Host = ingress.Host,
                        Http = new()
                        {
                            Paths = [new() { Path = "/" }]
                        }
                    }
                ],
                Tls =
                [
                    new() { SecretName = $"letsencrypt-{name}", Hosts = [ingress.Host] },
                ]
            }
        };
    }


    private Ingress CreateSecuredIngress(StackIngress ingress)
    {
        var name = GetIngressName(ingress);
        
        return new Ingress
        {
            Metadata =
            {
                Name = $"ingress-{name}",
                Annotations = new Dictionary<string, string>
                {
                    { "nginx.ingress.kubernetes.io/auth-url", $"http://{stack.Environment.Outpost}:9000/outpost.goauthentik.io/auth/nginx" },
                    {
                        "nginx.ingress.kubernetes.io/auth-signin",
                        $"https://{ingress.Host}/outpost.goauthentik.io/start?rd=$scheme://$http_host$escaped_request_uri"
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

    private Ingress CreateAuthIngress(StackIngress ingress)
    {
        var name = GetIngressName(ingress);
        
        return new Ingress
        {
            Metadata = { Name = $"ingress-{name}-auth" },
            Spec =
            {
                Rules =
                [
                    new()
                    {
                        Host = ingress.Host,
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
                                            Name = $"{stack.Name}-auth",
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
                    new() { SecretName = $"letsencrypt-{name}", Hosts = [ingress.Host] }
                ]
            }
        };
    }

    private Service CreateAuthService(Stack stack)
    {
        
    }
}