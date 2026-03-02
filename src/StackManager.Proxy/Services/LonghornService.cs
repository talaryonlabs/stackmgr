using System.Net;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using StackManager.Shared.Models;
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

public class LonghornService : ILonghornService
{
    private readonly HttpClient _client;

    public LonghornService(IHttpClientFactory clientFactory, IOptions<LonghornOptions> options)
    {
        var url = options.Value.Url ?? throw new ArgumentNullException(nameof(options.Value.Url));
        var token = options.Value.AccessToken ?? throw new ArgumentNullException(nameof(options.Value.AccessToken));
        
        _client = clientFactory.CreateClient();
        _client.BaseAddress = new Uri(url);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        _client.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async ValueTask<IEnumerable<Volume>> GetVolumesAsync(CancellationToken cancellationToken)
    {
        var response = await _client.GetAsync("/v1/volumes", cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to request volumes. Response code: {response.StatusCode}");

        var list = await response.Content.ReadFromJsonAsync<LonghornServiceVolumeList>(cancellationToken);
        return list?.Data.Select(v => new Volume
        {
            Name = v.Name,
            Size = v.Size,
            NumberOfReplicas = v.NumberOfReplicas,
            State = v.State,
            AccessMode = v.AccessMode,
            Frontend = v.Frontend,
            Labels = v.Labels ?? []
        }) ?? [];
    }

    public async ValueTask<Volume> GetVolumeAsync(string name, CancellationToken cancellationToken)
    {
        var response = await _client.GetAsync($"/v1/volumes/{name}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
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
            Size = data.Size,
            NumberOfReplicas = data.NumberOfReplicas,
            State = data.State,
            AccessMode = data.AccessMode,
            Frontend = data.Frontend,
            Labels = data.Labels ?? []
        };
    }

    public async ValueTask<Volume> CreateVolumeAsync(Volume volume, CancellationToken cancellationToken = default)
    {
        try
        {
            await GetVolumeAsync(volume.Name, cancellationToken);
            throw new ConflictError($"Volume '{volume.Name}' already exists.");
        }
        catch (NotFoundError) { }

        var request = new Dictionary<string, object>
        {
            { "name", volume.Name },
            { "size", volume.Size },
            { "numberOfReplicas", volume.NumberOfReplicas },
            { "frontend", volume.Frontend },
            { "accessMode", volume.AccessMode },
            { "labels", volume.Labels ?? new Dictionary<string, string>() }
        };

        var response = await _client.PostAsJsonAsync("/v1/volumes", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
            throw new InternalServerError($"Failed to create volume '{volume.Name}'. Response code: {response.StatusCode}");

        var data = await response.Content.ReadFromJsonAsync<LonghornServiceVolume>(cancellationToken);
        if (data is null)
            throw new InternalServerError($"Failed to create volume '{volume.Name}'. (unknown error)");

        return new Volume
        {
            Name = data.Name,
            Size = data.Size,
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
