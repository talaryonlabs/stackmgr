using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class ApplyOption : Option<bool>
{
    public ApplyOption() : base("--apply")
    {
        Description = "Immediately apply the changes";
    } 
}