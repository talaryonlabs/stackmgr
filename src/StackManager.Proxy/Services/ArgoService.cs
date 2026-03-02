using System.Text.Json;
using Microsoft.Extensions.Options;
using StackManager.Shared.Models;
using Talaryon.Toolbox;
using Talaryon.Toolbox.Api.Errors;
using Talaryon.Toolbox.Services.ArgoCD.Models;

namespace Talaryon.StackManager.Proxy.Services;

public interface IArgoService
{
    ValueTask<IEnumerable<Application>> GetApplicationsAsync(CancellationToken cancellationToken = default);
    ValueTask<Application> GetApplicationAsync(string name, CancellationToken cancellationToken = default);
    ValueTask<Application> CreateApplicationAsync(Application body, CancellationToken cancellationToken = default);
    ValueTask<Application> DeleteApplicationAsync(string name, CancellationToken cancellationToken = default);
    ValueTask<bool> RefreshApplicationAsync(string name, CancellationToken cancellationToken = default);
    ValueTask<bool> SyncApplicationAsync(string name, CancellationToken cancellationToken = default);   
}

public class ArgoOptions : TalaryonOptions<ArgoOptions>
{
    public string? Url { get; set; }
    public string? AccessToken { get; set; }
    public string? Project { get; set; }
}

public class ArgoService : IArgoService
{
    private readonly HttpClient _client;
    private readonly string _project;

    public ArgoService(IHttpClientFactory clientFactory, IOptions<ArgoOptions> options)
    {
        var url = options.Value.Url ?? throw new ArgumentNullException(nameof(options.Value.Url));
        var token = options.Value.AccessToken ?? throw new ArgumentNullException(nameof(options.Value.AccessToken));
        
        _project = options.Value.Project ?? throw new ArgumentNullException(nameof(options.Value.Project));
        
        _client = clientFactory.CreateClient();
        _client.BaseAddress = new Uri(url);
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        _client.DefaultRequestHeaders.Add("Accept", [
            "application/json",
            "application/x-www-form-urlencoded"
        ]);
    }

    public async ValueTask<IEnumerable<Application>> GetApplicationsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync($"/api/v1/applications?project={_project}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InternalServerError($"Failed to request applications. Response code: {response.StatusCode}");
        }
        
        var applications = await response.Content.ReadFromJsonAsync<V1alpha1ApplicationList>(cancellationToken);
        if (applications is null) throw new InternalServerError("Failed to request applications. (unknown error)");
        
        return applications.Items.Select(v => new Application
        {
            Name = v.Metadata.Name!,
            Project = v.Spec.Project,
            Repository = v.Spec.Source.RepoURL,
            Path = v.Spec.Source.Path,
            TargetRevision = v.Spec.Source.TargetRevision,
            IsAutoSyncEnabled = v.Spec.SyncPolicy is not null,
        });
    }

    public async ValueTask<Application> GetApplicationAsync(string name, CancellationToken cancellationToken = default)
    {
        var apps = await GetApplicationsAsync(cancellationToken);
        if (apps.All(x => x.Name != name))
        {
            throw new NotFoundError($"Application '{name}' not found.");
        }
        
        var response = await _client.GetAsync($"/api/v1/applications/{name}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InternalServerError($"Failed to get application '{name}'. Response code: {response.StatusCode}");
        }
        
        var application = await response.Content.ReadFromJsonAsync<V1alpha1Application>(cancellationToken);
        if(application is null) throw new InternalServerError($"Failed to get application '{name}'. (unknown error)");

        return new Application
        {
            Name = application.Metadata.Name!,
            Project = application.Spec.Project,
            Repository = application.Spec.Source.RepoURL,
            Path = application.Spec.Source.Path,
            TargetRevision = application.Spec.Source.TargetRevision,
            IsAutoSyncEnabled = application.Spec.SyncPolicy is not null,
        };
    }

    public async ValueTask<Application> CreateApplicationAsync(Application body, CancellationToken cancellationToken = default)
    {
        try
        {
            await GetApplicationAsync(body.Name, cancellationToken);
            throw new ConflictError($"Namespace '{body.Name}' already exists.");
        }
        catch (NotFoundError) { }
        
        var json = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            {
                "metadata", new Dictionary<string, object>()
                {
                    { "name", body.Name }
                }
            },
            {
                "spec", new Dictionary<string, object>
                {
                    { "destination", new Dictionary<string, object>()
                        {
                            { "server", "https://kubernetes.default.svc" }
                        }
                    },
                    { "source", new Dictionary<string, object>()
                        {
                            { "repoURL", body.Repository ?? throw new BadRequestError() },
                            { "path", body.Path ?? throw new BadRequestError() },
                            { "targetRevision", body.TargetRevision ?? throw new BadRequestError() }
                        }
                    }
                }
            }
        });

        var response = await _client.PostAsync("/api/v1/applications?validate=false", new StringContent(json)
        {
            Headers = { ContentType = new("application/json") }
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InternalServerError(
                $"Failed to create application '{body.Name}'. Response code: {response.StatusCode}");
        }
        return await GetApplicationAsync(body.Name, cancellationToken);
    }

    public async ValueTask<Application> DeleteApplicationAsync(string name, CancellationToken cancellationToken = default)
    {
        var app = await GetApplicationAsync(name, cancellationToken);
        var response =
            await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/applications/{app.Name}")
            {
                Content = new StringContent("{}"){ Headers = { ContentType = new("application/json") } }
            }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InternalServerError(
                $"Failed to delete application '{app.Name}'. Response code: {response.StatusCode} ({await response.Content.ReadAsStringAsync()})");
        }

        return app;
    }

    public async ValueTask<bool> RefreshApplicationAsync(string name, CancellationToken cancellationToken)
    {
        var app = await GetApplicationAsync(name, cancellationToken);
        var response = await _client.GetAsync($"/api/v1/applications/{app.Name}?refresh=hard", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InternalServerError(
                $"Failed to refresh application '{app.Name}'. Response code: {response.StatusCode}");
        }

        return true;
    }

    public async ValueTask<bool> SyncApplicationAsync(string name, CancellationToken cancellationToken)
    {
        throw new NotImplementedException();
    }
}