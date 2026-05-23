using System.CommandLine;

namespace Talaryon.StackManager.Options;

public class PortOption : Option<int>
{
    public PortOption() : base("--port")
    {
        Description = "port number (1-65535)";
        Arity = ArgumentArity.ExactlyOne;
    }
}