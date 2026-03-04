using System.CommandLine;
using StackManager.Shared.Models;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Services;
using Talaryon.StackManager.Types;
using Talaryon.Toolbox;

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

        using var proxy = new ProxyService(env);

        if (stack.IsDeleted)
        {
            await DeleteStackFromRemote(stack, proxy);
            return;
        }
        
        var ns = await SyncNamespaceWithRemote(stack, proxy);
        var application = await SyncApplicationWithRemote(stack, proxy);

        if (ns is not null)
        {
            await SyncStackVolumes(stack, proxy);
        }
    }

    private async Task DeleteStackFromRemote(Stack stack, IProxyService proxy)
    {
        var volumes = await proxy.GetVolumesAsync(stack.Namespace);
        var application = await proxy.GetApplicationAsync(stack.Namespace);
        var ns = await proxy.GetNamespaceAsync(stack.Namespace);
        var error = false;

        LogMessage.AsInfo($"Deleting stack '{stack.Name}' from remote.");
        foreach (var volume in volumes)
        {
            await LogBuilder.Message($"- [Volume] {volume.Name} ... ")
                .NoNewLineAfter()
                .WaitFor(async () =>
                {
                    if (await proxy.DeleteVolumeAsync(stack.Namespace, volume.Name) is null)
                    {
                        error = true;
                        return LogBuilder.Message("Failed.").AsError();
                    }
                    return LogBuilder.Message("Done.").AsSuccess();
                })
                .RunAsync();
        }
        
        if (application is not null)
        {
            await LogBuilder.Message($"- [Application] {stack.Namespace} ... ")
                .NoNewLineAfter()
                .WaitFor(async () =>
                {
                    if (await proxy.DeleteApplicationAsync(stack.Namespace) is null)
                    {
                        error = true;
                        return LogBuilder.Message("Failed.").AsError();
                    }
                    return LogBuilder.Message("Done.").AsSuccess();
                })
                .RunAsync();
        }
        
        if (ns is not null)
        {
            await LogBuilder.Message($"- [Namespace] {stack.Namespace} ... ")
                .NoNewLineAfter()
                .WaitFor(async () =>
                {
                    if (await proxy.DeleteNamespaceAsync(stack.Namespace) is null)
                    {
                        error = true;
                        return LogBuilder.Message("Failed.").AsError();
                    }
                    return LogBuilder.Message("Done.").AsSuccess();
                })
                .RunAsync();
        }

        if (error)
        {
            throw new Exception("Stack deletion not completed. Please try again.");
        }
        stack.Delete(true);
        LogMessage.AsSuccess($"Stack '{stack.Name}' deleted successfully.");
    }

    private async Task<Namespace?> SyncNamespaceWithRemote(Stack stack, IProxyService proxy)
    {
        var ns = default(Namespace);
        await LogBuilder.Message($"- [Namespace] {stack.Namespace} ... ")
            .NoNewLineAfter()
            .WaitFor(async () =>
            {
                if ((ns = await proxy.GetNamespaceAsync(stack.Namespace)) is not null)
                    return LogBuilder.Message("Already exists.").AsWarning();

                return (ns = await proxy.CreateNamespaceAsync(stack.Namespace)) is not null
                    ? LogBuilder.Message("Created.").AsSuccess()
                    : LogBuilder.Message("Failed.").AsError();
            })
            .RunAsync();
        return ns;
    }

    private async Task<Application?> SyncApplicationWithRemote(Stack stack, IProxyService proxy)
    {
        var application = default(Application);
        await LogBuilder.Message($"- [Application] {stack.Namespace} ... ")
            .NoNewLineAfter()
            .WaitFor(async () =>
            {
                if ((application = await proxy.GetApplicationAsync(stack.Namespace)) is not null)
                {
                    if (application.Path != $"{stack.Environment.Name}/{stack.Name}" ||
                        application.Repository != stack.Environment.Repository ||
                        application.IsAutoSyncEnabled != stack.EnableAutoSync)
                    {
 
                        
                        Console.WriteLine($"{stack.EnableAutoSync} => {application.IsAutoSyncEnabled}");
                        Console.WriteLine($"{stack.Environment.Name}/{stack.Name} => {application.Path}");
                        Console.WriteLine($"{stack.Environment.Repository} => {application.Repository}");
                        
                        application.IsAutoSyncEnabled = stack.EnableAutoSync;
                        application.Path = $"{stack.Environment.Name}/{stack.Name}";
                        application.Repository = stack.Environment.Repository;

                        return await proxy.UpdateApplicationAsync(stack.Namespace, application) is not null
                            ? LogBuilder.Message("Update succeeded.").AsSuccess()
                            : LogBuilder.Message("Update failed.").AsError();
                    }

                    return LogBuilder.Message("Up to date.").AsWarning();
                }

                var newApp = new Application
                {
                    Name = stack.Namespace,
                    IsAutoSyncEnabled = false,
                    Path = $"{stack.Environment.Name}/{stack.Name}",
                    Repository = stack.Environment.Repository
                };
                
                return (application = await proxy.CreateApplicationAsync(newApp)) is not null
                    ? LogBuilder.Message("Created.").AsSuccess()
                    : LogBuilder.Message("Failed.").AsError();
            })
            .RunAsync();
        return application;
    }

    private async Task SyncStackVolumes(Stack stack, IProxyService proxy)
    {
        var remote = await proxy.GetVolumesAsync(stack.Namespace);
        var local = stack.Volumes;
        
        foreach(var volume in local.IntersectBy(remote.Select(v => v.Name), v => v.Name))
        {
            await LogBuilder.Message($"- [Volume] {volume.Name} ... ")
                .NoNewLineAfter()
                .WaitFor(() => LogBuilder.Message("Exists.").AsWarning())
                .RunAsync();
        }
        
        foreach(var volume in local.ExceptBy(remote.Select(v => v.Name), v => v.Name))
        {
            await LogBuilder
                .Message($"- [Volume] Creating {volume.Name} ... ")
                .NoNewLineAfter()
                .WaitFor(async () =>
                {
                    var v = await proxy.CreateVolumeAsync(stack.Namespace, new Volume
                    {
                        Name = volume.Name,
                        AccessMode = volume.AccessMode,
                        Size = volume.StorageSize
                    });
                    return v is null
                        ? LogBuilder.Message("Failed.").AsError()
                        : LogBuilder.Message("Done.").AsSuccess();
                })
                .RunAsync();
        }

        foreach(var volume in remote.ExceptBy(local.Select(v => v.Name), v => v.Name))
        {
            await LogBuilder
                .Message($"- [Volume] Deleting {volume.Name} ... ")
                .NoNewLineAfter()
                .WaitFor(async () => await proxy.DeleteVolumeAsync(stack.Namespace, volume.Name) is null
                    ? LogBuilder.Message("Failed.").AsError()
                    : LogBuilder.Message("Done.").AsSuccess())
                .RunAsync();
        } 
    }
}