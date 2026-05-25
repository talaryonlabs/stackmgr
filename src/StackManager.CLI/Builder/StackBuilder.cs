using Talaryon.StackManager.Serialization;
using Talaryon.StackManager.Models.Kubernetes;

namespace Talaryon.StackManager.Builder;

public interface IStackBuilder
{
    void BuildAll();
    void BuildOutpost();
    void BuildIngresses();
    void BuildRegistryCredentials();
    void BuildKustomization();
}


public class StackBuilder(Stack stack) : IStackBuilder
{
    public void BuildAll()
    {
        BuildRegistryCredentials();
        BuildIngresses();

        if (stack.Ingresses.Any(v => v.IsSecured))
        {
            BuildOutpost();
        }
        
        // must be last
        BuildKustomization();
    }
    
    public void BuildKustomization()
    {
        var path = Path.Combine(stack.LocalDirectory.FullName, Kustomization.FileName);
        var file = new FileInfo(path);
        
        var allFiles = stack.LocalDirectory
            .GetFiles("*.yaml", SearchOption.AllDirectories)
            .Where(v => !new List<string> { Kustomization.FileName, Stack.FileName }.Contains(v.Name))
            .ToList();

        var normalFiles = allFiles
            .Where(v => !v.FullName.Contains(".base"))
            .ToList();
        
        var baseFiles = allFiles
            .Where(v => v.FullName.Contains(".base"))
            .Where(v =>
            {
                return !normalFiles.Any(f => f.Name.Equals($"override.{v.Name}"));
            })
            .ToList();
        
        var kustomization = new Kustomization
        {
            Namespace = stack.Namespace,
            Images = stack.Images.Select(i => (KustomizationImage)i).ToList(),
            Resources = normalFiles
                .Concat(baseFiles)
                .Select(v => v.FullName.Replace(stack.LocalDirectory.FullName + Path.DirectorySeparatorChar, ""))
                .ToList()
        };
            
        StackResource.Save(kustomization, file);
    }
    
    public void BuildRegistryCredentials()
    {
        var path = Path.Combine(stack.LocalDirectory.FullName, "registry-credentials.yaml");
        var file = new FileInfo(path);
        
        if (stack.Environment.RegistryCredentials is { Length: > 0 })
        {
            var credentials = new RegistryCredentials();
            credentials.Metadata.Annotations.Path = stack.Environment.RegistryCredentials;
            
            StackResource.Save(credentials, file);
        }
        else if (file.Exists)
        {
            file.Delete();
        }
    }

    public void BuildOutpost()
    {
        var outpost = new OutpostService(stack);
        var path = Path.Combine(stack.LocalDirectory.FullName, OutpostService.FileName);
        var file = new FileInfo(path);
        
        StackResource.Save(outpost, file);
    }

    public void BuildIngresses()
    {
        if (stack.Ingresses.Any(v => v.IsSecured) && stack.Environment.Outpost is not { Length: > 0 })
            throw new Exception("Some ingresses are secured, but there is no environment outpost defined.");
        
        var path = Path.Combine(stack.LocalDirectory.FullName, ".ingresses");
        var directory = new DirectoryInfo(path);
        
        if (directory.Exists)
        {
            directory.Delete(true);
        }
        directory.Create();

        foreach (var stackIngress in stack.Ingresses)
        {
            var builder = new IngressBuilder(stackIngress);
            var ingress = builder.ToIngress();
            
            var file = new FileInfo(Path.Combine(directory.FullName, $"ingress.[{stackIngress.Hostname}].yaml"));
            StackResource.Save(ingress, file);
            
            if (stackIngress.IsSecured)
            {
                var authIngress = builder.ToAuthIngress();
                file = new FileInfo(Path.Combine(directory.FullName, $"ingress.[{stackIngress.Hostname}].auth.yaml"));
                StackResource.Save(authIngress, file);
            }
        }
    }
}