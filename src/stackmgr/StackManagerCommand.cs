using System.CommandLine;

namespace stackmgr;

public class StackManagerCommand(string name, string description) : Command(name, description)
{
    protected static readonly StackManagerConfig Config;
    
    static StackManagerCommand()
    {
        Config = StackManagerConfig.Load();
    }
}