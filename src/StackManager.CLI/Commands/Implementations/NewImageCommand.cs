using System.CommandLine;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Commands.Base;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Types;
using Talaryon.StackManager.Validation;

namespace Talaryon.StackManager.Commands.Implementations;

/// <summary>
/// Command for adding a new image.
/// </summary>
public class NewImageCommand : ResourceCreateCommand<StackImage, ImageArgument>
{
    public NewImageCommand()
        : base("image", "Add a new image")
    {
        Add(new EnvironmentOption());
        Add(new StackOption());
        Add(new NameOption());
    }

    protected override StackImage CreateResourceInstance(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var stack = GetStack<StackOption>(parseResult, env);
        var imageName = GetName<ImageArgument>(parseResult);
        var name = parseResult.GetValue<string, NameOption>();
        
        ValidationHelper.ValidateImageName(imageName);
        
        return StackImage.Create(stack, imageName, name);
    }

    protected override void OnResourceCreated(StackImage resource)
    {
        LogMessage.AsSuccess($"Image '{resource.Image}' with name '{resource.Name}' added.");
    }
}
