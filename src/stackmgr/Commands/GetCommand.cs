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
            await GetEnvironments(parseResult);
            return;
        }
        
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        if (parseResult.CommandResult.Command.Name == "stacks")
        {
            await GetStacks(parseResult, env);
            return;
        }
        
        var stack = GetStack<StackArgument>(parseResult, env);
        if (parseResult.CommandResult.Command.Name == "apps")
        {
            await GetApps(parseResult, stack);
        }
    }

    private async Task GetEnvironments(ParseResult parseResult)
    {
        
    }
    
    private async Task GetStacks(ParseResult parseResult, StackEnvironment env)
    {
        Console.WriteLine($"Listing stacks for {env.Name}");

        var apps = await Argo.ListApplicationsAsync(env);

        var test = apps.Select(app =>
        {
            return new []
            {
                app.Metadata.Name,
                app.Spec.Project,
                app.Spec.Source.Path
            };
        }).ToList();
            
        test.Insert(0, new [] {"Name", "Project", "Path"});
            
        HelperMethods.PrintTable(test);

        return;
            
        var path = Path.Combine(Environment.CurrentDirectory, env.Name.ToLower());
        foreach (var stack in Directory.GetDirectories(path))
        {
            Console.WriteLine(Path.GetFileName(stack));
        }
    }
    
    private async Task GetApps(ParseResult parseResult, Stack stack)
    {
        
    }
}