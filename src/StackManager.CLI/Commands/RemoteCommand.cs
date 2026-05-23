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

        var set = new StackManagerCommand("set", "Set the access token")
        {
            new NameArgument(),
            new AccessTokenOption()
        };
        set.SetAction(SetRemote);
        
        var test = new StackManagerCommand("test", "Test a remote proxy")
        {
            new NameArgument()
        };
        test.SetAction(TestRemote);

        var generate = new StackManagerCommand("generate", "Generate deployment file for kubectl apply.")
        {
            new NameArgument(),
            new HostnameArgument(),
            new CertIssuerOption()
        };
        generate.SetAction(GenerateRemote);
        
        Add(add);
        Add(remove);
        Add(test);
        Add(set);
        // Add(generate);
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

    private void GenerateRemote(ParseResult obj)
    {
        // download from https://raw.githubusercontent.com/talaryonlabs/stackmgr/refs/heads/main/deployment/gen.json
        
        
        
        
        throw new NotImplementedException();
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

        var httpClientFactory = GetRequiredService<IHttpClientFactory>();
        await LogBuilder.Message($"Testing Connection '{remote.Name}' ...")
            .WaitFor(async () =>
            {
                try
                {
                    using var proxy = new ProxyService(remote, httpClientFactory);
                    if (await proxy.TestConnectionAsync())
                    {
                        return LogBuilder.Message("Done.").AsSuccess();
                    }
                    return LogBuilder.Message("Failed.").AsError();
                }
                catch (Exception ex)
                {
                    return LogBuilder.Message($"Failed: {ex}").AsError();
                }
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
            AccessToken = parseResult.GetRequiredValue<string, AccessTokenOption>()
        };
        
        config.Remotes.Add(remote);
        config.Save();
        LogMessage.AsSuccess($"Remote {remote.Name} added.");
    }
    
    private void SetRemote(ParseResult obj)
    {
        var config = LocalConfig.Get();
        var remote = config.Remotes.FirstOrDefault(v => v.Name == obj.GetRequiredValue<string, NameArgument>());
        if (remote is null)
        {
            LogMessage.AsError($"Remote {obj.GetRequiredValue<string, NameArgument>()} not found.");
            return;
        }
        
        remote.AccessToken = obj.GetRequiredValue<string, AccessTokenOption>();
        config.Save();
        LogMessage.AsSuccess($"Remote {remote.Name} updated.");
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