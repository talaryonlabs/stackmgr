using System.CommandLine;
using stackmgr.Arguments;
using stackmgr.Options;
using stackmgr.Services;
using Talaryon.Toolbox.Services.ArgoCD.Models;

namespace stackmgr.Commands;

public class SyncCommand : StackManagerCommand
{
    public SyncCommand() : base("sync", "Sync a stack")
    {
        Add(new EnvironmentOption());
        Add(new StackArgument());
        SetAction(SyncStack);
    }
    
    private async Task SyncStack(ParseResult parseResult)
    {
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var stack = GetStack<StackArgument>(parseResult, env);

        var ns = await GetOrCreateNamespaceAsync(stack);
        var application = await GetOrCreateApplicationAsync(stack);
            
        if (ns is not null && application is not null)
        {
            stack.Application = application;
            await SetAutoSyncSettingAsync(stack);
        }
    }
    
    private async Task<string?> GetOrCreateNamespaceAsync(Stack stack)
    {
        if (!await Rancher.NamespaceExists(stack.Environment, stack.Namespace))
        {
            Console.Write($".. Creating RKE2 namespace '{stack.Namespace}' .. ");
            await Rancher.CreateNamespace(stack.Environment, stack.Namespace);
            Console.WriteLine("Done.");
        }
        else
        {
            Console.WriteLine(".. RKE2 namespace already exists. (Nothing to do)");
        }
        return stack.Namespace;
    }

    private async Task<V1alpha1Application?> GetOrCreateApplicationAsync(Stack stack)
    {
        var application = await Argo.GetApplication(stack.Environment, stack.Namespace);
        if (application is not null)
        {
            Console.Write(".. Creating ArgoCD application .. ");
            if (await Argo.CreateApplication(stack.Environment, stack.Namespace))
            {
                Console.WriteLine("Done.");
            }
        }
        else
        {
            Console.WriteLine(".. ArgoCD application already exists. (Nothing to do)");
        }
        return application;
    }

    private async Task SetAutoSyncSettingAsync(Stack stack)
    {
        if (stack.Application?.Spec.SyncPolicy is not null)
        {
            await Argo.EnableAutoSync(stack.Environment, stack.Namespace);
        }
        else
        {
            await Argo.DisableAutoSync(stack.Environment, stack.Namespace);
        }
    }
}