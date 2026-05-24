using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Validation;

namespace Talaryon.StackManager.Commands.Images;

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
        
        if (string.IsNullOrEmpty(name))
        {
            var parts = imageName.Split("/");
            name = parts[^1].Contains(':') ? parts[^1].Split(":")[0] : parts[^1];
        }

        ValidationHelper.ValidateImageName(imageName);

        return stack
            .New<StackImage>()
            .WithName(name)
            .Configure(i => i.Image = imageName)
            .Save();
    }

    protected override void OnResourceCreated(StackImage resource)
    {
        LogMessage.AsSuccess($"Image '{resource.Image}' with name '{resource.Name}' added.");
    }
}
