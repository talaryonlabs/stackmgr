using System.CommandLine;

namespace Talaryon.StackManager.Commands.Remotes;

public class AddRemoteCommand : BaseCommand
{
    public AddRemoteCommand() 
        : base("add", "Add a remote proxy")
    {
        Add([
            new NameArgument(),
            new RemoteArgument(),
            new AccessTokenOption()
        ]);
    }

    protected override void Execute()
    {
        var name = GetRequiredValue<string, NameArgument>();
        var config = GetRequiredService<LocalConfig>();
        if (config.Remotes.Any(r => r.Name == name))
        {
            LogMessage.AsError($"Remote already exists: {name}");
            return;
        }
        
        var remote = new LocalConfigRemote
        {
            Name = name,
            Url = GetRequiredValue<string, RemoteArgument>(),
            AccessToken = GetRequiredValue<string, AccessTokenOption>()
        };
        
        config.Remotes.Add(remote);
        config.Save();
        LogMessage.AsSuccess($"Remote {remote.Name} added.");
    }
}