using System.CommandLine;

namespace Talaryon.StackManager.Arguments;

public class ImageArgument : Argument<string>
{
    public ImageArgument() : base("image")
    {
        Description = "image (ghcr.io/org/repo:tag)";
    }
}