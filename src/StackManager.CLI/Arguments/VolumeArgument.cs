using System.CommandLine;

namespace Talaryon.StackManager.Arguments;

public class VolumeArgument : Argument<string>
{
    public VolumeArgument() : base("volume")
    {
        Description = "volume name";
    }
}