using System.CommandLine;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Commands.Base;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Types;
using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Commands.Implementations;

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

    protected override StackApp LoadResource(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var stack = GetStack<StackOption>(parseResult, env);
        var name = GetName<AppArgument>(parseResult);
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
                resource.Delete();
                return LogBuilder.Message("Done.").AsSuccess();
            })
            .Run();
    }

    protected override void OnResourceDeleted(StackApp resource)
    {
    }
}
