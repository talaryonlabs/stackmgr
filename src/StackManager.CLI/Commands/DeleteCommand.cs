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
        Add(volume);       
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
            DeleteStack(parseResult, env);
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
            case "volume":
                DeleteVolume(parseResult, stack);
                return;
        }
    }

    private void DeleteEnvironment(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentArgument>(parseResult);
        LogMessage.AsWarning("ATTENTION: This only marks the environment 'deleted' in the config file.");
        LogMessage.AsWarning("It does not delete the environment from Rancher/ArgoCD nor the local directory.");
        LogMessage.AsWarning("");
        
        if (!LogMessage.AsConfirmWarning($"Are you sure you want to delete environment '{env.Name}'?"))
        {
            LogMessage.AsInfo("Aborted.");
            return;
        }
        LogMessage.AsInfo($"Removing environment '{env.Name}'.");
        env.IsDeleted = true;
        env.SaveConfig();
        LogMessage.AsSuccess("Success.");
    }
    
    private void DeleteStack(ParseResult parseResult, StackEnvironment env)
    {
        var stack = GetStack<StackArgument>(parseResult, env);
        LogMessage.AsWarning("ATTENTION: This will also delete all applications in the stack.");
        LogBuilder.Question($"Do you really want to delete stack '{stack.Name}'?")
            .AsYesNo()
            .AsWarning()
            .NoNewLineAfter()
            .WaitFor(result =>
            {
                if (!result) return LogBuilder.Message("Aborted.");
                stack.Delete();
                return LogBuilder.Message($"Stack '{stack.Name}' marked for deletion. Call 'stackmgr sync' to delete it.")
                    .AsSuccess();
            })
            .Run();
    }

    private void DeleteApp(ParseResult parseResult, Stack stack)
    {
        var app = GetApp<AppArgument>(parseResult, stack);
        LogBuilder.Question($"Are you sure you want to delete app '{app.Name}' in stack '{stack.Name}'? ({stack.Environment.Name})")
            .AsYesNo()
            .AsWarning()
            .NoNewLineAfter()
            .WaitFor(result =>
            {
                if (!result) return LogBuilder.Message("Aborted.");
                app.Delete();
                return LogBuilder.Message("Done.").AsSuccess();
            })
            .Run();
    }
    
    private void DeleteImage(ParseResult parseResult, Stack stack)
    {
        var image = GetImage<ImageArgument>(parseResult, stack);
        LogBuilder.Question($"Are you sure you want to delete image '{image.Name}' in stack '{stack.Name}'? ({stack.Environment.Name})")
            .AsYesNo()
            .AsWarning()
            .NoNewLineAfter()
            .WaitFor(result =>
            {
                if (!result) return LogBuilder.Message("Aborted.");
                image.Delete();
                return LogBuilder.Message("Done.").AsSuccess();
            })
            .Run();
    }
    
    private void DeleteVolume(ParseResult parseResult, Stack stack)
    {
        var volume = GetVolume<VolumeArgument>(parseResult, stack);
        LogBuilder.Question($"Do you really want to delete volume '{volume.Name}'?")
            .AsYesNo()
            .AsWarning()
            .NoNewLineAfter()
            .WaitFor(result =>
            {
                if (!result) return LogBuilder.Message("Aborted.");
                volume.Delete();
                return LogBuilder.Message("Done.").AsSuccess();
            })
            .Run();
    }

    private void DeleteIngress(ParseResult parseResult, Stack stack)
    {
        var ingress = GetIngress<HostnameArgument>(parseResult, stack);
        LogBuilder.Question($"Do you really want to delete ingress '{ingress.Hostname}'?")
            .AsYesNo()
            .AsWarning()
            .NoNewLineAfter()
            .WaitFor(result =>
            {
                if (!result) return LogBuilder.Message("Aborted.");
                ingress.Delete();
                return LogBuilder.Message("Done.").AsSuccess();
            })
            .Run();
    }
}