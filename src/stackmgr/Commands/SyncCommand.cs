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
        var argo = new ArgoService(env);
        var rancher = new RancherService(env);

        var ns = await GetOrCreateNamespaceAsync(stack, rancher);
        var application = await GetOrCreateApplicationAsync(stack, argo);
            
        if (ns is not null && application is not null)
        {
            stack.Application = application;
            await SetAutoSyncSettingAsync(stack, argo);
        }
        argo.Dispose();
        rancher.Dispose();
    }
    
    private async Task<string?> GetOrCreateNamespaceAsync(Stack stack, RancherService rancher)
    {
        var ns = await rancher.GetNamespaceAsync(stack);
        if (ns is null)
        {
            HelperMethods.LogInfo($".. Creating RKE2 namespace '{stack.Namespace}' .. ");
            await rancher.CreateNamespaceAsync(stack);
            HelperMethods.LogSuccess("Done.");
        }
        else
        {
            HelperMethods.LogInfo(".. RKE2 namespace already exists. (Nothing to do)");
        }
        return stack.Namespace;
    }

    private async Task<V1alpha1Application?> GetOrCreateApplicationAsync(Stack stack, ArgoService argo)
    {
        var application = await argo.GetApplicationAsync(stack);
        if (application is null)
        {
            HelperMethods.LogInfo(".. Creating ArgoCD application .. ");
            if (await argo.CreateApplicationAsync(stack) is not null)
            {
                HelperMethods.LogSuccess("Done.");
            }
        }
        else
        {
            HelperMethods.LogInfo(".. ArgoCD application already exists. (Nothing to do)");
        }
        return application;
    }

    private async Task SetAutoSyncSettingAsync(Stack stack, ArgoService argo)
    {
        HelperMethods.LogInfo(".. Setting auto-sync setting .. ");
        if (stack.Application?.Spec.SyncPolicy is null && stack.EnableAutoSync)
        {
            await argo.SetAutoSyncAsync(stack, true);
        }
        else if (!stack.EnableAutoSync)
        {
            await argo.SetAutoSyncAsync(stack, false);
        }
        HelperMethods.LogSuccess("Done.");
    }
}