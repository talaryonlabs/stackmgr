using System.CommandLine;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Options;
using Talaryon.Toolbox.Extensions;

namespace Talaryon.StackManager.Commands;

public class RemoteCommand : StackManagerCommand
{
    public RemoteCommand() : base("remote", "Manage remote proxy")
    {
        var add = new StackManagerCommand("add", "Add a remote proxy")
        {
            new NameArgument(),
            new RemoteArgument(),
            new AccessTokenOption()
        };
        add.SetAction(AddRemote);

        var remove = new StackManagerCommand("remove", "Remove a remote proxy")
        {
            new NameArgument()
        };
        remove.SetAction(RemoveRemote);
        
        Add(add);
        Add(remove);
        SetAction(_ =>
        {
            HelperMethods.LogInfo("Remotes:");
            foreach (var remote in LocalConfig.Get().Remotes)
            {
                HelperMethods.LogSuccess($"- {remote.Name}: {remote.Url}");
            }
        });
    }
    
    private void AddRemote(ParseResult parseResult)
    {
        var config = LocalConfig.Get();
        var remote = new LocalConfigRemote
        {
            Name = parseResult.GetRequiredValue<string, NameArgument>(),
            Url = parseResult.GetRequiredValue<string, RemoteArgument>(),
            AccessToken = parseResult
                .GetRequiredValue<string, AccessTokenOption>()
                .ToBase64String()
        };
        
        config.Remotes.Add(remote);
        config.Save();
    }

    private void RemoveRemote(ParseResult obj)
    {
        var config = LocalConfig.Get();
        var remote = config.Remotes.FirstOrDefault(v => v.Name == obj.GetRequiredValue<string, NameArgument>());
        if (remote is null)
        {
            HelperMethods.LogError($"Remote {obj.GetRequiredValue<string, NameArgument>()} not found.");
            return;
        }
        
        config.Remotes.Remove(remote);
        config.Save();
    }
}