using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class AccessModeOption : Option<string>
{
    public AccessModeOption() : base("--access-mode")
    {
        Description = "access mode (e.g. ReadWriteMany)";
    }
}