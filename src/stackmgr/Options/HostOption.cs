using System.CommandLine;

namespace stackmgr.Options;

public class HostOption : Option<string>
{
    public HostOption() : base("--host")
    {
        Description = "host name (e.g. example.com, sub.example.com)";
    }
}