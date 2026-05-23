using System.Security;
using Talaryon.StackManager.Exceptions;
using Talaryon.StackManager.Models;
using Talaryon.StackManager.Serialization;
using Talaryon.StackManager.Services;
using Talaryon.StackManager.Types;

namespace Talaryon.StackManager;

public class StackBuilder(Stack stack)
{
    public async Task BuildAsync()
    {
        BuildRegistryCredentials();
        BuildOutpostService();
        BuildIngressFiles();
        
        var kustomization = new Kustomization
        {
            Namespace = stack.Namespace,
            Images = stack.Images.Select(i => (KustomizationImage)i).ToList(),
            Resources = stack.LocalDirectory
                .GetFiles("*.yaml", SearchOption.AllDirectories)
                .Where(f => !new List<string> { Kustomization.FileName, Stack.FileName }.Contains(f.Name))
                .Select(f => GetRelativePath(f, stack.LocalDirectory))
                .ToList()
        };
            
        kustomization.Save(stack);
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
            File.WriteAllText(file.FullName, YamlSerializer.Serialize(credentials));
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
            File.WriteAllText(path, YamlSerializer.Serialize(service));
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

            var authFile = Path.ChangeExtension(ingress.LocalFile.FullName, "-auth.yaml");
            if (!ingress.IsSecured)
            {
                if(File.Exists(authFile)) File.Delete(authFile);
                continue;
            }
            
            ingress.GetAuthIngress().SaveTo(authFile);
            LogMessage.AsInfo($"Apply ingress file '{authFile}' for host '{ingress.Hostname}'.");
        }
    }

    private static string GetRelativePath(FileInfo file, DirectoryInfo root)
    {
        var fullPath = Path.GetFullPath(file.FullName);
        var rootPath = Path.GetFullPath(root.FullName + Path.DirectorySeparatorChar);
        
        if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new SecurityException(
                $"File '{file.FullName}' is outside root directory '{root.FullName}'");
        }
        
        var relative = fullPath.Substring(rootPath.Length).Replace("\\", "/");
        return relative;
    }
}