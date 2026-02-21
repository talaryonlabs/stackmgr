using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class RedirectToOption : Option<string>
{
    public RedirectToOption() : base("--redirect-to")
    {
        Description = "Redirect to this host name.";
    }
}