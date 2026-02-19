using stackmgr.Arguments;
using stackmgr.Options;
using YamlDotNet.Serialization;

namespace stackmgr.Commands;

public class BuildCommand : StackManagerCommand
{
    public BuildCommand() : base("build", "Build a stack")
    {
        Add(new EnvironmentOption());
        Add(new StackArgument());
        SetAction(parseResult =>
        {
            var env = GetEnvironment<EnvironmentOption>(parseResult);
            var stack = GetStack<StackArgument>(parseResult, env);
            
            HelperMethods.LogInfo($"Building stack '{stack.Name}' in environment '{env.Name}'");
            
            BuildRegistryCredentials(stack);
            BuildOutpostService(stack);
            BuildIngressFiles(stack);
            stack.SaveKustomization();
            HelperMethods.LogSuccess($"Stack '{stack.Name}' built.");
            HelperMethods.LogInfo("Run git commit and git push before stack sync.");
        });
    }

    private void BuildRegistryCredentials(Stack stack)
    {
        var path = Path.Combine(stack.LocalDirectory.FullName, "registry-credentials.yaml");
        var file = new FileInfo(path);
        
        if (stack.Environment.RegistryCredentials is { Length: > 0 })
        {
            var credentials = new RegistryCredentials();
            credentials.Metadata.Annotations.Path = stack.Environment.RegistryCredentials;
            HelperMethods.LogInfo($"Using registry credentials '{stack.Environment.RegistryCredentials}' for stack '{stack.Name}'.");
            File.WriteAllText(file.FullName, new Serializer().Serialize(credentials));
        }
        else if (file.Exists)
        {
            file.Delete();
            HelperMethods.LogInfo($"Registry credentials for stack '{stack.Name}' are empty. {file.Name} removed.");
        }
    }
    
    private void BuildOutpostService(Stack stack)
    {
        if (stack.Environment.Outpost is not { Length: > 0 }) return;

        var service = new Service
        {
            Metadata =
            {
                Name = $"{stack.Name}-auth"
            },
            Spec =
            {
                Type = "ExternalName",
                ExternalName = stack.Environment.Outpost
            }
        };
        
        var path = Path.Combine(stack.LocalDirectory.FullName, "svc.outpost.yaml");
        File.WriteAllText(path, new Serializer().Serialize(service));
    }

    private void BuildIngressFiles(Stack stack)
    {
        var template = Path.Combine(stack.LocalDirectory.FullName, "ingress.[{{name}}].yaml");

        foreach (var v in stack.Ingresses)
        {
            var name = v.Host
                .Replace(".", "-")
                .ToLower();
            var ingress = new Ingress()
            {
                Metadata = new() { Name = $"ingress-{name}" },
                Spec = new()
                {
                    Rules =
                    [
                        new()
                        {
                            Host = v.Host,
                            Http = new()
                            {
                                Paths = [new() { Path = "/" }]
                            }
                        }
                    ],
                    Tls =
                    [
                        new() { SecretName = $"letsencrypt-{name}", Hosts = [v.Host] },
                    ]
                }
            };

            var file = template.Replace("{{name}}", v.Host);
            File.WriteAllText(file, new Serializer().Serialize(ingress));
            HelperMethods.LogInfo($"Generated ingress file '{file}' for stack '{stack.Name}' with host '{v.Host}'.");
        }
    }
}