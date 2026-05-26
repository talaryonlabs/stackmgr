namespace Talaryon.StackManager.Commands.Remotes;

public class SetRemoteCommand : BaseCommand
{
    public SetRemoteCommand()
        : base("set", "Set the access token")
    {
        Add([new NameArgument(), new AccessTokenOption()]);
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
        
        remote.AccessToken = GetRequiredValue<string, AccessTokenOption>();
        config.Save();
        LogMessage.AsSuccess($"Remote {remote.Name} updated.");
    }
}