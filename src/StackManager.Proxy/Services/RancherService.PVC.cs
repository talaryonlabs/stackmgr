using System.Net;
using System.Text.Json.Serialization;
using Talaryon.StackManager.Proxy.Models;
using Talaryon.StackManager.Proxy.Utilities;
using Talaryon.Toolbox.Api.Errors;

namespace Talaryon.StackManager.Proxy.Services;

public partial class RancherService
{
    public async ValueTask<IEnumerable<PersistentVolumeClaim>> GetVolumeClaimsAsync(string ns,
        CancellationToken cancellationToken)
    {
        var response =
            await _client.GetAsync($"/v1/persistentvolumeclaims/{ns}", cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to request volume claims. Response code: {response.StatusCode}");

        var list = await response.Content.ReadFromJsonAsync<RancherPersistentVolumeClaimList>(cancellationToken);
        return list?.Data.Select(v => new PersistentVolumeClaim
        {
            Name = v.Metadata.Name,
            Namespace = v.Metadata.Namespace,
            VolumeName = v.Spec.VolumeName,
            StorageSize = v.Spec.Resources.Requests.GetValueOrDefault("storage", "0"),
            AccessMode = v.Spec.AccessModes.FirstOrDefault() ?? "ReadWriteOnce",
            Status = v.Status.Phase
        }) ?? [];
    }

    public async ValueTask<PersistentVolumeClaim> GetVolumeClaimAsync(string ns, string name,
        CancellationToken cancellationToken)
    {
        var response = await _client.GetAsync($"/v1/persistentvolumeclaims/{ns}/{name}",
            cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw response.StatusCode switch
            {
                HttpStatusCode.NotFound => new NotFoundError(
                    $"Volume claim '{name}' not found in namespace '{ns}'."),
                _ => new InternalServerError()
            };
        }

        var data = await response.Content.ReadFromJsonAsync<RancherPersistentVolumeClaim>(cancellationToken);
        if (data is null)
            throw new InternalServerError($"Failed to get volume claim '{name}'. (unknown error)");

        return new PersistentVolumeClaim
        {
            Name = data.Metadata.Name,
            Namespace = data.Metadata.Namespace,
            VolumeName = data.Spec.VolumeName,
            StorageSize = data.Spec.Resources.Requests.GetValueOrDefault("storage", "0"),
            AccessMode = data.Spec.AccessModes.FirstOrDefault() ?? "ReadWriteOnce",
            Status = data.Status.Phase
        };
    }

    public async ValueTask<PersistentVolumeClaim> CreateVolumeClaimAsync(
        string ns,
        PersistentVolumeClaim claim,
        CancellationToken cancellationToken)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(ns))
            throw new BadRequestError("Namespace cannot be null or empty.");
            
        if (string.IsNullOrWhiteSpace(claim.Name))
            throw new BadRequestError("Volume claim name cannot be null or empty.");
            
        if (string.IsNullOrWhiteSpace(claim.VolumeName))
            throw new BadRequestError("Volume name cannot be null or empty.");
            
        if (string.IsNullOrWhiteSpace(claim.AccessMode))
            throw new BadRequestError("Access mode cannot be null or empty.");
            
        if (string.IsNullOrWhiteSpace(claim.StorageSize))
            throw new BadRequestError("Storage size cannot be null or empty.");
            
        // Validate name formats
        if (!RegexPatterns.IsValidKubernetesName(claim.Name))
            throw new BadRequestError("Volume claim name must be valid Kubernetes DNS name (alphanumeric and hyphens only, max 63 chars).");
            
        // Validate access mode
        if (claim.AccessMode != "ReadWriteOnce" && claim.AccessMode != "ReadOnlyMany" && claim.AccessMode != "ReadWriteMany")
            throw new BadRequestError("Access mode must be one of: ReadWriteOnce, ReadOnlyMany, ReadWriteMany");
            
        // Validate storage size format
        if (!RegexPatterns.IsValidStorageSize(claim.StorageSize))
            throw new BadRequestError("Storage size must be a valid quantity (e.g., 10Gi, 500M).");
        
        try
        {
            await GetVolumeClaimAsync(ns, claim.Name, cancellationToken);
            throw new ConflictError($"Volume claim '{claim.Name}' already exists.");
        }
        catch (NotFoundError) { }
        
        var request = new Dictionary<string, object>
        {
            { "type", "persistentvolumeclaim" },
            { "metadata", new Dictionary<string, object>
                {
                    { "name", claim.Name },
                    { "namespace", ns }
                }
            },
            { "spec", new Dictionary<string, object>
                {
                    { "accessModes", new[] { claim.AccessMode } },
                    { "resources", new Dictionary<string, object>
                        {
                            { "requests", new Dictionary<string, string>
                                {
                                    { "storage", claim.StorageSize }
                                }
                            }
                        }
                    },
                    { "storageClassName", "longhorn-static" },
                    { "volumeName", claim.VolumeName },
                    { "volumeMode", "Filesystem" }
                }
            }
        };

        var response = await _client.PostAsJsonAsync($"/v1/persistentvolumeclaims/{ns}", request,
            cancellationToken);
        var content = await response.Content.ReadAsStringAsync(cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InternalServerError(
                $"Failed to create volume claim '{claim.Name}'. Response code: {response.StatusCode}");

        var data = await response.Content.ReadFromJsonAsync<RancherPersistentVolumeClaim>(cancellationToken);
        if (data is null)
            throw new InternalServerError($"Failed to create volume claim '{claim.Name}'. (unknown error)");

        return new PersistentVolumeClaim
        {
            Name = data.Metadata.Name,
            Namespace = data.Metadata.Namespace,
            VolumeName = data.Spec.VolumeName,
            StorageSize = claim.StorageSize,
            AccessMode = claim.AccessMode,
            Status = data.Status.Phase,
        };
    }

    public async ValueTask<PersistentVolumeClaim> DeleteVolumeClaimAsync(string namespaceName, string claimName,
        CancellationToken cancellationToken)
    {
        var claim = await GetVolumeClaimAsync(namespaceName, claimName, cancellationToken);
        var response = await _client.DeleteAsync($"/v1/persistentvolumeclaims/{namespaceName}/{claimName}",
            cancellationToken);
        return !response.IsSuccessStatusCode
            ? throw new InternalServerError(
                $"Failed to delete volume claim '{claimName}'. Response code: {response.StatusCode}")
            : claim;
    }

    private class RancherPersistentVolumeClaim
    {
        [JsonPropertyName("id")] public required string Id { get; init; }
        [JsonPropertyName("type")] public required string Type { get; init; }
        [JsonPropertyName("metadata")] public required RancherPersistentVolumeClaimMetadata Metadata { get; init; }
        [JsonPropertyName("spec")] public required RancherPersistentVolumeClaimSpec Spec { get; init; }
        [JsonPropertyName("status")] public required RancherPersistentVolumeClaimStatus Status { get; init; }
    }

    private class RancherPersistentVolumeClaimMetadata
    {
        [JsonPropertyName("name")] public required string Name { get; init; }
        [JsonPropertyName("namespace")] public required string Namespace { get; init; }
        [JsonPropertyName("annotations")] public Dictionary<string, string>? Annotations { get; init; }

        [JsonPropertyName("creationTimestamp")]
        public required string CreationTimestamp { get; init; }
    }

    private class RancherPersistentVolumeClaimSpec
    {
        [JsonPropertyName("accessModes")] public List<string> AccessModes { get; init; } = [];
        [JsonPropertyName("resources")] public required RancherPersistentVolumeClaimResources Resources { get; init; }
        [JsonPropertyName("storageClassName")] public required string StorageClassName { get; init; }
        [JsonPropertyName("volumeName")] public required string VolumeName { get; init; }
        [JsonPropertyName("volumeMode")] public string? VolumeMode { get; init; }
    }

    private class RancherPersistentVolumeClaimResources
    {
        [JsonPropertyName("requests")] public required Dictionary<string, string> Requests { get; init; }
    }

    private class RancherPersistentVolumeClaimStatus
    {
        [JsonPropertyName("phase")] public required string Phase { get; init; }
        [JsonPropertyName("accessModes")] public List<string> AccessModes { get; init; } = [];
        [JsonPropertyName("capacity")] public Dictionary<string, string> Capacity { get; init; } = new();
    }

    private class RancherPersistentVolumeClaimList
    {
        [JsonPropertyName("data")] public List<RancherPersistentVolumeClaim> Data { get; init; } = [];
    }
}