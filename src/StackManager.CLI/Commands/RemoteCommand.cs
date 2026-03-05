using System.CommandLine;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Services;
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
        
        var test = new StackManagerCommand("test", "Test a remote proxy")
        {
            new NameArgument()
        };
        test.SetAction(TestRemote);
        
        Add(add);
        Add(remove);
        Add(test);
        SetAction(_ =>
        {
            if (LocalConfig.Get().Remotes.Count == 0)
            {
                LogMessage.AsWarning("No remotes found.");
                return;
            }
            LogMessage.AsInfo("Remotes:");
            foreach (var remote in LocalConfig.Get().Remotes)
            {
                LogMessage.AsSuccess($"- {remote.Name}: {remote.Url}");
            }
        });
    }

    private async Task TestRemote(ParseResult obj)
    {
        var config = LocalConfig.Get();
        var remote = config.Remotes.FirstOrDefault(r => r.Name == obj.GetRequiredValue<string, NameArgument>());
        if (remote == null)
        {
            LogMessage.AsError($"Remote not found: {obj.GetRequiredValue<string, NameArgument>()}");
            return;
        }

        await LogBuilder.Message($"Testing Connection '{remote.Name}' ...")
            .WaitFor(async () =>
            {
                using var proxy = new ProxyService(remote);
                if (await proxy.TestConnectionAsync())
                {
                    return LogBuilder.Message("Done.").AsSuccess();
                }
                return LogBuilder.Message("Failed.").AsError();
            })
            .NoNewLineAfter()
            .RunAsync();
        
    }

    private void AddRemote(ParseResult parseResult)
    {
        var config = LocalConfig.Get();
        if (config.Remotes.Any(r => r.Name == parseResult.GetRequiredValue<string, NameArgument>()))
        {
            LogMessage.AsError($"Remote already exists: {parseResult.GetRequiredValue<string, NameArgument>()}");
            return;
        }
        
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
        LogMessage.AsSuccess($"Remote {remote.Name} added.");
    }

    private void RemoveRemote(ParseResult obj)
    {
        var config = LocalConfig.Get();
        var remote = config.Remotes.FirstOrDefault(v => v.Name == obj.GetRequiredValue<string, NameArgument>());
        if (remote is null)
        {
            LogMessage.AsError($"Remote {obj.GetRequiredValue<string, NameArgument>()} not found.");
            return;
        }
        
        config.Remotes.Remove(remote);
        config.Save();
        LogMessage.AsSuccess($"Remote {remote.Name} removed.");
    }
}