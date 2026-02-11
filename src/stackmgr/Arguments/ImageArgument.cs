using System.CommandLine;

namespace stackmgr.Arguments;

public class ImageArgument : Argument<string>
{
    public ImageArgument() : base("image")
    {
        Description = "image (ghcr.io/org/repo:tag)";
    }
}