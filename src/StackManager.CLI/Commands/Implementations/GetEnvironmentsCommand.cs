using System;
using System.CommandLine;
using System.IO;
using Talaryon.StackManager.Commands.Base;
using Talaryon.StackManager.Types;

namespace Talaryon.StackManager.Commands.Implementations;

/// <summary>
/// Command for listing environments.
/// Uses ResourceGetCommand with no options since environments are top-level.
/// </summary>
public class GetEnvironmentsCommand : ResourceGetCommand<StackEnvironment>
{
    public GetEnvironmentsCommand() 
        : base("environments", "List environments", "Environments")
    {
        Aliases.Add("env");
    }

    protected override IReadOnlyList<StackEnvironment> GetResources(ParseResult parseResult)
    {
        var root = new DirectoryInfo(System.Environment.CurrentDirectory);
        var directories = root
            .GetDirectories("*", SearchOption.TopDirectoryOnly)
            .Where(x => x.Name != ".apps" && x.Name != ".git")
            .ToList();

        return directories
            .Where(v => File.Exists(Path.Combine(v.FullName, StackEnvironment.FileName)))
            .Select(v => StackEnvironment.Load(v.Name))
            .ToList();
    }

    protected override void DisplayResource(StackEnvironment resource)
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

    protected override void DisplayResources(IReadOnlyList<StackEnvironment> resources)
    {
        var root = new DirectoryInfo(System.Environment.CurrentDirectory);
        var directories = root
            .GetDirectories("*", SearchOption.TopDirectoryOnly)
            .Where(x => x.Name != ".apps" && x.Name != ".git")
            .ToList();

        var uninitialized = directories
            .Where(v => !File.Exists(Path.Combine(v.FullName, StackEnvironment.FileName)))
            .ToList();
        
        if (resources.Count == 0 && uninitialized.Count == 0)
        {
            LogMessage.AsWarning("No environments found.");
            return;
        }

        LogMessage.AsInfo("Environments: ");
        foreach (var env in resources.Where(x => !x.IsDeleted))
        {
            LogMessage.AsSuccess($"- {env.Name}");
        }

        foreach (var env in uninitialized)
        {
            LogMessage.AsWarning($"- {env.Name} (not initialized)");
        }
        
        foreach (var env in resources.Where(x => x.IsDeleted))
        {
            LogMessage.AsError($"- {env.Name} (deleted)");
        }
    }
}
