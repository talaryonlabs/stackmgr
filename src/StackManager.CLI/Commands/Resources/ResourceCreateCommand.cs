using System.CommandLine;

namespace Talaryon.StackManager.Commands.Resources;

public abstract class ResourceCreateCommand<TResource, TArg> : BaseCommand
    where TArg : Argument<string>, new()
{
    protected ResourceCreateCommand(string name, string description)
        : base(name, description)
    {
        Add(new TArg());
    }
    
    protected virtual TResource CreateResourceInstance()
    {
        throw new NotImplementedException($"Either {nameof(CreateResourceInstance)} or {nameof(CreateResourceInstanceAsync)} must be overridden.");
    }

    protected virtual Task<TResource> CreateResourceInstanceAsync()
    {
        return Task.FromResult(CreateResourceInstance());
    }
    
    protected abstract void OnResourceCreated(TResource resource);

    protected override async Task ExecuteAsync()
    {
        var resource = await CreateResourceInstanceAsync().ConfigureAwait(false);
        OnResourceCreated(resource);
    }
}
