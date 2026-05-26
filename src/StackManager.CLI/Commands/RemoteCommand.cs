using System.CommandLine;
using Talaryon.StackManager.Commands.Remotes;
using Talaryon.StackManager.Services;

namespace Talaryon.StackManager.Commands;

public class RemoteCommand : BaseCommand
{
    public RemoteCommand() : base("remote", "Manage remote proxy")
    {
        Add([
            new AddRemoteCommand(),
            new RemoveRemoteCommand(),
            new SetRemoteCommand(),
            new TestRemoteCommand()
        ]);
     
        // var generate = new BaseCommand("generate", "Generate deployment file for kubectl apply.")
        // {
        //     new NameArgument(),
        //     new HostnameArgument(),
        //     new CertIssuerOption()
        // };
        // generate.SetAction(GenerateRemote);
        
        // Add(generate);
        
    }

    protected override void Execute()
    {
        var config = GetRequiredService<LocalConfig>();
        if (config.Remotes.Count == 0)
        {
            LogMessage.AsWarning("No remotes found.");
            return;
        }
        LogMessage.AsInfo("Remotes:");
        foreach (var remote in config.Remotes)
        {
            LogMessage.AsSuccess($"- {remote.Name}: {remote.Url}");
        }
    }

    private void GenerateRemote(ParseResult obj)
    {
        // download from https://raw.githubusercontent.com/talaryonlabs/stackmgr/refs/heads/main/deployment/gen.json
        
        throw new NotImplementedException();
    }
}