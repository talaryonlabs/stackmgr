using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class VolumeOption : Option<string>
{
    public VolumeOption() : base("--volume")
    {
        Description = "volume name from longhorn (e.g. data)";
    }
}