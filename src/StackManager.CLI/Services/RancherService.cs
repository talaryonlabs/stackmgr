using System.Net;
using System.Net.Http.Json;
using System.Text.Json.Serialization;
using Talaryon.StackManager.Types;
using Talaryon.Toolbox.Extensions;

namespace Talaryon.StackManager.Services;

public class RancherService : IDisposable
{
    private readonly StackEnvironmentRancher _rancher;
    private readonly HttpClient _client;
    
    public RancherService(StackEnvironment env)
    {
        _rancher = env.Rancher;
        
        var accessToken = _rancher.GetAccessToken(env);
        if (accessToken is null or "")
            throw new Exception("No access token provided. Please check your configuration.");
        
        if (_rancher.Url is null or "")
            throw new Exception("No RKE2 URL provided. Please check your configuration.");
        
        if (_rancher.ProjectId is null or "")
            throw new Exception("No RKE2 project ID provided. Please check your configuration.");
        
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken.FromBase64String()}");
        _client.DefaultRequestHeaders.Add("Accept", "application/json");
    }

    public async Task TestAsync()
    {
        Console.WriteLine($" - API URL: {_rancher.Url}");
        Console.WriteLine($" - Project: {_rancher.ProjectId}");

        var response =
            await _client.GetAsync(
                $"{_rancher.Url}/v3/projects/{_rancher.ProjectId}");

        if (response.IsSuccessStatusCode) return;
        throw response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => new Exception("Unauthorized. Please check your access token."),
            HttpStatusCode.NotFound => new Exception("Project not found. Please check your project ID."),
            _ => new Exception($"Failed. Response code: {response.StatusCode}")
        };
    }
    
    public async Task<IEnumerable<RancherServiceNamespace>> GetNamespacesAsync()
    {
        var response = await _client.GetAsync($"{_rancher.Url}/v3/cluster/local/namespaces?projectId={_rancher.ProjectId}");
        if (!response.IsSuccessStatusCode) throw new Exception($"Failed to request namespaces. Response code: {response.StatusCode}");
        var list = await response.Content.ReadFromJsonAsync<RancherServiceNamespaceList>();

        return list?.Data ?? [];
    }

    public async Task<RancherServiceNamespace?> GetNamespaceAsync(Stack stack)
    {
        var response = await _client.GetAsync($"{_rancher.Url}/v3/cluster/local/namespaces/{stack.Namespace}");
        if (!response.IsSuccessStatusCode)
        {
            return response.StatusCode switch
            {
                HttpStatusCode.NotFound => null,
                _ => throw new Exception($"Failed to request namespace '{stack.Namespace}'. Response code: {response.StatusCode}")
            };
        }
        return await response.Content.ReadFromJsonAsync<RancherServiceNamespace>();
    }

    public async Task CreateNamespaceAsync(Stack stack)
    {
        var ns = await GetNamespaceAsync(stack);
        if (ns is not null) throw new Exception($"Namespace '{stack.Namespace}' already exists.");
        
        var request = new Dictionary<string, string?>
        {
            { "containerDefaultResourceLimit", null },
            { "name", stack.Namespace },
            { "projectId", _rancher.ProjectId },
            { "resourceQuota", null }
        };
        
        var response = await _client.PostAsJsonAsync($"{_rancher.Url}/v3/cluster/local/namespaces", request);
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to create namespace '{stack.Namespace}'. Response code: {response.StatusCode}");
        }
    }

    public async Task DeleteNamespaceAsync(Stack stack)
    {
        var ns = await GetNamespaceAsync(stack);
        if (ns is null) throw new Exception($"Namespace '{stack.Namespace}' not found.");
        
        var response = await _client.DeleteAsync($"{_rancher.Url}/v3/cluster/local/namespaces/{stack.Namespace}");
        if (!response.IsSuccessStatusCode)
        {
            throw new Exception($"Failed to delete namespace '{stack.Namespace}'. Response code: {response.StatusCode}");
        }
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}

public class RancherServiceNamespace
{
    [JsonPropertyName("name")] public string? Name { get; set; }
}

public class RancherServiceNamespaceList
{
    [JsonPropertyName("data")] public List<RancherServiceNamespace>? Data { get; set; }
}