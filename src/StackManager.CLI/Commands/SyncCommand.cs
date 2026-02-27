using System.CommandLine;
using System.Net;
using StackManager.Shared.Models;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Services;
using Talaryon.StackManager.Types;
using Talaryon.Toolbox;
using Talaryon.Toolbox.Api;

namespace Talaryon.StackManager.Commands;

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

        var proxy = new ProxyService(env);

        
        try
        {
            var ns1 = await proxy.GetNamespaceAsync("my-test-namespace");
        }
        catch (ApiError e)
        {
            if (e.Code == (int)HttpStatusCode.NotFound)
            {
                await proxy.CreateNamespaceAsync("my-test-namespace");
            }
            Console.WriteLine(e);
            throw;
        }
        
        
        return;
        
        var argo = new ArgoService(env);
        var rancher = new RancherService(env);
        
        
        var ns = await GetOrCreateNamespaceAsync(stack, proxy);
        var application = await GetOrCreateApplicationAsync(stack, proxy);
            
        // if (ns is not null && application is not null)
        // {
        //     stack.Application = application;
        //     await SetAutoSyncSettingAsync(stack, argo);
        //     await argo.RefreshApplicationAsync(stack);
        // }

        if (ns is not null)
        {
            await SyncStackVolumes(stack, proxy);
        }
        
        argo.Dispose();
        rancher.Dispose();
    }
    
    private async Task<string?> GetOrCreateNamespaceAsync(Stack stack, IProxyService proxy)
    {
        var ns = await proxy.GetNamespaceAsync(stack.Namespace);
        if (ns is null)
        {
            LogMessage.AsInfo($".. Creating RKE2 namespace '{stack.Namespace}' .. ");
            await proxy.CreateNamespaceAsync(stack.Namespace);
            LogMessage.AsSuccess("Done.");
        }
        else
        {
            LogMessage.AsInfo(".. RKE2 namespace already exists. (Nothing to do)");
        }
        return stack.Namespace;
    }

    private async Task<Application?> GetOrCreateApplicationAsync(Stack stack, IProxyService proxy)
    {
        var application = await proxy.GetApplicationAsync(stack.Namespace);
        if (application is null)
        {
            LogMessage.AsInfo(".. Creating ArgoCD application .. ");
            // if (await proxy.CreateApplicationAsync(stack) is not null)
            // {
            //     LogMessage.AsSuccess("Done.");
            // }
        }
        else
        {
            LogMessage.AsInfo(".. ArgoCD application already exists. (Nothing to do)");
        }
        return application;
    }

    private async Task SetAutoSyncSettingAsync(Stack stack, ArgoService argo)
    {
        LogMessage.AsInfo(".. Setting auto-sync setting .. ");
        if (stack.Application?.Spec.SyncPolicy is null && stack.EnableAutoSync)
        {
            await argo.SetAutoSyncAsync(stack, true);
        }
        else if (!stack.EnableAutoSync)
        {
            await argo.SetAutoSyncAsync(stack, false);
        }
        LogMessage.AsSuccess("Done.");
    }

    private async Task SyncStackVolumes(Stack stack, IProxyService proxy)
    {
        var remote = await proxy.GetVolumesAsync();
        var local = stack.Volumes;
        
        foreach(var volume in local.IntersectBy(remote.Select(v => v.Name), v => v.Name))
        {
            LogMessage.AsInfo($"Volume '{volume.Name}' already exists in remote.");
        }
        
        foreach(var volume in local.ExceptBy(remote.Select(v => v.Name), v => v.Name))
        {
            await LogBuilder
                .Message($"Creating volume '{volume.Name}' in remote ... ")
                .WaitFor(async () =>
                {
                    await proxy.CreateVolumeAsync(new Volume
                    {
                        Name = volume.Name,
                        AccessMode = volume.AccessMode,
                        Size = (long)TalaryonHelper.ParseNamedSize(volume.StorageSize)
                    });
                    
                    return LogBuilder.Message("Volume created successfully.").AsSuccess();
                })
                .RunAsync();
        }

        foreach(var volume in remote.ExceptBy(local.Select(v => v.Name), v => v.Name))
        {
            await LogBuilder
                .Message($"Deleting volume '{volume.Name}' from remote ... ")
                .WaitFor(async () =>
                {
                    await proxy.DeleteVolumeAsync(volume.Name);
                    
                    return LogBuilder.Message("Volume deleted successfully.").AsSuccess();
                })
                .RunAsync();
        }
        // var ignore = volumes.Uni(stack.Volumes.Select(v => v.Name), v => v.Name);
        //
        // foreach (var volume in stack.Volumes.Where(x => volumes.All(y => y.Name != x.Name)))
        // {
        //     await proxy.CreateVolumeAsync(volume);
        // }
        //
        // foreach (var volume in volumes.Where(x => stack.Volumes.All(y => y.Name != x.Name)))
        // {
        //     await proxy.DeleteVolumeAsync(volume.Name);
        // }
    }
}