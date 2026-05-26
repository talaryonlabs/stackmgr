using System.CommandLine;

namespace Talaryon.StackManager.Commands.Resources;

public abstract class ResourceConfigureCommand<TArg> : BaseCommand
    where TArg : Argument<string>, new()
{
    protected ResourceConfigureCommand(string name, string description)
        : base(name, description)
    {
        Add(new TArg());
    }

    protected abstract void Configure();

    protected override async Task ExecuteAsync()
    {
        Configure();
        await Task.CompletedTask;
    }
}
