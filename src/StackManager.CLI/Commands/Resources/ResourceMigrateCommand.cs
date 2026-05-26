using System.CommandLine;

namespace Talaryon.StackManager.Commands.Resources;

public abstract class ResourceMigrateCommand<TResource, TArg> : BaseCommand
    where TArg : Argument<string>, new()
{
    protected ResourceMigrateCommand(string name, string description)
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

    protected virtual void MigrateResource(TResource resource)
    {
        throw new NotImplementedException($"Either {nameof(MigrateResource)} or {nameof(MigrateResourceAsync)} must be overridden.");
    }

    protected virtual Task MigrateResourceAsync(TResource resource)
    {
        MigrateResource(resource);
        return Task.CompletedTask;
    }

    protected override async Task ExecuteAsync()
    {
        var resource = await LoadResourceAsync().ConfigureAwait(false);
        await MigrateResourceAsync(resource).ConfigureAwait(false);
    }
}
