using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;

namespace Talaryon.StackManager.Commands.Images;

/// <summary>
/// Command for describing a single image.
/// </summary>
public class DescribeImageCommand : ResourceDescribeCommand<StackImage, ImageArgument>
{
    public DescribeImageCommand()
        : base("image", "Describe an image")
    {
        Add(new EnvironmentOption());
        Add(new StackOption());
    }

    protected override StackImage LoadResource()
    {
        var env = GetEnvironment<EnvironmentOption>();
        var stack = GetStack<StackOption>(env);
        var name = GetName<ImageArgument>();
        
        return stack.Get<StackImage>(name);
    }

    protected override void DisplayResource(StackImage resource)
    {
        LogMessage.Separator();

        LogBuilder.Message("Image: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{resource.Name}").AsSuccess())
            .Run();

        LogBuilder.Message(" Repository: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{resource.Image}").AsWarning())
            .Run();

        LogMessage.Separator();
    }
}
