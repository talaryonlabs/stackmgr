using System.CommandLine;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Types;

namespace Talaryon.StackManager.Commands;

public class MoveCommand : StackManagerCommand
{
    public MoveCommand() : base("move", "Move a resource")
    {
        var stack = new StackManagerCommand("stack", "Move a stack to another environment")
        {
            new StackArgument(),
            new EnvironmentArgument(),
            new EnvironmentOption()
        };
        stack.SetAction(MoveStack);
        Add(stack);
    }

    private async Task MoveStack(ParseResult parseResult)
    {
        var sourceEnvName = parseResult.GetValue<string, EnvironmentOption>()!;
        var stackName = GetName<StackArgument>(parseResult);
        var targetEnvName = GetName<EnvironmentArgument>(parseResult);

        var sourceEnv = StackEnvironment.Load(sourceEnvName);
        var stack = Stack.Load(sourceEnv, stackName);
        var targetEnv = StackEnvironment.Load(targetEnvName);

        var targetStackPath = Path.Combine(targetEnv.LocalDirectory.FullName, stackName, Stack.FileName);
        if (new FileInfo(targetStackPath).Exists)
        {
            throw new InvalidOperationException("A stack with this name already exists in the target environment.");
        }

        LogMessage.AsInfo("Moving stack...");
        var oldNamespace = stack.Namespace;
        stack.Environment = targetEnv;
        stack.Namespace = targetEnv.Name.ToLower() + "-" + stackName.ToLower().Replace(".", "-");

        var sourceStackDir = stack.LocalDirectory;
        var targetStackDir = new DirectoryInfo(Path.Combine(targetEnv.LocalDirectory.FullName, stackName));

        if (!sourceStackDir.Exists)
        {
            throw new InvalidOperationException("Stack directory not found.");
        }

        if (!targetStackDir.Exists)
        {
            targetStackDir.Create();
        }

        foreach (var file in sourceStackDir.GetFiles())
        {
            var targetFile = new FileInfo(Path.Combine(targetStackDir.FullName, file.Name));
            file.MoveTo(targetFile.FullName, true);
        }

        foreach (var dir in sourceStackDir.GetDirectories())
        {
            var targetDir = new DirectoryInfo(Path.Combine(targetStackDir.FullName, dir.Name));
            if (targetDir.Exists)
            {
                targetDir.Delete(true);
            }
            dir.MoveTo(targetDir.FullName);
        }

        stack.SaveConfig();
        sourceStackDir.Delete(true);

        LogMessage.AsSuccess("Stack moved successfully.");
        LogMessage.AsInfo("Old namespace: " + oldNamespace);
        LogMessage.AsInfo("New namespace: " + stack.Namespace);
    }
}
