using System.CommandLine;

namespace Talaryon.StackManager.Commands.Remotes;

public class RemoveRemoteCommand : BaseCommand
{
    public RemoveRemoteCommand() 
        : base("remove", "Remove a remote proxy")
    {
        Add(new NameArgument());
    }

    protected override void Execute()
    {
        var name = GetRequiredValue<string, NameArgument>();
        var config = GetRequiredService<LocalConfig>();
        var remote = config.Remotes.FirstOrDefault(v => v.Name == name);
        if (remote is null)
        {
            LogMessage.AsError($"Remote {name} not found.");
            return;
        }
        
        config.Remotes.Remove(remote);
        config.Save();
        LogMessage.AsSuccess($"Remote {remote.Name} removed.");
    }
}