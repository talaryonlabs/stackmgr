using System.Net;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Options;
using StackManager.Shared.Models;
using Talaryon.Toolbox;
using Talaryon.Toolbox.Api.Errors;

namespace Talaryon.StackManager.Proxy.Services;

public interface IRancherService
{
    ValueTask<IEnumerable<Namespace>> GetNamespacesAsync(CancellationToken cancellationToken = default);
    ValueTask<Namespace> GetNamespaceAsync(string name, CancellationToken cancellationToken = default);
    ValueTask<Namespace> CreateNamespaceAsync(string name, CancellationToken cancellationToken = default);
    ValueTask<Namespace> DeleteNamespaceAsync(string name, CancellationToken cancellationToken = default);
}

public class RancherOptions : TalaryonOptions<RancherOptions>
{
    public string? Url { get; set; }
    public string? AccessToken { get; set; }
    public string? Project { get; set; }
}

public class RancherService : IRancherService
{
    private readonly HttpClient _client;
    private readonly string _project;

    public RancherService(HttpClient client, IOptions<RancherOptions> options)
    {
        var url = options.Value.Url ?? throw new ArgumentNullException(nameof(options.Value.Url));
        var token = options.Value.AccessToken ?? throw new ArgumentNullException(nameof(options.Value.AccessToken));
        
        _project = options.Value.Project ?? throw new ArgumentNullException(nameof(options.Value.Project));
        
        _client = client;
        _client.BaseAddress = new Uri(url);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        _client.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async ValueTask<IEnumerable<Namespace>> GetNamespacesAsync(CancellationToken cancellationToken)
    {
        var response = await _client.GetAsync($"/v3/cluster/local/namespaces?projectId={_project}", cancellationToken);
        if (!response.IsSuccessStatusCode) throw new Exception($"Failed to request namespaces. Response code: {response.StatusCode}");
        var list = await response.Content.ReadFromJsonAsync<RancherServiceNamespaceList>(cancellationToken);

        return list?.Data.Select(v => new Namespace { Name = v.Name, Project = _project }) ?? [];
    }
    
    public async ValueTask<Namespace> GetNamespaceAsync(string name, CancellationToken cancellationToken)
    {
        var response = await _client.GetAsync($"/v3/cluster/local/namespaces/{name}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw response.StatusCode switch
            {
                HttpStatusCode.NotFound => new NotFoundError($"Namespace '{name}' not found."),
                _ => new InternalServerError()
            };
        }

        var data = await response.Content.ReadFromJsonAsync<RancherServiceNamespace>(cancellationToken);
        if (data is null) throw new InternalServerError($"Failed to get namespace '{name}'. (unknown error)");
        return new Namespace
        {
            Name = data.Name, Project = _project
        };
    }
    
    public async ValueTask<Namespace> CreateNamespaceAsync(string name, CancellationToken cancellationToken)
    {
        try
        {
            await GetNamespaceAsync(name, cancellationToken);
            throw new ConflictError($"Namespace '{name}' already exists.");
        }
        catch (NotFoundError) { }
        
        var request = new Dictionary<string, string?>
        {
            { "containerDefaultResourceLimit", null },
            { "name", name },
            { "projectId", _project },
            { "resourceQuota", null }
        };
        
        var response = await _client.PostAsJsonAsync("/v3/cluster/local/namespaces", request, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InternalServerError($"Failed to create namespace '{name}'. Response code: {response.StatusCode}");
        }
        
        var data = await response.Content.ReadFromJsonAsync<RancherServiceNamespace>(cancellationToken);
        if (data is null) throw new InternalServerError($"Failed to create namespace '{name}'. (unknown error)");
        return new Namespace
        {
            Name = data.Name, Project = _project
        };
    }

    public async ValueTask<Namespace> DeleteNamespaceAsync(string name, CancellationToken cancellationToken)
    {
        var ns = await GetNamespaceAsync(name, cancellationToken);
        var response = await _client.DeleteAsync($"/v3/cluster/local/namespaces/{name}", cancellationToken);
        return !response.IsSuccessStatusCode
            ? throw new InternalServerError(
                $"Failed to delete namespace '{name}'. Response code: {response.StatusCode}")
            : ns;
    }

    public class RancherServiceNamespace
    {
        [JsonPropertyName("name")] public required string Name { get; init; }
    }

    public class RancherServiceNamespaceList
    {
        [JsonPropertyName("data")] public List<RancherServiceNamespace> Data { get; init; } = [];
    }
}