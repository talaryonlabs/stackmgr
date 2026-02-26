using StackManager.Shared.Models;
using Talaryon.StackManager.Types;
using Talaryon.Toolbox.Api;
using Talaryon.Toolbox.Extensions;

namespace Talaryon.StackManager.Services;

public interface IProxyService
{
    Task<IEnumerable<Namespace>> GetNamespacesAsync();
    Task<Namespace?> GetNamespaceAsync(string name);
    Task<Namespace?> CreateNamespaceAsync(string name);
    Task<Namespace?> DeleteNamespaceAsync(string name);
    
    // Task<IEnumerable<Application>> GetApplicationsAsync();
    // Task<Application> GetApplicationAsync(string name);
    // // Task<Application> CreateApplicationAsync(Application body);
    // Task<Application> DeleteApplicationAsync(string name);
    //
    // Task<IEnumerable<Volume>> GetVolumesAsync();   
    // Task<Volume> GetVolumeAsync(string name);
    // // Task<Volume> CreateVolumeAsync(Volume volume);
    // Task<Volume> DeleteVolumeAsync(string name);
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
                  throw new Exception($"Remote not found for environment: {env.Name}");
        
        _client = new HttpClient();
        _client.BaseAddress = new Uri(_remote.Url);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {_remote.AccessToken.FromBase64String()}");
    }

    public async Task<IEnumerable<Namespace>> GetNamespacesAsync()
    {
        var response = await new ApiRequest<NamespaceList>(_client, _remote.Url)
            .WithType(ApiEndpointType.Many)
            .RunAsync();

        return response ?? [];
    }

    public async Task<Namespace?> GetNamespaceAsync(string name)
    {
        var request = new ApiRequest<Namespace>(_client, _remote.Url);
        request.WithType(ApiEndpointType.Single);
        request.WithParam("{name}", name);
        var response = await request.RunAsync();
        
        return response;
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
}