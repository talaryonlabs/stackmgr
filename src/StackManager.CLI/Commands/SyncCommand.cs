using System.CommandLine;
using StackManager.Shared.Models;
using Talaryon.StackManager.Options;
using Talaryon.StackManager.Services;
using Talaryon.StackManager.Types;

namespace Talaryon.StackManager.Commands;

public class SyncCommand : StackManagerCommand
{
    public SyncCommand() : base("sync", "Sync a stack")
    {
        Add(new EnvironmentOption());
        Add(new StackOption());
        Add(new ApplyOption());
        SetAction(SyncStack);
    }
    
    private async Task SyncStack(ParseResult parseResult)
    {
        var config = GetRequiredService<LocalConfig>();
        var env = GetEnvironment<EnvironmentOption>(parseResult);
        var stack = GetStack<StackOption>(parseResult, env);
        var remote = config
                         .Remotes
                         .FirstOrDefault(r => r.Name == env.Remote)
                     ?? throw new Exception($"Remote '{env.Remote}' not found in configuration.");

        await stack.BuildAsync();
        var git = GetRequiredService<GitService>();
        await git.ApplyAsync(stack);
        
        var httpClientFactory = GetRequiredService<IHttpClientFactory>();
        var proxy = new ProxyService(remote, httpClientFactory);

        if (stack.IsDeleted)
        {
            await DeleteStackFromRemote(stack, proxy);
            return;
        }
        
        var ns = await SyncNamespaceWithRemote(stack, proxy);
        if (ns is not null)
        {
            await SyncStackVolumes(stack, proxy);
        }
        
        var application = await SyncApplicationWithRemote(stack, proxy);
        if (application is not null && parseResult.GetValue<bool, ApplyOption>())
        {
            await LogBuilder.Message($"- [Application] Applying changes ... ")
                .NoNewLineAfter()
                .WaitFor(async () => await proxy.ApplyApplicationAsync(stack.Namespace) is null
                    ? LogBuilder.Message("Failed.").AsError()
                    : LogBuilder.Message("Done.").AsSuccess())
                .RunAsync();
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
                if (stack.Environment.Repository is null)
                {
                    return LogBuilder.Message("No repository set in environment.").AsError();
                }

                if ((application = await proxy.GetApplicationAsync(stack.Namespace)) is not null)
                {
                    if (application.Path != $"{stack.Environment.Name}/{stack.Name}" ||
                        application.Repository != stack.Environment.Repository ||
                        application.IsAutoSyncEnabled != stack.EnableAutoSync)
                    {
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

    private async Task SyncStackVolumes(Talaryon.StackManager.Types.Stack stack, IProxyService proxy)
    {
        var remote = await proxy.GetVolumesAsync(stack.Namespace);
        var local = stack.Volumes
            .SelectMany(v =>
            {
                if (v.Replicas > 0)
                {
                    return Enumerable.Range(0, v.Replicas).Select(i => new StackVolume
                    {
                        AccessMode = v.AccessMode,
                        Name = $"{v.Name}-{i}",
                        StorageSize = v.StorageSize,
                        Replicas = 0,
                        Stack = v.Stack
                    });
                }

                return [v];
            })
            .ToList();
        
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