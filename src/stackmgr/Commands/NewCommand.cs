using System.CommandLine;
using stackmgr.Arguments;
using stackmgr.Exceptions;
using stackmgr.Options;
using stackmgr.Services;

namespace stackmgr.Commands;

public class NewCommand : StackManagerCommand
{
    public NewCommand() : base("new", "Create a new resource (environment, stack, app)")
    {
        var env = new StackManagerCommand("environment", "Create a new environment")
        {
            new EnvironmentArgument()
        };
        env.Aliases.Add("env");
        env.SetAction(New);
        
        var stack = new StackManagerCommand("stack", "Create a new stack")
        {
            new EnvironmentOption(),
            new StackArgument()
        };
        stack.Aliases.Add("s");
        stack.SetAction(New);

        var app = new StackManagerCommand("app", "Create a new application")
        {
            new EnvironmentOption(),
            new StackArgument(),
            new AppArgument(),
            new TemplateOption(),
            new DevOption(),
            new VolumeOption(),
            new ConfigOption(),
            new HostOption(),
            new WithoutIngressOption()
        };
        app.SetAction(New);
        
        Add(env);
        Add(stack);
        Add(app);
    }

    private async Task New(ParseResult v)
    {
        if (v.CommandResult.Command.Name == "environment")
        {
            NewEnvironment(v);
            return;
        }
        
        var env = GetEnvironment<EnvironmentOption>(v);
        if (v.CommandResult.Command.Name == "stack")
        {
            NewStack(v, env);
            return;
        }
        
        var stack = GetStack<StackArgument>(v, env);
        if (v.CommandResult.Command.Name == "app")
        {
            await NewApp(v, stack);
        }
    }

    private void NewEnvironment(ParseResult v)
    {
        var name = GetEnvironmentName<EnvironmentArgument>(v);
        var env = new StackEnvironment { Name = name };

        if (!env.LocalDirectory.Exists)
        {
            env.LocalDirectory.Create();
            HelperMethods.LogSuccess($"Directory '{env.LocalDirectory.FullName}' created.");
        }
            
        if (Config.Environments.Any(x => x.Name.Equals(env.Name, StringComparison.CurrentCultureIgnoreCase)))
        {
            HelperMethods.LogWarning($"Environment '{env.Name}' already exists.");
            return;
        }
            
        HelperMethods.LogInfo($"Initializing environment '{env.Name}' ...");
        Config.Environments.Add(env);
        Config.Save();
        HelperMethods.LogSuccess("Success.");
    }
    
    private void NewStack(ParseResult v, StackEnvironment env)
    {
        var name = GetStackName<StackArgument>(v);
        var stack = Stack.New(env, name);

        if (stack.LocalDirectory.Exists)
        {
            throw new StackAlreadyExistsException(stack);
        }

        HelperMethods.LogInfo($"Creating stack '{stack.Name}' in environment '{env.Name}'.");
        if (!stack.LocalDirectory.Exists)
        {
            HelperMethods.LogError("Failed.");
            return;
        }
        stack.LocalDirectory.Create();
        stack.SaveConfig();
        stack.SaveKustomization();
            
        HelperMethods.LogSuccess("Done.");
    }

    private async Task NewApp(ParseResult v, Stack stack)
    {
        var name = GetAppName<AppArgument>(v);
        try
        {
            var a = GetApp<AppArgument>(v, stack);
            throw new AppAlreadyExistsException(stack, a);
        }
        catch (AppNotFoundException) { }

        var path = Path.Combine(stack.LocalDirectory.FullName, name);
        var dir = new DirectoryInfo(path);
            
        if(!dir.Exists) dir.Create();

        var template = v.GetValue<string, TemplateOption>();
        var branch = v.GetValue<bool, DevOption>() ? "dev" : "prod";
        var app = new StackApp()
        {
            Name = name,
            Volume = v.GetValue<string, VolumeOption>() ?? "",
            Host = v.GetValue<string, HostOption>() ?? "",
            Template = template is not null ? $"{branch}:{template}" : "",
            Config = (v.GetValue<string[], ConfigOption>() ?? []).Select(x =>
            {
                var config = x.Split("=");
                return new StackAppConfig
                {
                    Name = config[0].Trim(),
                    Value = config.Length > 1 ? config[1].Trim() : ""
                };
            }).ToList()
        };
        
        if (template is not null)
        {
            var options = new AppServiceOptions
            {
                WithoutIngress = v.GetValue<bool, WithoutIngressOption>()
            };
            var appService = new AppService(stack, app);
            await appService.Install(options);
            
            HelperMethods.LogSuccess("Installation done.");
        }
        
        stack.Apps.Add(app);
        stack.SaveConfig();
        stack.SaveKustomization();
        
        HelperMethods.LogSuccess("App created.");
    }
}