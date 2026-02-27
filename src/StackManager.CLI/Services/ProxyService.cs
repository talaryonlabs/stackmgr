using StackManager.Shared.Models;
using Talaryon.StackManager.Types;
using Talaryon.Toolbox.Api;
using Talaryon.Toolbox.Extensions;

namespace Talaryon.StackManager.Services;

public interface IProxyService
{
    Task<IReadOnlyList<Namespace>> GetNamespacesAsync();
    Task<Namespace?> GetNamespaceAsync(string name);
    Task<Namespace?> CreateNamespaceAsync(string name);
    Task<Namespace?> DeleteNamespaceAsync(string name);
    
    Task<IReadOnlyList<Application>> GetApplicationsAsync();
    Task<Application?> GetApplicationAsync(string name);
    // Task<Application> CreateApplicationAsync(Application body);
    Task<Application?> DeleteApplicationAsync(string name);

    Task<IReadOnlyList<Volume>> GetVolumesAsync();   
    Task<Volume?> GetVolumeAsync(string name);
    Task<Volume?> CreateVolumeAsync(Volume volume);
    Task<Volume?> DeleteVolumeAsync(string name);
}

public class ProxyService : IProxyService
{
    private readonly StackEnvironment _env;
    private readonly LocalConfig _config;
    private readonly HttpClient _client;
    private readonly LocalConfigRemote _remote;
    
    public ProxyService(StackEnvironment env)
    {
        _env = env;
        _config = LocalConfig.Get();
        _remote = _config.Remotes.FirstOrDefault(x => x.Name == env.Remote) ??
                  throw new Exception($"Remote '{env.Remote}' not found for environment: {env.Name}");
        
        _client = new HttpClient();
        _client.BaseAddress = new Uri(_remote.Url);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_remote.AccessToken.FromBase64String()}");
    }

    public async Task<IReadOnlyList<Namespace>> GetNamespacesAsync()
    {
        var response = await new ApiRequest<NamespaceList>(_client, _remote.Url)
            .WithType(ApiEndpointType.Many)
            .RunAsync();

        return response ?? [];
    }

    public async Task<Namespace?> GetNamespaceAsync(string name)
    {
        try
        {
            return await new ApiRequest<Namespace>(_client, _remote.Url)
                .WithType(ApiEndpointType.Single)
                .WithParam("{name}", name)
                .RunAsync();
        }
        catch (ApiError e)
        {
            if (e.Code == 404) return null;
            throw;
        }
    }

    public Task<Namespace?> CreateNamespaceAsync(string name)
    {
        return new ApiRequest<Namespace>(_client, _remote.Url)
            .WithType(ApiEndpointType.Create)
            .WithContent(new Namespace
            {
                Name = name
            })
            .RunAsync();
    }

    public Task<Namespace?> DeleteNamespaceAsync(string name)
    {
        return new ApiRequest<Namespace>(_client, _remote.Url)
            .WithType(ApiEndpointType.Delete)
            .WithParam("{name}", name)
            .RunAsync();
    }

    public async Task<IReadOnlyList<Application>> GetApplicationsAsync()
    {
        var response = await new ApiRequest<ApplicationList>(_client, _remote.Url)
            .WithType(ApiEndpointType.Many)
            .RunAsync();

        return response ?? [];
    }

    public async Task<Application?> GetApplicationAsync(string name)
    {
        try
        {
            return await new ApiRequest<Application>(_client, _remote.Url)
                .WithType(ApiEndpointType.Single)
                .WithParam("{name}", name)
                .RunAsync();
        }
        catch (ApiError e)
        {
            if (e.Code == 404) return null;
            throw;
        }
    }

    public Task<Application?> DeleteApplicationAsync(string name)
    {
        return new ApiRequest<Application>(_client, _remote.Url)
            .WithType(ApiEndpointType.Delete)
            .WithParam("{name}", name)
            .RunAsync();
    }

    public async Task<IReadOnlyList<Volume>> GetVolumesAsync()
    {
        var response = await new ApiRequest<VolumeList>(_client, _remote.Url)
            .WithType(ApiEndpointType.Many)
            .RunAsync();

        return response ?? [];
    }

    public async Task<Volume?> GetVolumeAsync(string name)
    {
        try
        {
            return await new ApiRequest<Volume>(_client, _remote.Url)
                .WithType(ApiEndpointType.Single)
                .WithParam("{name}", name)
                .RunAsync();
        }
        catch (ApiError e)
        {
            if (e.Code == 404) return null;
            throw;
        }
    }

    public Task<Volume?> CreateVolumeAsync(Volume volume)
    {
        return new ApiRequest<Volume>(_client, _remote.Url)
            .WithType(ApiEndpointType.Create)
            .WithContent(volume)
            .RunAsync();
    }

    public Task<Volume?> DeleteVolumeAsync(string name)
    {
        return new ApiRequest<Volume>(_client, _remote.Url)
            .WithType(ApiEndpointType.Delete)
            .WithParam("{name}", name)
            .RunAsync();
    }
}