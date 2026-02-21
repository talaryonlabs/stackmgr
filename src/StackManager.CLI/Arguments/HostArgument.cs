using System.CommandLine;

namespace Talaryon.StackManager.Arguments;

public class HostArgument : Argument<string>
{
    public HostArgument() : base("host")
    {
        Description = "host name (e.g. example.com, sub.example.com)";
    }   
}