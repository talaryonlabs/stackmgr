using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;

namespace Talaryon.StackManager.Commands.Images;

/// <summary>
/// Command for listing images in a stack.
/// </summary>
public class GetImagesCommand : ResourceGetCommand<StackImage>
{
    public GetImagesCommand()
        : base("images", "List images", "Images", new EnvironmentOption(), new StackOption())
    {
    }

    protected override IReadOnlyList<StackImage> GetResources(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var stack = GetStack<StackOption>(parseResult, env);
        return stack.Images;
    }

    protected override void DisplayResource(StackImage resource)
    {
        LogMessage.AsSuccess($"- {resource.Name}: {resource.Image}");
    }
}
