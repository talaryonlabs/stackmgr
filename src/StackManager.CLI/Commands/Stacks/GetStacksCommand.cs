using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Types;

namespace Talaryon.StackManager.Commands.Stacks;

/// <summary>
/// Command for listing stacks in an environment.
/// </summary>
public class GetStacksCommand : ResourceGetCommand<Talaryon.StackManager.Types.Stack>
{
    public GetStacksCommand()
        : base("stacks", "List stacks", "Stacks", new EnvironmentOption())
    {
        Aliases.Add("s");
    }

    protected override IReadOnlyList<Talaryon.StackManager.Types.Stack> GetResources(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var files = env.LocalDirectory.GetFiles(Talaryon.StackManager.Types.Stack.FileName, SearchOption.AllDirectories);
        
        return files
            .Select(v => Talaryon.StackManager.Types.Stack.Load(env, v.Directory!.Name))
            .ToList();
    }

    protected override void DisplayResource(Talaryon.StackManager.Types.Stack resource)
    {
        if (!resource.IsDeleted)
        {
            LogMessage.AsSuccess($"- {resource.Name}");
        }
        else
        {
            LogMessage.AsError($"- {resource.Name} (deleted)");
        }
    }
}
