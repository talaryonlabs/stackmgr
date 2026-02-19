using System.CommandLine;

namespace stackmgr.Arguments;

public class HostArgument : Argument<string>
{
    public HostArgument() : base("host")
    {
        Description = "host name (e.g. example.com, sub.example.com)";
    }   
}