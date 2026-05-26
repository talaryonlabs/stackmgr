using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Commands.Apps;

/// <summary>
/// Command for deleting an app.
/// </summary>
public class DeleteAppCommand : ResourceDeleteCommand<StackApp, AppArgument>
{
    public DeleteAppCommand()
        : base("app", "Delete an application")
    {
        Add(new EnvironmentOption());
        Add(new StackOption());
    }

    protected override StackApp LoadResource()
    {
        var env = GetEnvironment<EnvironmentOption>();
        var stack = GetStack<StackOption>(env);
        var name = GetName<AppArgument>();
        return stack.Apps.FirstOrDefault(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) 
            ?? throw new AppNotFoundException(name);
    }

    protected override void DeleteResourceInstance(StackApp resource)
    {
        var stack = resource.Stack;
        LogBuilder.Question($"Are you sure you want to delete app '{resource.Name}' in stack '{stack.Name}'? ({stack.Environment.Name})")
            .AsYesNo()
            .AsWarning()
            .NoNewLineAfter()
            .WaitFor(result =>
            {
                if (!result) return LogBuilder.Message("Aborted.");
                resource.Delete<StackApp>();
                return LogBuilder.Message("Done.").AsSuccess();
            })
            .Run();
    }

    protected override void OnResourceDeleted(StackApp resource)
    {
    }
}
