using System.CommandLine;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Services;
using Talaryon.StackManager.Types;

namespace Talaryon.StackManager.Commands;

public class GetCommand : StackManagerCommand
{
    public GetCommand() : base("get", "Get a resource")
    {
        var environments = new StackManagerCommand("environments", "List environments")
        {
        };
        environments.Aliases.Add("env");
        environments.SetAction(Get);

        var stacks = new StackManagerCommand("stacks", "List stacks")
        {
            new EnvironmentOption()
        };
        stacks.Aliases.Add("s");
        stacks.SetAction(Get);

        var apps = new StackManagerCommand("apps", "List applications")
        {
            new EnvironmentOption(),
            new StackOption()
        };
        apps.SetAction(Get);

        var images = new StackManagerCommand("images", "List images")
        {
            new EnvironmentOption(),
            new StackOption()
        };
        images.SetAction(Get);

        var volumes = new StackManagerCommand("volumes", "List volumes")
        {
            new EnvironmentOption(),
            new StackOption()
        };
        volumes.SetAction(Get);

        var ingresses = new StackManagerCommand("ingresses", "List ingresses")
        {
            new EnvironmentOption(),
            new StackOption()
        };
        ingresses.SetAction(Get);

        Add(environments);
        Add(stacks);
        Add(apps);
        Add(images);
        Add(volumes);
        Add(ingresses);
    }

    private async Task Get(ParseResult parseResult)
    {
        if (parseResult.CommandResult.Command.Name == "environments")
        {
            GetEnvironments(parseResult);
            return;
        }

        var env = GetEnvironment<EnvironmentOption>(parseResult);
        if (parseResult.CommandResult.Command.Name == "stacks")
        {
            await GetStacks(env);
            return;
        }

        var stack = GetStack<StackOption>(parseResult, env);
        switch (parseResult.CommandResult.Command.Name)
        {
            case "apps":
                GetApps(stack);
                return;
            case "images":
                GetImages(stack);
                return;
            case "ingresses":
                GetIngresses(stack);
                return;
            case "volumes":
                GetVolumes(stack);
                return;
        }
    }

    private void GetEnvironments(ParseResult parseResult)
    {
        var root = new DirectoryInfo(Environment.CurrentDirectory);
        var directories = root
            .GetDirectories("*", SearchOption.TopDirectoryOnly)
            .Where(x => x.Name != ".apps" && x.Name != ".git")
            .ToList();

        var uninitialized = directories.Where(v => !File.Exists(Path.Combine(v.FullName, StackEnvironment.FileName)));
        var environments = directories
            .Where(v => File.Exists(Path.Combine(v.FullName, StackEnvironment.FileName)))
            .Select(v => StackEnvironment.Load(v.Name))
            .ToList();

        LogMessage.AsInfo("Environments: ");
        foreach (var env in environments.Where(x => !x.IsDeleted))
        {
            LogMessage.AsSuccess($"- {env.Name}");
        }

        foreach (var env in uninitialized)
        {
            LogMessage.AsWarning($"- {env.Name} (not initialized)");
        }
        
        foreach (var env in environments.Where(x => x.IsDeleted))
        {
            LogMessage.AsError($"- {env.Name} (deleted)");
        }
    }

    private async Task GetStacks(StackEnvironment env)
    {
        LogMessage.AsInfo($"Stacks in environment '{env.Name}': ");

        var files = env.LocalDirectory.GetFiles(Stack.FileName, SearchOption.AllDirectories);
        var stacks = files
            .Select(v => Stack.Load(env, v.Directory!.Name))
            .ToList();
        
        foreach (var stack in stacks.Where(x => !x.IsDeleted))
        {
            LogMessage.AsSuccess($"- {stack.Name}");
        }
        
        foreach (var stack in stacks.Where(x => x.IsDeleted))
        {
            LogMessage.AsError($"- {stack.Name} (deleted)");
        }
    }

    private void GetApps(Stack stack)
    {
        LogMessage.AsInfo($"Apps in stack '{stack.Name}': ");
        foreach (var app in stack.Apps)
        {
            LogMessage.AsSuccess($"- {app.Name}");
        }
    }
    
    private void GetImages(Stack stack)
    {
        LogMessage.AsInfo($"Listing images for stack '{stack.Name}' ...");
        foreach (var image in stack.Images)
        {
            LogMessage.AsSuccess($"- {image.Name}: {image.Image}");
        }
    }
    
    private void GetVolumes(Stack stack)
    {
        LogMessage.AsInfo($"Listing volumes for stack '{stack.Name}' ...");
        foreach (var volume in stack.Volumes)
        {
            LogMessage.AsSuccess($"- {volume.Name}: {volume.StorageSize} ({volume.AccessMode})");
        }
    }
    
    private void GetIngresses(Stack stack)
    {
        LogMessage.AsInfo($"Ingresses in stack '{stack.Name}': ");
        foreach (var ingress in stack.Ingresses)
        {
            LogMessage.AsSuccess($"- {ingress.Hostname} [{ingress.Application ?? ingress.Redirect}]");
        }
    }
}