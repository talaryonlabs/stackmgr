using StackManager.Shared.Models;
using Talaryon.StackManager.Exceptions;
using Talaryon.StackManager.Types;

namespace Talaryon.StackManager.Services;

/// <summary>
/// Service for synchronizing stack resources with a remote Kubernetes cluster.
/// </summary>
public class SyncService
{
    /// <summary>
    /// Gets the proxy service used for remote operations.
    /// </summary>
    public IProxyService Proxy => _proxy;

    private readonly IProxyService _proxy;

    /// <summary>
    /// Creates a new SyncService.
    /// </summary>
    /// <param name="proxy">The proxy service for remote operations</param>
    public SyncService(IProxyService proxy)
    {
        _proxy = proxy;
    }

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

        var ns = await SyncNamespaceAsync(stack);
        if (ns is null)
        {
            return false;
        }

        await SyncVolumesAsync(stack);

        var application = await SyncApplicationAsync(stack);

        if (application is not null && applyChanges)
        {
            var result = await _proxy.ApplyApplicationAsync(stack.Namespace);
            return result is not null;
        }

        return true;
    }

    /// <summary>
    /// Deletes a stack from the remote cluster.
    /// </summary>
    /// <param name="stack">The stack to delete</param>
    /// <returns>True if deletion was successful</returns>
    public async Task<bool> DeleteStackAsync(Stack stack)
    {
        var volumes = await _proxy.GetVolumesAsync(stack.Namespace);
        var application = await _proxy.GetApplicationAsync(stack.Namespace);
        var ns = await _proxy.GetNamespaceAsync(stack.Namespace);
        var error = false;

        foreach (var volume in volumes)
        {
            if (await _proxy.DeleteVolumeAsync(stack.Namespace, volume.Name) is null)
            {
                error = true;
            }
        }

        if (application is not null)
        {
            if (await _proxy.DeleteApplicationAsync(stack.Namespace) is null)
            {
                error = true;
            }
        }

        if (ns is not null)
        {
            if (await _proxy.DeleteNamespaceAsync(stack.Namespace) is null)
            {
                error = true;
            }
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
        var ns = await _proxy.GetNamespaceAsync(stack.Namespace);
        
        if (ns is not null)
        {
            return ns;
        }

        return await _proxy.CreateNamespaceAsync(stack.Namespace);
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
        var remote = await _proxy.GetVolumesAsync(stack.Namespace);
        var local = GetExpandedVolumes(stack);

        foreach (var volume in local.ExceptBy(remote.Select(v => v.Name), v => v.Name))
        {
            await _proxy.CreateVolumeAsync(stack.Namespace, new Volume
            {
                Name = volume.Name,
                AccessMode = volume.AccessMode,
                Size = volume.StorageSize
            });
        }

        foreach (var volume in remote.ExceptBy(local.Select(v => v.Name), v => v.Name))
        {
            await _proxy.DeleteVolumeAsync(stack.Namespace, volume.Name);
        }
    }

    /// <summary>
    /// Synchronizes the ArgoCD application for a stack.
    /// </summary>
    /// <param name="stack">The stack</param>
    /// <returns>The application object, or null if sync failed</returns>
    public async Task<Application?> SyncApplicationAsync(Stack stack)
    {
        if (stack.Environment.Repository is null)
        {
            throw new ConfigurationException("No repository set in environment.");
        }

        var application = await _proxy.GetApplicationAsync(stack.Namespace);

        if (application is not null)
        {
            if (application.Path != $"{stack.Environment.Name}/{stack.Name}" ||
                application.Repository != stack.Environment.Repository ||
                application.IsAutoSyncEnabled != stack.EnableAutoSync)
            {
                application.IsAutoSyncEnabled = stack.EnableAutoSync;
                application.Path = $"{stack.Environment.Name}/{stack.Name}";
                application.Repository = stack.Environment.Repository;

                return await _proxy.UpdateApplicationAsync(stack.Namespace, application);
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

        return await _proxy.CreateApplicationAsync(newApp);
    }

    /// <summary>
    /// Expands volume definitions based on replicas.
    /// If a volume has Replicas > 0, creates multiple volumes with numbered suffixes.
    /// </summary>
    /// <param name="stack">The stack</param>
    /// <returns>List of expanded volumes</returns>
    public static List<StackVolume> GetExpandedVolumes(Stack stack)
    {
        return stack.Volumes
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
    }
}
