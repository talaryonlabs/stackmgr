using Talaryon.StackManager.Models.Kubernetes;

namespace Talaryon.StackManager.Builder;

public interface IStackBuilder
{
    void BuildAll();
    void BuildOutpost();
    void BuildIngresses();
    void BuildRegistryCredentials();
    void BuildApps();
    void BuildKustomization();
}


public class StackBuilder(Stack stack) : IStackBuilder
{
    public void BuildAll()
    {
        BuildRegistryCredentials();
        BuildIngresses();
        BuildApps();

        if (stack.Ingresses.Any(v => v.IsSecured))
        {
            BuildOutpost();
        }
        
        // must be last
        BuildKustomization();
    }

    public void BuildApps()
    {
        stack.Apps.ForEach(BuildApp);
    }

    private void BuildApp(StackApp app)
    {
        var files = app.LocalDirectory.GetFiles("*.yaml", SearchOption.AllDirectories);
        files
            .Where(v => v.Name.StartsWith("blueprint.", StringComparison.OrdinalIgnoreCase))
            .Where(v => !v.FullName.Contains(".base"))
            .ToList()
            .ForEach(v => v.Delete());
        files
            .Where(v => v.Name.StartsWith("template.", StringComparison.OrdinalIgnoreCase))
            .Where(v => !v.FullName.Contains(".base"))
            .ToList()
            .ForEach(v => v.Delete());
        
        var baseFiles = files
            .Where(v => v.FullName.Contains(".base"))
            .ToList();

        foreach (var file in baseFiles)
        {
            var destination = app.LocalDirectory.GetFile(file.Name);
            var content = new ContentBuilder(app)
                .With(file)
                .Build();
            
            var overrideFile = app.LocalDirectory.GetFile($"override.{file.Name.Replace("template.", "")}");
            if (overrideFile.Exists)
                continue;

            File.WriteAllText(destination.FullName, content);
        }
        
    }

    public void BuildKustomization()
    {
        var file = stack.LocalDirectory.GetFile(Kustomization.FileName);
        var resources = stack.LocalDirectory
            .GetFiles("*.yaml", SearchOption.AllDirectories)
            .Where(v => !new List<string> { Kustomization.FileName, Stack.FileName }.Contains(v.Name))
            .Where(v => !v.FullName.Contains(".base"))
            .Where(v => !v.FullName.Contains(".validation"))
            .ToList();
        
        var kustomization = new Kustomization
        {
            Namespace = stack.Namespace,
            Images = stack.Images.Select(i => (KustomizationImage)i).ToList(),
            Resources = resources
                .Select(v => v.FullName.Replace(stack.LocalDirectory.FullName + Path.DirectorySeparatorChar, ""))
                .ToList()
        };
            
        StackResource.Save(kustomization, file);
    }
    
    public void BuildRegistryCredentials()
    {
        var file = stack.LocalDirectory.GetFile("registry-credentials.yaml");
        
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
        var file = stack.LocalDirectory.GetFile(OutpostService.FileName);
        
        StackResource.Save(outpost, file);
    }

    public void BuildIngresses()
    {
        if (stack.Ingresses.Any(v => v.IsSecured) && stack.Environment.Outpost is not { Length: > 0 })
            throw new Exception("Some ingresses are secured, but there is no environment outpost defined.");
        
        var directory = stack.LocalDirectory.GetDirectory(".ingresses");
        if (directory.Exists)
        {
            directory.Delete(true);
        }
        directory.Create();

        foreach (var stackIngress in stack.Ingresses)
        {
            var builder = new IngressBuilder(stackIngress);
            var ingress = builder.ToIngress();
            
            var file = directory.GetFile($"ingress.[{stackIngress.Hostname}].yaml");
            StackResource.Save(ingress, file);
            
            if (stackIngress.IsSecured)
            {
                var authIngress = builder.ToAuthIngress();
                file = directory.GetFile($"ingress.[{stackIngress.Hostname}].auth.yaml");
                StackResource.Save(authIngress, file);
            }
        }
    }
    
    
}