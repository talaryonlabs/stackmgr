using Talaryon.StackManager.Models;

namespace Talaryon.StackManager.Services;

public interface IIngressService
{
    IIngressServiceActions Directory(DirectoryInfo directory);
    IIngressServiceActions Directory(string directory);
}

public interface IIngressServiceActions
{
    IReadOnlyList<Ingress> GetIngresses();
    
    void Save(Ingress ingress);
}

public class IngressService : IIngressService, IIngressServiceActions
{
    private string DirectoryName => ".ingresses";
    private DirectoryInfo _currentDirectory = new(Environment.CurrentDirectory);
    
    IIngressServiceActions IIngressService.Directory(DirectoryInfo directory)
    {
        _currentDirectory = new DirectoryInfo(Path.Combine(directory.FullName, DirectoryName));
        return this;
    }

    IIngressServiceActions IIngressService.Directory(string directory)
    {
        _currentDirectory = new DirectoryInfo(Path.Combine(Environment.CurrentDirectory, directory, DirectoryName));
        return this;
    }

    IReadOnlyList<Ingress> IIngressServiceActions.GetIngresses()
    {
        var ingresses = _currentDirectory
            .GetFiles("*.yaml")
            .Select(StackResource.Load<Ingress>)
            .ToList();

        return ingresses;
    }
    
    void IIngressServiceActions.Save(Ingress ingress)
    {
        var path = Path.Combine(_currentDirectory.FullName, $"ingress.[{ingress.Hostname}].yaml");
        var file = new FileInfo(path);
        
        StackResource.Save(ingress, file);
    }
}