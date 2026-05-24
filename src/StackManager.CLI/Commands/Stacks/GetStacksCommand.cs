using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;

namespace Talaryon.StackManager.Commands.Stacks;

/// <summary>
/// Command for listing stacks in an environment.
/// </summary>
public class GetStacksCommand : ResourceGetCommand<Stack>
{
    public GetStacksCommand()
        : base("stacks", "List stacks", "Stacks", new EnvironmentOption())
    {
        Aliases.Add("s");
    }

    protected override IReadOnlyList<Stack> GetResources(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var files = env.LocalDirectory.GetFiles(Stack.FileName, SearchOption.AllDirectories);
        
        return files
            .Select(v => env.GetStack(v.Directory!.Name))
            .ToList();
    }

    protected override void DisplayResource(Stack resource)
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
