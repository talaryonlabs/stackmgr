using System.CommandLine;
using Talaryon.Toolbox;

namespace Talaryon.StackManager.Options;

public class SizeOption : Option<string>
{
    public SizeOption() : base("--size")
    {
        Description = "storage size (e.g. 10Gi)";
    }
}