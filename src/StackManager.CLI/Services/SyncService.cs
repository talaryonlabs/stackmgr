using StackManager.Shared.Models;
using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Services;

public interface ISyncService
{
    Task<bool> SyncStackAsync(Stack stack, bool applyChanges);
    Task<bool> DeleteStackAsync(Stack stack);
}


/// <summary>
/// Service for synchronizing stack resources with a remote Kubernetes cluster.
/// </summary>
public class SyncService(IProxyService proxy, IGitService git) : ISyncService
{
    /// <summary>
    /// Synchronizes a stack with the remote cluster.
    /// </summary>
    /// <param name="stack">The stack to synchronize</param>
    /// <param name="applyChanges">Whether to apply changes after sync</param>
    /// <returns>True if synchronization was successful</returns>
    public async Task<bool> SyncStackAsync(Stack stack, bool applyChanges)
    {
        if (stack.IsDeleted)
        {
            return await DeleteStackAsync(stack);
        }
        
        if (stack.Environment.Repository is null)
        {
            throw new ConfigurationException("No repository set in environment.");
        }
        
        var success = true;
        
        await LogBuilder.Message($"- [Namespace] {stack.Namespace} ... ")
            .NoNewLineAfter()
            .WaitFor(async () =>
            {
                success &= await SyncNamespaceAsync(stack) is not null;
                return LogBuilder.Message("");
            })
            .RunAsync();

        await SyncChangesAsync(stack);
        await SyncVolumesAsync(stack);

        var application = await SyncApplicationAsync(stack);

        if (application is not null && applyChanges)
        {
            var result = await proxy.ApplyApplicationAsync(stack.Namespace);
            return result is not null;
        }

        return success;
    }

    /// <summary>
    /// Deletes a stack from the remote cluster.
    /// </summary>
    /// <param name="stack">The stack to delete</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteStackAsync(Stack stack)
    {
        var volumes = await proxy.GetVolumesAsync(stack.Namespace);
        var application = await proxy.GetApplicationAsync(stack.Namespace);
        var ns = await proxy.GetNamespaceAsync(stack.Namespace);
        var error = false;

        foreach (var volume in volumes)
        {
            await LogBuilder.Message($"- [Volume] {volume.Name} ... ")
                .NoNewLineAfter()
                .WaitFor(async () =>
                {
                    if (await proxy.DeleteVolumeAsync(stack.Namespace, volume.Name) is not null)
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
                    if (await proxy.DeleteApplicationAsync(stack.Namespace) is not null)
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
                    if (await proxy.DeleteNamespaceAsync(stack.Namespace) is not null)
                        return LogBuilder.Message("Done.").AsSuccess();
                    
                    error = true;
                    return LogBuilder.Message("Failed.").AsError();
                })
                .RunAsync();
        }

        return !error;
    }

    /// <summary>
    /// Synchronizes the namespace for a stack.
    /// </summary>
    /// <param name="stack">The stack</param>
    /// <returns>The namespace object, or null if creation failed</returns>
    public async Task<Namespace?> SyncNamespaceAsync(Stack stack)
    {
        var ns = await proxy.GetNamespaceAsync(stack.Namespace);
        if (ns is not null)
        {
            LogMessage.AsWarning("Already exists.");
            return ns;
        }
        
        ns = await proxy.CreateNamespaceAsync(stack.Namespace);
        if (ns is not null) return ns;
        
        LogMessage.AsError("Failed.");
        return null;

    }

    /// <summary>
    /// Synchronizes volumes for a stack.
    /// Creates volumes that exist locally but not remotely.
    /// Deletes volumes that exist remotely but not locally.
    /// </summary>
    /// <param name="stack">The stack</param>
    /// <returns>Task representing the async operation</returns>
    public async Task SyncVolumesAsync(Stack stack)
    {
        var remote = await proxy.GetVolumesAsync(stack.Namespace!);
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
                    var v = await proxy.CreateVolumeAsync(stack.Namespace!, new Volume
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
                .WaitFor(async () => await proxy.DeleteVolumeAsync(stack.Namespace!, volume.Name) is null
                    ? LogBuilder.Message("Failed.").AsError()
                    : LogBuilder.Message("Done.").AsSuccess())
                .RunAsync();
        } 
    }

    /// <summary>
    /// Synchronizes the ArgoCD application for a stack.
    /// </summary>
    /// <param name="stack">The stack</param>
    /// <returns>The application object, or null if sync failed</returns>
    public async Task<Application?> SyncApplicationAsync(Stack stack)
    {
        var application = await proxy.GetApplicationAsync(stack.Namespace);

        if (application is not null)
        {
            if (application.Path != $"{stack.Environment.Name}/{stack.Name}" ||
                application.Repository != stack.Environment.Repository ||
                application.IsAutoSyncEnabled != stack.EnableAutoSync)
            {
                application.IsAutoSyncEnabled = stack.EnableAutoSync;
                application.Path = $"{stack.Environment.Name}/{stack.Name}";
                application.Repository = stack.Environment.Repository;

                return await proxy.UpdateApplicationAsync(stack.Namespace, application);
            }

            return application;
        }

        var newApp = new Application
        {
            Name = stack.Namespace,
            IsAutoSyncEnabled = false,
            Path = $"{stack.Environment.Name}/{stack.Name}",
            Repository = stack.Environment.Repository
        };

        return await proxy.CreateApplicationAsync(newApp);
    }
    
    public async Task SyncChangesAsync(Stack stack)
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
                    "\"Apply changes. (StackManager)\"",
                    $"\"> Stack: [{stack.Name}]\"",
                    $"\"> Environment: [{stack.Environment.Name}]\"",
                    "\"> Files: \"",
                    string.Join(Environment.NewLine, files.Select(x => $"\" - {x.Name.Trim()}\""))
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
    }
}
