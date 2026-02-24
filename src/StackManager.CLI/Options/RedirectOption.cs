using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class RedirectOption : Option<string>
{
    public RedirectOption() : base("--redirect")
    {
        Description = "Redirect to this host name.";
    }
}