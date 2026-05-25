using StackManager.Shared.Models;
using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Services;

/// <summary>
/// Service for synchronizing stack resources with a remote Kubernetes cluster.
/// </summary>
public interface ISyncService
{
    /// <summary>
    /// Synchronizes a stack with the remote cluster.
    /// </summary>
    /// <param name="stack">The stack to synchronize</param>
    /// <param name="applyChanges">Whether to apply changes after sync</param>
    /// <returns>True if synchronization was successful</returns>
    Task<bool> SyncStackAsync(Stack stack, bool applyChanges);
    
    /// <summary>
    /// Deletes a stack from the remote cluster.
    /// </summary>
    /// <param name="stack">The stack to delete</param>
    /// <returns>True if deletion was successful</returns>
    Task<bool> DeleteStackAsync(Stack stack);
}

public class SyncService(IProxyService proxy, IGitService git) : ISyncService
{
    private IProxyServiceActions? _remote;
    
    public async Task<bool> SyncStackAsync(Stack stack, bool applyChanges)
    {
        var remote = LocalConfig.Get()
                         .Remotes
                         .FirstOrDefault(r => r.Name == stack.Environment.Remote)
                     ?? throw new Exception($"Remote '{stack.Environment.Remote}' not found in configuration.");
        
        _remote = proxy.Remote(remote);
        
        if (stack.Namespace is null)
        {
            throw new ConfigurationException("Stack namespace is not set.");
        }
        if (stack.Environment.Repository is null)
        {
            throw new ConfigurationException("No repository set in environment.");
        }
        
        var success = true;
        if (await SyncNamespaceAsync(stack))
        {
            success &= await SyncChangesAsync(stack);
            success &= await SyncVolumesAsync(stack);
            success &= await SyncApplicationAsync(stack, applyChanges);
            
            return success;
        }

        return false;
    }
    
    public async Task<bool> DeleteStackAsync(Stack stack)
    {
        if (stack.Namespace is null)
        {
            throw new ConfigurationException("Stack namespace is not set.");
        }
        
        var remote = LocalConfig.Get()
                         .Remotes
                         .FirstOrDefault(r => r.Name == stack.Environment.Remote)
                     ?? throw new Exception($"Remote '{stack.Environment.Remote}' not found in configuration.");
        
        _remote = proxy.Remote(remote);
        
        var volumes = await _remote.GetVolumesAsync(stack.Namespace);
        var application = await _remote.GetApplicationAsync(stack.Namespace);
        var ns = await _remote.GetNamespaceAsync(stack.Namespace);
        var error = false;

        foreach (var volume in volumes)
        {
            await LogBuilder.Message($"- [Volume] {volume.Name} ... ")
                .NoNewLineAfter()
                .WaitFor(async () =>
                {
                    if (await _remote.DeleteVolumeAsync(stack.Namespace, volume.Name) is not null)
                        return LogBuilder.Message("Done.").AsSuccess();
                    
                    error = true;
                    return LogBuilder.Message("Failed.").AsError();
                })
                .RunAsync();
        }

        if (application is not null)
        {
            await LogBuilder.Message($"- [Application] {stack.Namespace} ... ")
                .NoNewLineAfter()
                .WaitFor(async () =>
                {
                    if (await _remote.DeleteApplicationAsync(stack.Namespace) is not null)
                        return LogBuilder.Message("Done.").AsSuccess();
                    
                    error = true;
                    return LogBuilder.Message("Failed.").AsError();
                })
                .RunAsync();
        }

        if (ns is not null)
        {
            await LogBuilder.Message($"- [Namespace] {stack.Namespace} ... ")
                .NoNewLineAfter()
                .WaitFor(async () =>
                {
                    if (await _remote.DeleteNamespaceAsync(stack.Namespace) is not null)
                        return LogBuilder.Message("Done.").AsSuccess();
                    
                    error = true;
                    return LogBuilder.Message("Failed.").AsError();
                })
                .RunAsync();
        }

        return !error;
    }
    
    private async Task<bool> SyncNamespaceAsync(Stack stack)
    {
        var ns = default(Namespace);
        
        await LogBuilder.Message($"- [Namespace] {stack.Namespace} ... ")
            .NoNewLineAfter()
            .WaitFor(async () =>
            {
                if ((ns = await _remote.GetNamespaceAsync(stack.Namespace!)) is not null)
                {
                    return LogBuilder.Message("Already exists.").AsWarning();
                }

                if ((ns = await _remote.CreateNamespaceAsync(stack.Namespace!)) is not null)
                {
                    return LogBuilder.Message("Done.").AsSuccess();
                }
                
                return LogBuilder.Message("Failed.").AsError();
            })
            .RunAsync();

        return (ns is not null);
    }
    
    private async Task<bool> SyncVolumesAsync(Stack stack)
    {
        var remote = await _remote.GetVolumesAsync(stack.Namespace!);
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
                    var v = await _remote.CreateVolumeAsync(stack.Namespace!, new Volume
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
                .WaitFor(async () => await _remote.DeleteVolumeAsync(stack.Namespace!, volume.Name) is null
                    ? LogBuilder.Message("Failed.").AsError()
                    : LogBuilder.Message("Done.").AsSuccess())
                .RunAsync();
        }

        return true;
    }
    
    private async Task<bool> SyncApplicationAsync(Stack stack, bool applyChanges)
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

                if ((application = await _remote.GetApplicationAsync(stack.Namespace!)) is not null)
                {
                    if (application.Path != $"{stack.Environment.Name}/{stack.Name}" ||
                        application.Repository != stack.Environment.Repository ||
                        application.IsAutoSyncEnabled != stack.EnableAutoSync)
                    {
                        application.IsAutoSyncEnabled = stack.EnableAutoSync;
                        application.Path = $"{stack.Environment.Name}/{stack.Name}";
                        application.Repository = stack.Environment.Repository;

                        return await _remote.UpdateApplicationAsync(stack.Namespace!, application) is not null
                            ? LogBuilder.Message("Update succeeded.").AsSuccess()
                            : LogBuilder.Message("Update failed.").AsError();
                    }

                    return LogBuilder.Message("Up to date.").AsWarning();
                }

                var newApp = new Application
                {
                    Name = stack.Namespace!,
                    IsAutoSyncEnabled = false,
                    Path = $"{stack.Environment.Name}/{stack.Name}",
                    Repository = stack.Environment.Repository
                };
                
                return (application = await _remote.CreateApplicationAsync(newApp)) is not null
                    ? LogBuilder.Message("Created.").AsSuccess()
                    : LogBuilder.Message("Failed.").AsError();
            })
            .RunAsync();

        if (applyChanges && application is not null)
        {
            await _remote.ApplyApplicationAsync(application.Name);
        }
        
        return (application is not null);
    }

    private async Task<bool> SyncChangesAsync(Stack stack)
    {
        var files = Array.Empty<FileInfo>();
        var repo = git.CurrentDirectory();

        await repo.PullAsync();
        await LogBuilder.Message("- [Git] Review changes ... ")
            .NoNewLineAfter()
            .WaitFor(async () =>
            {
                await repo.ResetAsync();
                files = (await repo.DiffAsync())
                    .Split(Environment.NewLine)
                    .Where(x => x.Length > 0)
                    .Select(x => new FileInfo(x))
                    .ToArray();

                return LogBuilder.Message("Done.").AsSuccess();

            })
            .RunAsync();

        await LogBuilder.Message("- [Git] Commit changes ... ")
            .NoNewLineAfter()
            .WaitFor(async () =>
            {
                if (files.Length == 0) return LogBuilder.Message("No changes.").AsWarning();
               
                await repo.AddAsync();
                await repo.CommitAsync([
                    "Apply changes. (StackManager)",
                    $" > Stack: [{stack.Name}]",
                    $" > Environment: [{stack.Environment.Name}]",
                    $" > Files: [{string.Join("; ", files.Select(x => x.Name.Trim()))}]"
                ]);
                
                return LogBuilder.Message("Done.").AsSuccess();
            })
            .RunAsync();

        await LogBuilder.Message("- [Git] Push changes ... ")
            .NoNewLineAfter()
            .WaitFor(async () =>
            {
                await repo.PushAsync();
                return LogBuilder.Message("Done.").AsSuccess();
            })
            .RunAsync();

        return true;
    }
}
