using Talaryon.StackManager.Exceptions;
using Talaryon.StackManager.Models;
using Talaryon.StackManager.Services;
using Talaryon.StackManager.Types;
using YamlDotNet.Serialization;

namespace Talaryon.StackManager;

public class StackBuilder(Stack stack)
{
    public async Task Build()
    {
        BuildRegistryCredentials();
        BuildOutpostService();
        BuildIngressFiles();
        await BuildRedirect();
        
        var kustomization = new Kustomization
        {
            Namespace = stack.Namespace,
            Images = stack.Images.Select(i => (KustomizationImage)i).ToList(),
            Resources = stack.LocalDirectory
                .GetFiles("*.yaml", SearchOption.AllDirectories)
                .Where(f => !new List<string> { Kustomization.FileName, Stack.FileName }.Contains(f.Name))
                .Select(f => f.FullName.Replace(stack.LocalDirectory.FullName, "").Replace("\\", "/")[1..])
                .ToList()
        };
            
        kustomization.Save(stack);
    }
    
    private async Task BuildRedirect()
    {
        if(stack.Redirects.Count == 0) return;
        
        var git = new GitService(stack.Environment);
        var apps = await git.GetAppsAsync("prod");
        var template = apps.FirstOrDefault(x => x.Name.Equals("redirect", StringComparison.CurrentCultureIgnoreCase));
        if (template is null)
        {
            throw new TemplateNotFoundException("redirect");
        }
        
        var folder = new DirectoryInfo(Path.Combine(stack.LocalDirectory.FullName, StackRedirect.DirectoryName));
        if (!folder.Exists) folder.Create();

        var files = template.GetFileSystemInfos("*", SearchOption.AllDirectories);
        
        foreach (var v in stack.Redirects)
        {
            await v.Migrate(files);
        }
    }

    private void BuildRegistryCredentials()
    {
        var path = Path.Combine(stack.LocalDirectory.FullName, "registry-credentials.yaml");
        var file = new FileInfo(path);
        
        if (stack.Environment.RegistryCredentials is { Length: > 0 })
        {
            var credentials = new RegistryCredentials();
            credentials.Metadata.Annotations.Path = stack.Environment.RegistryCredentials;
            LogMessage.AsInfo($"Using registry credentials '{stack.Environment.RegistryCredentials}' for stack '{stack.Name}'.");
            File.WriteAllText(file.FullName, new Serializer().Serialize(credentials));
        }
        else if (file.Exists)
        {
            file.Delete();
            LogMessage.AsInfo($"Registry credentials for stack '{stack.Name}' are empty. {file.Name} removed.");
        }
    }
    
    private void BuildOutpostService()
    {
        var path = Path.Combine(stack.LocalDirectory.FullName, "svc.outpost.yaml");
        if (stack.Environment.Outpost is { Length: > 0 } && stack.Ingresses.Any(v => v.IsSecured))
        {
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
            File.WriteAllText(path, new Serializer().Serialize(service));
            LogMessage.AsInfo($"Apply outpost service '{path}'.");
        }
        else if (File.Exists(path))
        {
            File.Delete(path);
            LogMessage.AsInfo($"Delete outpost service '{path}'.");
        }
    }

    private void BuildIngressFiles()
    {
        if (stack.Ingresses.Count == 0) return;
        
        if (stack.Ingresses.Any(v => v.IsSecured) && stack.Environment.Outpost is not { Length: >0 })
            throw new Exception("Some ingresses are secured, but there is no environment outpost defined.");

        var folder = new DirectoryInfo(Path.Combine(stack.LocalDirectory.FullName, StackIngress.DirectoryName));
        if (!folder.Exists) folder.Create();
        
        foreach (var ingress in stack.Ingresses)
        {
            ingress.ToIngress().SaveTo(ingress.LocalFile.FullName);
            LogMessage.AsInfo($"Apply ingress file '{ingress.LocalFile.FullName}' for host '{ingress.Hostname}'.");

            var authFile = ingress.LocalFile.FullName.Replace(".yaml", "-auth.yaml");
            if (!ingress.IsSecured)
            {
                if(File.Exists(authFile)) File.Delete(authFile);
                continue;
            }
            
            ingress.GetAuthIngress().SaveTo(authFile);
            LogMessage.AsInfo($"Apply ingress file '{authFile}' for host '{ingress.Hostname}'.");
        }
    }
}