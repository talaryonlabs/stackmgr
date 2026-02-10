using System.CommandLine;
using stackmgr.Arguments;
using stackmgr.Options;
using stackmgr.Services;

namespace stackmgr.Commands;

public class DeleteCommand : StackManagerCommand
{
    public DeleteCommand() : base("delete", "Delete a resource (environment, stack, app)")
    {
        var env = new StackManagerCommand("environment", "Delete a environment")
        {
            new EnvironmentArgument()
        };
        env.Aliases.Add("env");
        env.SetAction(Delete);
        
        var stack = new StackManagerCommand("stack", "Delete a stack")
        {
            new EnvironmentOption(),
            new StackArgument()
        };
        stack.Aliases.Add("s");
        stack.SetAction(Delete);

        var app = new StackManagerCommand("app", "Delete a application")
        {
            new EnvironmentOption(),
            new StackArgument(),
            new AppArgument()
        };
        app.SetAction(Delete);
        
        Add(env);
        Add(stack);
        Add(app);
    }
    
    private async Task Delete(ParseResult v)
    {
        
        if (v.CommandResult.Command.Name == "environment")
        {
            DeleteEnvironment(v);
            return;
        }
        
        var env = GetEnvironment<EnvironmentOption>(v);
        if (v.CommandResult.Command.Name == "stack")
        {
            await DeleteStack(v, env);
            return;
        }
        
        var stack = GetStack<StackArgument>(v, env);
        if (v.CommandResult.Command.Name == "app")
        {
            await DeleteApp(v, stack);
        }
    }

    private void DeleteEnvironment(ParseResult v)
    {
        var env = GetEnvironment<EnvironmentArgument>(v);
        HelperMethods.LogWarning("ATTENTION: This only deletes the environment from the stackmgr config.");
        HelperMethods.LogWarning("It does not delete the environment from Rancher/ArgoCD nor the local directory.");
        HelperMethods.LogWarning("");
        
        if (!HelperMethods.ConfirmWarning($"Are you sure you want to delete environment '{env.Name}'?"))
        {
            HelperMethods.LogInfo("Aborted.");
            return;
        }
        HelperMethods.LogInfo($"Removing environment '{env.Name}'.");
                
        Config.Environments.Remove(env);
        Config.Save();
        HelperMethods.LogSuccess("Success.");
    }
    
    private async Task DeleteStack(ParseResult v, StackEnvironment env)
    {
        var stack = GetStack<StackArgument>(v, env);
        
        HelperMethods.LogWarning("ATTENTION: This will also delete all applications in the stack.");
        if (!HelperMethods.ConfirmWarning($"Are you sure you want to delete stack '{stack.Name}' in environment '{env.Name}'?"))
        {
            HelperMethods.LogInfo("Aborted.");
            return;
        }
        HelperMethods.LogWarning($"Deleting stack '{stack.Name}' in environment '{env.Name}':");

        await Argo.DeleteApplication(env, stack.Name);
        await Rancher.DeleteNamespace(env, stack.Namespace);
            
        stack.LocalDirectory.Delete(true);
        HelperMethods.LogSuccess("Done.");
    }

    private async Task DeleteApp(ParseResult v, Stack stack)
    {
        var app = GetApp<AppArgument>(v, stack);

        if (!HelperMethods.ConfirmWarning($"Are you sure you want to delete app '{app.Name}' in stack '{stack.Name}'? ({stack.Environment.Name})"))
        {
            HelperMethods.LogInfo("Aborted.");
            return;
        }

        stack.Apps.Remove(app);
        stack.SaveConfig();
        stack.SaveKustomization();
        
        var path = Path.Combine(stack.LocalDirectory.FullName, app.Name);
        Directory.Delete(path, true);
        HelperMethods.LogSuccess($"App '{app.Name}' deleted.");
    }
}