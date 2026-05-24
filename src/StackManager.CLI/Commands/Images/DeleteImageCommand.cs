using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Commands.Images;

/// <summary>
/// Command for deleting an image.
/// </summary>
public class DeleteImageCommand : ResourceDeleteCommand<StackImage, ImageArgument>
{
    public DeleteImageCommand()
        : base("image", "Delete an image")
    {
        Add(new EnvironmentOption());
        Add(new StackOption());
    }

    protected override StackImage LoadResource(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var stack = GetStack<StackOption>(parseResult, env);
        var name = GetName<ImageArgument>(parseResult);
        return stack.Images.FirstOrDefault(v => v.Name.Equals(name, StringComparison.OrdinalIgnoreCase)) 
            ?? throw new ImageNotFoundException(name);
    }

    protected override void DeleteResourceInstance(StackImage resource)
    {
        var stack = resource.Stack;
        LogBuilder.Question($"Are you sure you want to delete image '{resource.Name}' in stack '{stack.Name}'? ({stack.Environment.Name})")
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

    protected override void OnResourceDeleted(StackImage resource)
    {
    }
}
