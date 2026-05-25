using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class SecuredOption : Option<bool>
{
    public SecuredOption() : base("--secured")
    {
        Description = "Create a secured ingress for the outpost";
    }
}