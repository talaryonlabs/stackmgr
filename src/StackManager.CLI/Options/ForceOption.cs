using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class ForceOption : Option<bool>
{
    public ForceOption() : base("--force", "-f")
    {
        Description = "Force deletion without confirmation";
        Arity = ArgumentArity.ZeroOrOne;
    }
}
