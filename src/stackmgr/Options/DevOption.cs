using System.CommandLine;

namespace stackmgr.Options;

public class DevOption : Option<bool>
{
    public DevOption() : base("--dev")
    {
        Description = "Get app from dev branch";
    }  
}