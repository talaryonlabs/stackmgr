using StackManager.Shared.Models;
using Talaryon.Toolbox.Api;

namespace Talaryon.StackManager.Services;

public interface IProxyService
{
    Task<bool> TestConnectionAsync();
    
    Task<IReadOnlyList<Namespace>> GetNamespacesAsync();
    Task<Namespace?> GetNamespaceAsync(string name);
    Task<Namespace?> CreateNamespaceAsync(string name);
    Task<Namespace?> DeleteNamespaceAsync(string name);
    
    Task<IReadOnlyList<Application>> GetApplicationsAsync();
    Task<Application?> GetApplicationAsync(string name);
    Task<Application?> CreateApplicationAsync(Application application);
    Task<Application?> UpdateApplicationAsync(string name, Application application);
    Task<Application?> DeleteApplicationAsync(string name);
    
    Task<IReadOnlyList<Repository>> GetRepositoriesAsync();   
    Task<Repository?> GetRepositoryAsync(string name);
    Task<Repository?> CreateRepositoryAsync(Repository repository);
    Task<Repository?> DeleteRepositoryAsync(string name);
    
    Task<IReadOnlyList<Volume>> GetVolumesAsync(string ns);   
    Task<Volume?> GetVolumeAsync(string ns, string name);
    Task<Volume?> CreateVolumeAsync(string ns, Volume volume);
    Task<Volume?> DeleteVolumeAsync(string ns, string name);
}

public class ProxyService : IProxyService, IDisposable
{
    private readonly HttpClient _client;
    private readonly LocalConfigRemote _remote;
    
    public ProxyService(LocalConfigRemote remote)
    {
        _remote = remote;
        _client = new HttpClient();
        _client.BaseAddress = new Uri(_remote.Url);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_remote.AccessToken}");
    }

    public async Task<bool> TestConnectionAsync()
    {
        var response = await new ApiRequest<Namespace>(_client, _remote.Url)
            .WithType(ApiEndpointType.Single)
            .WithParam("namespace", "kube-system")
            .RunAsync();

        if (!response.IsSuccessful)
        {
            if (response.Error != null)
                throw response.Error;
            else
                throw new Exception($"Connection failed. Status: {response.StatusCode}, URL: {_remote.Url}");
        }
        
        return response.IsSuccessful;
    }

    public async Task<IReadOnlyList<Namespace>> GetNamespacesAsync()
    {
        var response = await new ApiRequest<NamespaceList>(_client, _remote.Url)
            .WithType(ApiEndpointType.Many)
            .RunAsync();

        return response.Data ?? [];
    }

    public async Task<Namespace?> GetNamespaceAsync(string name)
    {
        var response = await new ApiRequest<Namespace>(_client, _remote.Url)
            .WithType(ApiEndpointType.Single)
            .WithParam("namespace", name)
            .RunAsync();

        return response.Data;
    }

    public async Task<Namespace?> CreateNamespaceAsync(string name)
    {
        var response = await new ApiRequest<Namespace>(_client, _remote.Url)
            .WithType(ApiEndpointType.Create)
            .WithContent(new Namespace
            {
                Name = name
            })
            .RunAsync();

        return response.Data;
    }

    public async Task<Namespace?> DeleteNamespaceAsync(string name)
    {
        var response = await new ApiRequest<Namespace>(_client, _remote.Url)
            .WithType(ApiEndpointType.Delete)
            .WithParam("namespace", name)
            .RunAsync();
        
        return response.Data;
    }

    public async Task<IReadOnlyList<Application>> GetApplicationsAsync()
    {
        var response = await new ApiRequest<ApplicationList>(_client, _remote.Url)
            .WithType(ApiEndpointType.Many)
            .RunAsync();

        return response.Data ?? [];
    }

    public async Task<Application?> GetApplicationAsync(string name)
    {
        var response = await new ApiRequest<Application>(_client, _remote.Url)
            .WithType(ApiEndpointType.Single)
            .WithParam("name", name)
            .RunAsync();
        
        return response.Data;
    }

    public async Task<Application?> CreateApplicationAsync(Application application)
    {
        var response = await new ApiRequest<Application>(_client, _remote.Url)
            .WithType(ApiEndpointType.Create)
            .WithContent(application)
            .RunAsync();

        return response.Data;
    }

    public async Task<Application?> UpdateApplicationAsync(string name, Application application)
    {
        var response = await new ApiRequest<Application>(_client, _remote.Url)
            .WithType(ApiEndpointType.Update)
            .WithParam("name", name)
            .WithContent(application)
            .RunAsync();

        return response.Data;
    }

    public async Task<Application?> DeleteApplicationAsync(string name)
    {
        var response = await new ApiRequest<Application>(_client, _remote.Url)
            .WithType(ApiEndpointType.Delete)
            .WithParam("name", name)
            .RunAsync();

        return response.Data;
    }

    public async Task<IReadOnlyList<Repository>> GetRepositoriesAsync()
    {
        var response = await new ApiRequest<RepositoryList>(_client, _remote.Url)
            .WithType(ApiEndpointType.Many)
            .RunAsync();

        return response.Data ?? [];
    }

    public async Task<Repository?> GetRepositoryAsync(string name)
    {
        var response = await new ApiRequest<Repository>(_client, _remote.Url)
            .WithType(ApiEndpointType.Single)
            .WithParam("name", name)
            .RunAsync();
        
        return response.Data;
    }

    public async Task<Repository?> CreateRepositoryAsync(Repository repository)
    {
        var response = await new ApiRequest<Repository>(_client, _remote.Url)
            .WithType(ApiEndpointType.Create)
            .WithContent(repository)
            .RunAsync();

        return response.Data;
    }

    public async Task<Repository?> DeleteRepositoryAsync(string name)
    {
        var response = await new ApiRequest<Repository>(_client, _remote.Url)
            .WithType(ApiEndpointType.Delete)
            .WithParam("name", name)
            .RunAsync();

        return response.Data;
    }

    public async Task<IReadOnlyList<Volume>> GetVolumesAsync(string ns)
    {
        var response = await new ApiRequest<VolumeList>(_client, _remote.Url)
            .WithType(ApiEndpointType.Many)
            .WithParam("namespace", ns)
            .RunAsync();

        return response.Data ?? [];
    }

    public async Task<Volume?> GetVolumeAsync(string ns, string name)
    {
        var response = await new ApiRequest<Volume>(_client, _remote.Url)
            .WithType(ApiEndpointType.Single)
            .WithParam("name", name)
            .WithParam("namespace", ns)
            .RunAsync();

        return response.Data;
    }

    public async Task<Volume?> CreateVolumeAsync(string ns, Volume volume)
    {
        var response = await new ApiRequest<Volume>(_client, _remote.Url)
            .WithType(ApiEndpointType.Create)
            .WithParam("namespace", ns)
            .WithContent(volume)
            .RunAsync();
        
        return response.Data;
    }

    public async Task<Volume?> DeleteVolumeAsync(string ns, string name)
    {
        var response = await new ApiRequest<Volume>(_client, _remote.Url)
            .WithType(ApiEndpointType.Delete)
            .WithParam("name", name)
            .WithParam("namespace", ns)
            .RunAsync();

        return response.Data;
    }

    public void Dispose()
    {
        _client.Dispose();
    }
}