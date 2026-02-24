using Talaryon.StackManager.Models;
using YamlDotNet.Serialization;

namespace Talaryon.StackManager.Types;

public class StackIngress : IStackObject
{
    public const string DirectoryName = ".ingresses";
    
    public static StackIngress Create(Stack stack, string hostname, string app, short port, bool secured = false)
    {
        if(stack.Ingresses.Any(v => v.Hostname.Equals(hostname, StringComparison.InvariantCultureIgnoreCase)))
            throw new Exception($"Ingress {hostname} already exists.");
        
        var ingress = new StackIngress
        {
            Hostname = hostname,
            Application = app,
            Port = port,
            Stack = stack,
            IsSecured = secured,
        };

        lock (stack.Ingresses)
        {
            stack.Ingresses.Add(ingress);
        }
        stack.SaveConfig();
        
        return ingress;
    }

    public static StackIngress Create(Stack stack, string hostname, string redirectTo)
    {
        if(stack.Ingresses.Any(v => v.Hostname.Equals(hostname, StringComparison.InvariantCultureIgnoreCase)))
            throw new Exception($"Ingress {hostname} already exists.");
        
        var ingress = new StackIngress
        {
            Hostname = hostname,
            Redirect = redirectTo,
            Stack = stack,
            Application = $"redirect-{HelperMethods.HostToName(redirectTo)}",
            Port = 80
        };
        
        stack.Ingresses.Add(ingress);
        stack.SaveConfig();

        return ingress;
    }

    public void Delete()
    {
        LocalDirectory
            .GetFiles(LocalFile.Name.Replace(".yaml", "*"))
            .ToList()
            .ForEach(v =>
            {
                v.Delete();
            });
        
        lock (Stack.Ingresses)
        {
            Stack.Ingresses.Remove(this);
        }
        Stack.SaveConfig();
    }
    
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
            Metadata = new() { Name = $"ingress-{Name}" },
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

        if (IsSecured)
        {
            ingress.Metadata.Annotations = new Dictionary<string, string>
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
        }

        return ingress;
    }
    
    [YamlIgnore] public required Stack Stack { get; set; }

    [YamlIgnore] public DirectoryInfo LocalDirectory => new(
        Path.Combine(Stack.LocalDirectory.FullName, DirectoryName)
    );
    [YamlIgnore]
    public FileInfo LocalFile => new(
        Path.Combine(LocalDirectory.FullName, $"ingress.[{Hostname}].yaml")
    );
    [YamlIgnore] public string Name => HelperMethods.HostToName(Hostname);
    
    [YamlMember(Alias = "isSecured")] public bool IsSecured { get; init; }
    [YamlMember(Alias = "hostname")] public required string Hostname { get; init; }
    [YamlMember(Alias = "app")] public string? Application { get; init; }
    [YamlMember(Alias = "port")] public short Port { get; init; }
    [YamlMember(Alias = "redirect")] public string? Redirect { get; init; }
    [YamlMember(Alias = "annotations")] public Dictionary<string, string>? Annotations { get; init; } = [];
}