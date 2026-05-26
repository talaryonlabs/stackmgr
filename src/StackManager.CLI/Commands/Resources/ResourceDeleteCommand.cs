using System.CommandLine;

namespace Talaryon.StackManager.Commands.Resources;

public abstract class ResourceDeleteCommand<TResource, TArg> : BaseCommand
    where TArg : Argument<string>, new()
{
    protected ResourceDeleteCommand(string name, string description)
        : base(name, description)
    {
        Add(new TArg());
    }
    
    protected virtual TResource LoadResource()
    {
        throw new NotImplementedException($"Either {nameof(LoadResource)} or {nameof(LoadResourceAsync)} must be overridden.");
    }
    
    protected virtual Task<TResource> LoadResourceAsync()
    {
        return Task.FromResult(LoadResource());
    }

    protected virtual void DeleteResourceInstance(TResource resource)
    {
        throw new NotImplementedException($"Either {nameof(DeleteResourceInstance)} or {nameof(DeleteResourceInstanceAsync)} must be overridden.");
    }

    protected virtual Task DeleteResourceInstanceAsync(TResource resource)
    {
        DeleteResourceInstance(resource);
        return Task.CompletedTask;
    }

    protected abstract void OnResourceDeleted(TResource resource);

    protected override async Task ExecuteAsync()
    {
        var resource = await LoadResourceAsync().ConfigureAwait(false);
        await DeleteResourceInstanceAsync(resource).ConfigureAwait(false);
        OnResourceDeleted(resource);
    }
}
