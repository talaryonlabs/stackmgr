using System.CommandLine;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Services;
using Talaryon.StackManager.Types;

namespace Talaryon.StackManager.Commands;

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
            new StackOption(),
            new AppArgument()
        };
        app.SetAction(Delete);

        var image = new StackManagerCommand("image", "Delete an image")
        {
            new EnvironmentOption(),
            new StackOption(),
            new ImageArgument()
        };
        image.Aliases.Add("i");
        image.SetAction(Delete);
        
        var volume = new StackManagerCommand("volume", "Delete a volume")
        {
            new EnvironmentOption(),
            new StackOption(),
            new VolumeArgument()
        };
        volume.SetAction(Delete);
        
        var ingress = new StackManagerCommand("ingress", "Delete an ingress")
        {
            new EnvironmentOption(),
            new StackOption(),
            new HostnameArgument()
        };
        ingress.SetAction(Delete);
        
        Add(env);
        Add(stack);
        Add(app);
        Add(image);
        Add(ingress);
    }
    
    private async Task Delete(ParseResult parseResult)
    {
        
        if (parseResult.CommandResult.Command.Name == "environment")
        {
            DeleteEnvironment(parseResult);
            return;
        }
        
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        if (parseResult.CommandResult.Command.Name == "stack")
        {
            await DeleteStack(parseResult, env);
            return;
        }
        
        var stack = GetStack<StackOption>(parseResult, env);
        switch (parseResult.CommandResult.Command.Name)
        {
            case "app":
                DeleteApp(parseResult, stack);
                return;
            case "image":
                DeleteImage(parseResult, stack);
                return;
            case "ingress":
                DeleteIngress(parseResult, stack);
                return;
        }
    }

    private void DeleteIngress(ParseResult parseResult, Stack stack)
    {
        var hostname = parseResult.GetRequiredValue<string, HostnameArgument>();
        var ingress = stack.Ingresses.FirstOrDefault(x => x.Hostname.Equals(hostname, StringComparison.CurrentCultureIgnoreCase));
        if (ingress is null)
        {
            throw new Exception($"Ingress with hostname '{hostname}' does not exist in stack '{stack.Name}'.");
        }
        ingress.Delete();

        stack
            .Redirects
            .Where(v => !stack.Ingresses.Any(x =>
                (x.Redirect ?? "").Equals(v.Hostname, StringComparison.InvariantCultureIgnoreCase)))
            .ToList()
            .ForEach(v => v.Delete());
        
        HelperMethods.LogSuccess($"Ingress '{hostname}' deleted.");
    }

    private void DeleteEnvironment(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentArgument>(parseResult);
        HelperMethods.LogWarning("ATTENTION: This only marks the environment 'deleted' in the config file.");
        HelperMethods.LogWarning("It does not delete the environment from Rancher/ArgoCD nor the local directory.");
        HelperMethods.LogWarning("");
        
        if (!HelperMethods.ConfirmWarning($"Are you sure you want to delete environment '{env.Name}'?"))
        {
            HelperMethods.LogInfo("Aborted.");
            return;
        }
        HelperMethods.LogInfo($"Removing environment '{env.Name}'.");
        env.IsDeleted = true;
        env.SaveConfig();
        HelperMethods.LogSuccess("Success.");
    }
    
    private async Task DeleteStack(ParseResult parseResult, StackEnvironment env)
    {
        var stack = GetStack<StackArgument>(parseResult, env);
        using var argo = new ArgoService(env);
        using var rancher = new RancherService(env);
        
        HelperMethods.LogWarning("ATTENTION: This will also delete all applications in the stack.");
        if (!HelperMethods.ConfirmWarning($"Are you sure you want to delete stack '{stack.Name}' in environment '{env.Name}'?"))
        {
            HelperMethods.LogInfo("Aborted.");
            return;
        }
        HelperMethods.LogWarning($"Deleting stack '{stack.Name}' in environment '{env.Name}':");

        HelperMethods.LogInfo(".. Deleting ArgoCD application ...");
        await argo.DeleteApplicationAsync(stack);
        
        HelperMethods.LogInfo(".. Deleting Rancher namespace ...");
        await rancher.DeleteNamespaceAsync(stack);
        
        HelperMethods.LogInfo(".. Deleting local directory ...");
        stack.LocalDirectory.Delete(true);
        
        HelperMethods.LogSuccess("Done.");
    }

    private void DeleteApp(ParseResult parseResult, Stack stack)
    {
        var app = GetApp<AppArgument>(parseResult, stack);

        if (!HelperMethods.ConfirmWarning($"Are you sure you want to delete app '{app.Name}' in stack '{stack.Name}'? ({stack.Environment.Name})"))
        {
            HelperMethods.LogInfo("Aborted.");
            return;
        }

        app.Delete();
        HelperMethods.LogSuccess($"App '{app.Name}' deleted.");
    }
    
    private void DeleteImage(ParseResult parseResult, Stack stack)
    {
        var image = parseResult.GetRequiredValue<string, ImageArgument>();
        
        var local = stack.Images.FirstOrDefault(x => x.Name.Equals(image, StringComparison.CurrentCultureIgnoreCase));
        if (local is null)
        {
            HelperMethods.LogWarning($"Image '{image}' not found in stack '{stack.Name}' (environment '{stack.Environment.Name}').");
            return;
        }
        local.Delete();
        
        HelperMethods.LogSuccess($"Image '{image}' deleted.");
    }
}