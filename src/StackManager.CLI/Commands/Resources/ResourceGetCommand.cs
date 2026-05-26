using System.CommandLine;

namespace Talaryon.StackManager.Commands.Resources;

public abstract class ResourceGetCommand<TResource> : BaseCommand
{
    private readonly string _resourceName;

    protected ResourceGetCommand(
        string name, 
        string description, 
        string resourceName,
        params Option[] options)
        : base(name, description)
    {
        _resourceName = resourceName;
        
        foreach (var option in options)
        {
            Add(option);
        }
    }

   protected virtual IReadOnlyList<TResource> GetResources()
    {
        throw new NotImplementedException($"Either {nameof(GetResources)} or {nameof(GetResourcesAsync)} must be overridden.");
    }

    protected virtual Task<IReadOnlyList<TResource>> GetResourcesAsync()
    {
        return Task.FromResult(GetResources());
    }

    protected abstract void DisplayResource(TResource resource);
    
    protected virtual string ResourceName => _resourceName;
    
    protected virtual void DisplayResources(IReadOnlyList<TResource> resources)
    {
        if (resources.Count == 0)
        {
            LogMessage.AsWarning($"No {ResourceName} found.");
            return;
        }
        
        LogMessage.AsInfo($"{ResourceName}:");
        foreach (var resource in resources)
        {
            DisplayResource(resource);
        }
    }

    protected override async Task ExecuteAsync()
    {
        var resources = await GetResourcesAsync().ConfigureAwait(false);
        DisplayResources(resources);
    }
}
