using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class DevOption : Option<bool>
{
    public DevOption() : base("--dev")
    {
        Description = "Get app from dev branch";
    }  
}