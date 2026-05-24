using System.CommandLine;
using StackManager.Shared.Models;
using Talaryon.StackManager.Exceptions;
using Talaryon.StackManager.Services;

namespace Talaryon.StackManager.Commands;

public class SyncCommand : BaseCommand
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
        var syncService = new SyncService(proxy);

        if (stack.IsDeleted)
        {
            await DeleteStackFromRemote(stack, syncService);
            return;
        }
        
        await SyncNamespaceWithRemote(stack, syncService);
        await SyncStackVolumes(stack, syncService);
        await SyncApplicationWithRemote(stack, syncService, parseResult.GetValue<bool, ApplyOption>());
    }

    private async Task DeleteStackFromRemote(Stack stack, SyncService syncService)
    {
        LogMessage.AsInfo($"Deleting stack '{stack.Name}' from remote.");
        
        var volumes = await syncService.Proxy.GetVolumesAsync(stack.Namespace);
        var application = await syncService.Proxy.GetApplicationAsync(stack.Namespace);
        var ns = await syncService.Proxy.GetNamespaceAsync(stack.Namespace);
        var error = false;

        foreach (var volume in volumes)
        {
            await LogBuilder.Message($"- [Volume] {volume.Name} ... ")
                .NoNewLineAfter()
                .WaitFor(async () =>
                {
                    if (await syncService.Proxy.DeleteVolumeAsync(stack.Namespace, volume.Name) is null)
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
                    if (await syncService.Proxy.DeleteApplicationAsync(stack.Namespace) is null)
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
                    if (await syncService.Proxy.DeleteNamespaceAsync(stack.Namespace) is null)
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

    private async Task SyncNamespaceWithRemote(Stack stack, SyncService syncService)
    {
        var ns = default(Namespace);
        await LogBuilder.Message($"- [Namespace] {stack.Namespace} ... ")
            .NoNewLineAfter()
            .WaitFor(async () =>
            {
                ns = await syncService.SyncNamespaceAsync(stack);
                return ns is not null
                    ? LogBuilder.Message("Already exists.").AsWarning()
                    : LogBuilder.Message("Failed.").AsError();
            })
            .RunAsync();
    }

    private async Task SyncApplicationWithRemote(Stack stack, SyncService syncService, bool applyChanges)
    {
        var application = default(Application);
        await LogBuilder.Message($"- [Application] {stack.Namespace} ... ")
            .NoNewLineAfter()
            .WaitFor(async () =>
            {
                try
                {
                    application = await syncService.SyncApplicationAsync(stack);
                    return application is not null
                        ? LogBuilder.Message("Up to date.").AsWarning()
                        : LogBuilder.Message("Failed.").AsError();
                }
                catch (ConfigurationException ex)
                {
                    return LogBuilder.Message(ex.Message).AsError();
                }
            })
            .RunAsync();
        
        if (application is not null && applyChanges)
        {
            await LogBuilder.Message($"- [Application] Applying changes ... ")
                .NoNewLineAfter()
                .WaitFor(async () => await syncService.Proxy.ApplyApplicationAsync(stack.Namespace) is null
                    ? LogBuilder.Message("Failed.").AsError()
                    : LogBuilder.Message("Done.").AsSuccess())
                .RunAsync();
        }
    }

    private async Task SyncStackVolumes(Stack stack, SyncService syncService)
    {
        var remote = await syncService.Proxy.GetVolumesAsync(stack.Namespace);
        var local = SyncService.GetExpandedVolumes(stack);
        
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
                    var v = await syncService.Proxy.CreateVolumeAsync(stack.Namespace, new Volume
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
                .WaitFor(async () => await syncService.Proxy.DeleteVolumeAsync(stack.Namespace, volume.Name) is null
                    ? LogBuilder.Message("Failed.").AsError()
                    : LogBuilder.Message("Done.").AsSuccess())
                .RunAsync();
        } 
    }
}