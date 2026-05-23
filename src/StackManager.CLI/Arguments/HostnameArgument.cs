using System.CommandLine;

namespace Talaryon.StackManager.Arguments;

public class HostnameArgument : Argument<string>
{
    public HostnameArgument() : base("hostname")
    {
        Description = "host name (e.g. example.com, sub.example.com)";
    }   
}