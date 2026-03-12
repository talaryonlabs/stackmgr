using System.Net;
using System.Text.Json.Serialization;
using Talaryon.StackManager.Proxy.Models;
using Talaryon.StackManager.Proxy.Utilities;
using Talaryon.Toolbox.Api.Errors;

namespace Talaryon.StackManager.Proxy.Services;

public partial class RancherService
{
    public async ValueTask<IEnumerable<PersistentVolume>> GetPersistentVolumesAsync(CancellationToken cancellationToken)
    {
        var response = await _client.GetAsync("/v1/persistentvolumes", cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to request persistent volumes. Response code: {response.StatusCode}");

        var list = await response.Content.ReadFromJsonAsync<RancherPersistentVolumeList>(cancellationToken);
        return (list?.Data ?? []).Select(v => new PersistentVolume
        {
            Name = v.Metadata.Name,
            StorageSize = v.Spec.Capacity.GetValueOrDefault("storage", "0"),
            AccessMode = v.Spec.AccessModes.FirstOrDefault() ?? "ReadWriteOnce",
            VolumeHandle = v.Spec.CSI.VolumeHandle,
        });
    }

    public async ValueTask<PersistentVolume> GetPersistentVolumeAsync(string name, CancellationToken cancellationToken)
    {
        var response = await _client.GetAsync($"/v1/persistentvolumes/{name}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw response.StatusCode switch
            {
                HttpStatusCode.NotFound => new NotFoundError($"Persistent volume '{name}' not found."),
                _ => new InternalServerError()
            };
        }

        var data = await response.Content.ReadFromJsonAsync<RancherPersistentVolume>(cancellationToken);
        if (data is null)
            throw new InternalServerError($"Failed to get persistent volume '{name}'. (unknown error)");

        return new PersistentVolume
        {
            Name = data.Metadata.Name,
            StorageSize = data.Spec.Capacity.GetValueOrDefault("storage", "0"),
            AccessMode = data.Spec.AccessModes.FirstOrDefault() ?? "ReadWriteOnce",
            VolumeHandle = data.Spec.CSI.VolumeHandle
        };
    }

    public async ValueTask<PersistentVolume> CreatePersistentVolumeAsync(PersistentVolume pv, CancellationToken cancellationToken = default)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(pv.Name))
            throw new BadRequestError("Persistent volume name cannot be null or empty.");
            
        if (string.IsNullOrWhiteSpace(pv.VolumeHandle))
            throw new BadRequestError("Volume handle cannot be null or empty.");
            
        if (string.IsNullOrWhiteSpace(pv.AccessMode))
            throw new BadRequestError("Access mode cannot be null or empty.");
            
        if (string.IsNullOrWhiteSpace(pv.StorageSize))
            throw new BadRequestError("Storage size cannot be null or empty.");
            
        // Validate name format
        if (!RegexPatterns.IsValidKubernetesName(pv.Name))
            throw new BadRequestError("Persistent volume name must be valid Kubernetes DNS name (alphanumeric and hyphens only, max 63 chars).");
            
        // Validate access mode
        if (pv.AccessMode != "ReadWriteOnce" && pv.AccessMode != "ReadOnlyMany" && pv.AccessMode != "ReadWriteMany")
            throw new BadRequestError("Access mode must be one of: ReadWriteOnce, ReadOnlyMany, ReadWriteMany");
            
        // Validate storage size format
        if (!RegexPatterns.IsValidStorageSize(pv.StorageSize))
            throw new BadRequestError("Storage size must be a valid quantity (e.g., 10Gi, 500M).");
        
        try
        {
            await GetPersistentVolumeAsync(pv.Name, cancellationToken);
            throw new ConflictError($"Volume claim '{pv.Name}' already exists.");
        }
        catch (NotFoundError) { }
        
        var request = new Dictionary<string, object>
        {
            { "type", "persistentvolume" },
            { "metadata", new Dictionary<string, object> { { "name", pv.Name } } },
            { "spec", new Dictionary<string, object>
                {
                    { "capacity", new Dictionary<string, string> { { "storage", pv.StorageSize } } },
                    { "accessModes", new[] { pv.AccessMode } },
                    { "storageClassName", "longhorn-static" },
                    { "persistentVolumeReclaimPolicy", "Retain" },
                    { "volumeMode", "Filesystem" },
                    { "csi", new Dictionary<string, object>
                        {
                            { "driver", "driver.longhorn.io" },
                            { "volumeHandle", pv.VolumeHandle }
                        }
                    }
                }
            }
        };
        
        var response = await _client.PostAsJsonAsync("/v1/persistentvolumes", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InternalServerError($"Failed to create persistent volume '{pv.Name}'. Response code: {response.StatusCode}");

        var data = await response.Content.ReadFromJsonAsync<RancherPersistentVolume>(cancellationToken);
        if (data is null)
            throw new InternalServerError($"Failed to create persistent volume '{pv.Name}'. (unknown error)");

        return new PersistentVolume
        {
            Name = data.Metadata.Name,
            StorageSize = pv.StorageSize,
            AccessMode = pv.AccessMode,
            VolumeHandle = data.Spec.CSI.VolumeHandle,
        };
    }

    public async ValueTask<PersistentVolume> DeletePersistentVolumeAsync(string name, CancellationToken cancellationToken = default)
    {
        var pv = await GetPersistentVolumeAsync(name, cancellationToken);
        var response = await _client.DeleteAsync($"/v1/persistentvolumes/{name}", cancellationToken);
        return !response.IsSuccessStatusCode
            ? throw new InternalServerError($"Failed to delete persistent volume '{name}'. Response code: {response.StatusCode}")
            : pv;
    }

    private class RancherPersistentVolume
    {
        [JsonPropertyName("id")] public required string Id { get; init; }
        [JsonPropertyName("type")] public required string Type { get; init; }
        [JsonPropertyName("metadata")] public required RancherPersistentVolumeMetadata Metadata { get; init; }
        [JsonPropertyName("spec")] public required RancherPersistentVolumeSpec Spec { get; init; }
        [JsonPropertyName("status")] public required RancherPersistentVolumeStatus Status { get; init; }
    }

    private class RancherPersistentVolumeMetadata
    {
        [JsonPropertyName("name")] public required string Name { get; init; }
        [JsonPropertyName("creationTimestamp")] public required string CreationTimestamp { get; init; }
    }

    private class RancherPersistentVolumeSpec
    {
        [JsonPropertyName("capacity")] public required Dictionary<string, string> Capacity { get; init; }
        [JsonPropertyName("accessModes")] public List<string> AccessModes { get; init; } = [];
        [JsonPropertyName("storageClassName")] public required string StorageClassName { get; init; }
        [JsonPropertyName("persistentVolumeReclaimPolicy")] public required string PersistentVolumeReclaimPolicy { get; init; }
        [JsonPropertyName("volumeMode")] public required string VolumeMode { get; init; }
        [JsonPropertyName("csi")] public required RancherPersistentVolumeCSIDriver CSI { get; init; }
    }

    private class RancherPersistentVolumeCSIDriver
    {
        [JsonPropertyName("driver")] public required string Driver { get; init; } // "driver.longhorn.io"
        [JsonPropertyName("volumeHandle")] public required string VolumeHandle { get; init; } // Longhorn-Volume-Name
    }

    private class RancherPersistentVolumeStatus
    {
        [JsonPropertyName("phase")] public required string Phase { get; init; } // "Available", "Bound", etc.
    }

    private class RancherPersistentVolumeList
    {
        [JsonPropertyName("data")] public List<RancherPersistentVolume> Data { get; init; } = [];
    }
}