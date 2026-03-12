using System.Net;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using StackManager.Shared.Models;
using Talaryon.StackManager.Proxy.Utilities;
using Talaryon.Toolbox;
using Talaryon.Toolbox.Api.Errors;

namespace Talaryon.StackManager.Proxy.Services;

public interface ILonghornService
{
    ValueTask<IEnumerable<Volume>> GetVolumesAsync(CancellationToken cancellationToken = default);
    ValueTask<Volume> GetVolumeAsync(string name, CancellationToken cancellationToken = default);
    ValueTask<Volume> CreateVolumeAsync(Volume volume, CancellationToken cancellationToken = default);
    ValueTask<Volume> DeleteVolumeAsync(string name, CancellationToken cancellationToken = default);
}

public class LonghornOptions : TalaryonOptions<LonghornOptions>
{
    public string? Url { get; set; }
    public string? AccessToken { get; set; }   
}

public partial class LonghornService : ILonghornService
{
    private readonly HttpClient _client;

    public LonghornService(IHttpClientFactory clientFactory, IOptions<LonghornOptions> options)
    {
        var url = options.Value.Url ?? throw new ArgumentNullException(nameof(options.Value.Url));
        var token = options.Value.AccessToken ?? throw new ArgumentNullException(nameof(options.Value.AccessToken));
        
        Console.WriteLine($"Service added with base url '{url}' and access token '{token}'");
        
        _client = clientFactory.CreateClient();
        _client.BaseAddress = new Uri(url);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        _client.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async ValueTask<IEnumerable<Volume>> GetVolumesAsync(CancellationToken cancellationToken)
    {
        var response = await _client.GetAsync("/v1/volumes", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(await response.Content.ReadAsStringAsync(cancellationToken));
            throw new InternalServerError($"Failed to request volumes. Response code: {response.StatusCode}");
        }

        var list = await response.Content.ReadFromJsonAsync<LonghornServiceVolumeList>(cancellationToken);
        return (list?.Data ?? []).Select(v => new Volume
        {
            Name = v.Name,
            Size = TalaryonHelper.FormatNamedSize((ulong)v.Size),
            NumberOfReplicas = v.NumberOfReplicas,
            State = v.State,
            AccessMode = v.AccessMode,
            Frontend = v.Frontend,
            Labels = v.Labels ?? []
        });
    }

    public async ValueTask<Volume> GetVolumeAsync(string name, CancellationToken cancellationToken)
    {
        var response = await _client.GetAsync($"/v1/volumes/{name}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(await response.Content.ReadAsStringAsync(cancellationToken));
            throw response.StatusCode switch
            {
                HttpStatusCode.NotFound => new NotFoundError($"Volume '{name}' not found."),
                _ => new InternalServerError()
            };
        }

        var data = await response.Content.ReadFromJsonAsync<LonghornServiceVolume>(cancellationToken);
        if (data is null)
            throw new InternalServerError($"Failed to get volume '{name}'. (unknown error)");

        return new Volume
        {
            Name = data.Name,
            Size = TalaryonHelper.FormatNamedSize((ulong)data.Size),
            NumberOfReplicas = data.NumberOfReplicas,
            State = data.State,
            AccessMode = data.AccessMode,
            Frontend = data.Frontend,
            Labels = data.Labels ?? []
        };
    }

    public async ValueTask<Volume> CreateVolumeAsync(Volume volume, CancellationToken cancellationToken = default)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(volume.Name))
            throw new BadRequestError("Volume name cannot be null or empty.");
            
        if (string.IsNullOrWhiteSpace(volume.Size))
            throw new BadRequestError("Volume size cannot be null or empty.");
            
        if (string.IsNullOrWhiteSpace(volume.AccessMode))
            throw new BadRequestError("Access mode cannot be null or empty.");
            
        if (string.IsNullOrWhiteSpace(volume.Frontend))
            throw new BadRequestError("Frontend cannot be null or empty.");
            
        // Validate name format
        if (!RegexPatterns.IsValidKubernetesName(volume.Name))
            throw new BadRequestError("Volume name must be valid Kubernetes DNS name (alphanumeric and hyphens only, max 63 chars).");
            
        // Validate access mode
        if (volume.AccessMode != "ReadWriteOnce" && volume.AccessMode != "ReadWriteMany" && volume.AccessMode != "ReadOnlyMany")
            throw new BadRequestError("Access mode must be one of: ReadWriteOnce, ReadWriteMany, ReadOnlyMany");
            
        // Validate frontend
        if (volume.Frontend != "blockdev")
            throw new BadRequestError("Frontend must be 'blockdev'");
            
        // Validate storage size format
        if (!RegexPatterns.IsValidStorageSize(volume.Size))
            throw new BadRequestError("Volume size must be a valid quantity (e.g., 10Gi, 500M).");
        
        try
        {
            await GetVolumeAsync(volume.Name, cancellationToken);
            throw new ConflictError($"Volume '{volume.Name}' already exists.");
        }
        catch (NotFoundError) { }

        var request = new Dictionary<string, object>
        {
            { "name", volume.Name },
            { "size", TalaryonHelper.ParseNamedSize(volume.Size).ToString() },
            { "numberOfReplicas", volume.NumberOfReplicas },
            { "frontend", volume.Frontend },
            { "accessMode", volume.AccessMode switch
            {
                "ReadWriteOnce" => "rwo",
                "ReadWriteMany" => "rwx",
                _ => throw new ArgumentException($"Invalid access mode: {volume.AccessMode}")
            } },
            { "labels", volume.Labels }
        };

        var response = await _client.PostAsJsonAsync("/v1/volumes", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine(await response.Content.ReadAsStringAsync(cancellationToken));
            throw new InternalServerError($"Failed to create volume '{volume.Name}'. Response code: {response.StatusCode}");
        }

        var data = await response.Content.ReadFromJsonAsync<LonghornServiceVolume>(cancellationToken);
        if (data is null)
            throw new InternalServerError($"Failed to create volume '{volume.Name}'. (unknown error)");

        return new Volume
        {
            Name = data.Name,
            Size = TalaryonHelper.FormatNamedSize((ulong)data.Size),
            NumberOfReplicas = data.NumberOfReplicas,
            State = data.State,
            AccessMode = data.AccessMode,
            Frontend = data.Frontend,
            Labels = data.Labels ?? []
        };
    }

    public async ValueTask<Volume> DeleteVolumeAsync(string name, CancellationToken cancellationToken)
    {
        var volume = await GetVolumeAsync(name, cancellationToken);
        var response = await _client.DeleteAsync($"/v1/volumes/{name}", cancellationToken);
        return !response.IsSuccessStatusCode
            ? throw new InternalServerError($"Failed to delete volume '{name}'. Response code: {response.StatusCode}")
            : volume;
    }

    public class LonghornServiceVolume
    {
        [JsonPropertyName("name")] public required string Name { get; init; }
        [JsonPropertyName("size")] public required long Size { get; init; } // in Bytes
        [JsonPropertyName("accessMode")] public required string AccessMode { get; init; } // "rwo", "rwm", "rox"
        [JsonPropertyName("numberOfReplicas")] public required int NumberOfReplicas { get; init; }
        [JsonPropertyName("frontend")] public required string Frontend { get; init; } // "blockdev"
        [JsonPropertyName("state")] public string? State { get; init; } // z. B. "attached", "detached"
        [JsonPropertyName("labels")] public Dictionary<string, string>? Labels { get; init; }
    }

    public class LonghornServiceVolumeList
    {
        [JsonPropertyName("data")] public List<LonghornServiceVolume> Data { get; init; } = [];
    }
}
