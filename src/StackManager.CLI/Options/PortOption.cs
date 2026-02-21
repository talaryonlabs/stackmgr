using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class PortOption : Option<short>
{
    public PortOption() : base("--port")
    {
        Description = "port number";
        Arity = ArgumentArity.ExactlyOne;
    }
}