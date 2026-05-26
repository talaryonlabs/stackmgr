using System.CommandLine;

namespace Talaryon.StackManager.Commands.Resources;

public abstract class ResourceDescribeCommand<TResource, TArg> : BaseCommand
    where TArg : Argument<string>, new()
{
    protected ResourceDescribeCommand(string name, string description)
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

    protected abstract void DisplayResource(TResource resource);

    protected override async Task ExecuteAsync()
    {
        var resource = await LoadResourceAsync().ConfigureAwait(false);
        DisplayResource(resource);
    }
}
