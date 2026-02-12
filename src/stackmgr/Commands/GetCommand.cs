using System.CommandLine;
using stackmgr.Arguments;
using stackmgr.Options;
using stackmgr.Services;

namespace stackmgr.Commands;

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
            new StackArgument()
        };
        apps.SetAction(Get);

        Add(environments);
        Add(stacks);
        Add(apps);
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

        var stack = GetStack<StackArgument>(parseResult, env);
        if (parseResult.CommandResult.Command.Name == "apps")
        {
            GetApps(stack);
        }
    }

    private void GetEnvironments(ParseResult parseResult)
    {
        var environments = Directory
            .GetDirectories(Environment.CurrentDirectory, "*", SearchOption.TopDirectoryOnly)
            .Where(x => Path.GetFileName(x) != ".apps" && Path.GetFileName(x) != ".git")
            .ToList();

        HelperMethods.LogInfo("Environments: ");
        foreach (var env in environments.Where(x => File.Exists(Path.Combine(x, StackEnvironment.FileName))))
        {
            HelperMethods.LogSuccess($"- {Path.GetFileName(env)}");
        }

        foreach (var env in environments.Where(x => !File.Exists(Path.Combine(x, StackEnvironment.FileName))))
        {
            HelperMethods.LogWarning($"- {Path.GetFileName(env)} (not initialized)");
        }
    }

    private async Task GetStacks(StackEnvironment env)
    {
        HelperMethods.LogInfo($"Stacks in environment '{env.Name}': ");

        var directories = env.LocalDirectory.GetDirectories("*", SearchOption.TopDirectoryOnly);
        using var rancher = new RancherService(env);
        using var argo = new ArgoService(env);

        var namespaces = await rancher.GetNamespacesAsync();
        var apps = await argo.GetApplicationsAsync();

        var test = directories
            .Where(v => File.Exists(Path.Combine(v.FullName, Stack.FileName)))
            .Select(v =>
            {
                var stack = Stack.Load(env, v.Name);
                var app = apps.FirstOrDefault(a => a.Metadata.Name == stack.Namespace);
                var ns = namespaces.FirstOrDefault(n => n.Name == stack.Namespace);

                return new[]
                {
                    stack.Name,
                    stack.Namespace,
                    (ns is not null ? "yes" : "no"),
                    (app is not null ? "yes" : "no")
                };
            })
            .Prepend([
                "Name", "Namespace", "Rancher synced?", "ArgoCD synced?"
            ])
            .ToList();

        HelperMethods.PrintTable(test);
    }

    private void GetApps(Stack stack)
    {
        var apps = stack.LocalDirectory
            .GetDirectories("*", SearchOption.TopDirectoryOnly)
            .Select(x => x.Name)
            .ToList();

        HelperMethods.LogInfo($"Apps in stack '{stack.Name}': ");
        foreach (var app in apps)
        {
            HelperMethods.LogSuccess($"- {app}");
        }
    }
}