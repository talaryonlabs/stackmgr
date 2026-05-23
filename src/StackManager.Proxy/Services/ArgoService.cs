using System.Reflection;
using System.Text.Json;
using Microsoft.Extensions.Options;
using StackManager.Shared.Models;
using Talaryon.StackManager.Proxy.Utilities;
using Talaryon.Toolbox;
using Talaryon.Toolbox.Api.Errors;
using Talaryon.Toolbox.Services.ArgoCD.Models;

namespace Talaryon.StackManager.Proxy.Services;

public interface IArgoService
{
    ValueTask<IEnumerable<Application>> GetApplicationsAsync(CancellationToken cancellationToken = default);
    ValueTask<Application> GetApplicationAsync(string name, CancellationToken cancellationToken = default);
    ValueTask<Application> CreateApplicationAsync(Application body, CancellationToken cancellationToken = default);
    ValueTask<Application> UpdateApplicationAsync(string name, Application body, CancellationToken cancellationToken = default);
    ValueTask<Application> DeleteApplicationAsync(string name, CancellationToken cancellationToken = default);
    ValueTask<bool> RefreshApplicationAsync(string name, CancellationToken cancellationToken = default);
    ValueTask<bool> SyncApplicationAsync(string name, CancellationToken cancellationToken = default);   
    
    ValueTask<IEnumerable<Repository>> GetRepositoriesAsync(CancellationToken cancellationToken = default);   
    ValueTask<Repository> GetRepositoryAsync(string name, CancellationToken cancellationToken = default);
    ValueTask<Repository> CreateRepositoryAsync(Repository body, CancellationToken cancellationToken = default);
    ValueTask<Repository> DeleteRepositoryAsync(string name, CancellationToken cancellationToken = default);
}

public class ArgoOptions : TalaryonOptions<ArgoOptions>
{
    public string? Url { get; set; }
    public string? AccessToken { get; set; }
    public string? Project { get; set; }
}

public partial class ArgoService : IArgoService
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
        _client.DefaultRequestHeaders.Add("User-Agent", $"StackManager/{Assembly.GetExecutingAssembly().GetName().Version}");
    }

    public async ValueTask<IEnumerable<Application>> GetApplicationsAsync(CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync($"/api/v1/applications?project={_project}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InternalServerError("Failed to request applications. Please try again later.");
        }
        var applications = await response.Content.ReadFromJsonAsync<V1alpha1ApplicationList>(cancellationToken);
        var repositories = await GetRepositoriesAsync(cancellationToken);
        return (applications?.Items ?? []).Select(v => new Application
        {
            Name = v.Metadata.Name!,
            Project = v.Spec.Project,
            Repository = repositories.First(x => v.Spec.Source.RepoURL.StartsWith(x.Url)).Name,
            Path = v.Spec.Source.Path,
            IsAutoSyncEnabled = v.Spec.SyncPolicy is { Automated: not null },
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
            throw new InternalServerError("Failed to get application. Please try again later.");
        }
        
        var application = await response.Content.ReadFromJsonAsync<V1alpha1Application>(cancellationToken);
        if(application is null) throw new InternalServerError("Failed to get application due to an unexpected error.");

        var repositories = await GetRepositoriesAsync(cancellationToken);
        
        return new Application
        {
            Name = application.Metadata.Name!,
            Project = application.Spec.Project,
            Repository = repositories.First(x => application.Spec.Source.RepoURL.StartsWith(x.Url)).Name,
            Path = application.Spec.Source.Path,
            IsAutoSyncEnabled = application.Spec.SyncPolicy is { Automated: not null },
        };
    }

    public async ValueTask<Application> CreateApplicationAsync(Application body, CancellationToken cancellationToken = default)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(body.Name))
            throw new BadRequestError("Application name cannot be null or empty.");
        
        if (string.IsNullOrWhiteSpace(body.Repository))
            throw new BadRequestError("Repository cannot be null or empty.");
            
        if (string.IsNullOrWhiteSpace(body.Path))
            throw new BadRequestError("Path cannot be null or empty.");
        
        // Validate name format (alphanumeric and hyphens only, max 63 chars)
        if (!RegexPatterns.IsValidKubernetesName(body.Name))
            throw new BadRequestError("Application name must be valid Kubernetes DNS name (alphanumeric and hyphens only, max 63 chars).");
        
        try
        {
            await GetApplicationAsync(body.Name, cancellationToken);
            throw new ConflictError($"Application '{body.Name}' already exists.");
        }
        catch (NotFoundError) { }
        
        var repository = await GetRepositoryAsync(body.Repository, cancellationToken);
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
                    { "project", _project },
                    { "destination", new Dictionary<string, object>()
                        {
                            { "server", "https://kubernetes.default.svc" }
                        }
                    },
                    { "source", new Dictionary<string, object>()
                        {
                            { "repoURL", repository.Url ?? throw new BadRequestError("Missing repository URL.") },
                            { "path", body.Path ?? throw new BadRequestError("Missing path.") },
                            { "targetRevision", "HEAD" }
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

    public async ValueTask<Application> UpdateApplicationAsync(string name, Application body, CancellationToken cancellationToken = default)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(name))
            throw new BadRequestError("Application name cannot be null or empty.");
            
        if (string.IsNullOrWhiteSpace(body.Repository))
            throw new BadRequestError("Repository cannot be null or empty.");
            
        if (string.IsNullOrWhiteSpace(body.Path))
            throw new BadRequestError("Path cannot be null or empty.");
        
        var application = await GetApplicationAsync(name, cancellationToken);
        var repository = await GetRepositoryAsync(body.Repository, cancellationToken);
        var json = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            {
                "metadata", new Dictionary<string, object>()
                {
                    { "name", application.Name }
                }
            },
            {
                "spec", new Dictionary<string, object>
                {
                    { "project", _project },
                    { "destination", new Dictionary<string, object>()
                        {
                            { "server", "https://kubernetes.default.svc" }
                        }
                    },
                    { "source", new Dictionary<string, object>()
                        {
                            { "repoURL", repository.Url },
                            { "path", body.Path },
                            { "targetRevision", "HEAD" }
                        }
                    },
                    {
                        "syncPolicy", new V1alpha1SyncPolicy
                        {
                            Automated = body.IsAutoSyncEnabled ? new V1alpha1SyncPolicyAutomated
                            {
                                Prune = true,
                                SelfHeal = true
                            } : null!
                        }
                    }
                }
            }
        });

        var response = await _client.PutAsync($"/api/v1/applications/{application.Name}?validate=false", new StringContent(json)
        {
            Headers = { ContentType = new("application/json") }
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InternalServerError(
                $"Failed to update application '{application.Name}'. Response code: {response.StatusCode}, {await response.Content.ReadAsStringAsync(cancellationToken)}");
        }
        return await GetApplicationAsync(application.Name, cancellationToken);
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
                $"Failed to delete application '{app.Name}'. Response code: {response.StatusCode} ({await response.Content.ReadAsStringAsync(cancellationToken)})");
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
        var app = await GetApplicationAsync(name, cancellationToken);
        var json = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            { "prune", true }
        });
        
        var response = await _client.PostAsync($"/api/v1/applications/{app.Name}/sync", new StringContent(json)
        {
            Headers = { ContentType = new("application/json") }
        }, cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InternalServerError(
                $"Failed to sync application '{app.Name}'. Response code: {response.StatusCode}");
        }

        return true;
    }

    public async ValueTask<IEnumerable<Repository>> GetRepositoriesAsync(CancellationToken cancellationToken = default)
    {
        var response = await _client.GetAsync("/api/v1/repositories", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InternalServerError($"Failed to request repositories. Response code: {response.StatusCode}");
        }
        
        var repositories = await response.Content.ReadFromJsonAsync<V1alpha1RepositoryList>(cancellationToken);
        return (repositories?.Items ?? []).Select(v => new Repository
        {
            Name = v.Name,
            Url = v.Repo,
            Username = "(internal)",
            Password = "(internal)"
        });
    }

    public async ValueTask<Repository> GetRepositoryAsync(string name, CancellationToken cancellationToken = default)
    {
        var repositories = await GetRepositoriesAsync(cancellationToken);
        var local = repositories.SingleOrDefault(x => x.Name == name);
        if (local is null)
        {
            throw new NotFoundError($"Repository '{name}' not found.");
        }

        var response = await _client.GetAsync($"/api/v1/repositories/{Uri.EscapeDataString(local.Url)}", cancellationToken);
        if (!response.IsSuccessStatusCode)
        {
            throw new InternalServerError($"Failed to get repository '{name}'. Response code: {response.StatusCode}");
        }
        
        var repository = await response.Content.ReadFromJsonAsync<V1alpha1Repository>(cancellationToken);
        if(repository is null) throw new InternalServerError($"Failed to get repository '{name}'. (unknown error)");

        return new Repository
        {
            Name = repository.Name,
            Url = repository.Repo,
            Username = "(internal)",
            Password = "(internal)"
        };
    }

    public async ValueTask<Repository> CreateRepositoryAsync(Repository body, CancellationToken cancellationToken = default)
    {
        // Input validation
        if (string.IsNullOrWhiteSpace(body.Name))
            throw new BadRequestError("Repository name cannot be null or empty.");
            
        if (string.IsNullOrWhiteSpace(body.Url))
            throw new BadRequestError("Repository URL cannot be null or empty.");
            
        // Validate URL format
        if (!RegexPatterns.IsValidUrl(body.Url))
            throw new BadRequestError("Repository URL must be a valid absolute URI.");
            
        // Validate name format (alphanumeric and hyphens only, max 63 chars)
        if (!RegexPatterns.IsValidKubernetesName(body.Name))
            throw new BadRequestError("Repository name must be valid Kubernetes DNS name (alphanumeric and hyphens only, max 63 chars).");
        
        var repository = (await GetRepositoriesAsync(cancellationToken))
            .SingleOrDefault(x => x.Name == body.Name || x.Url == body.Url);
        if (repository is not null)
        {
            if (repository.Name == body.Name) throw new ConflictError($"Repository '{body.Name}' already exists.");
            if (repository.Url == body.Url) throw new ConflictError($"Repository with url '{body.Url}' already exists as '{repository.Name}'.");
        }
        
        var json = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            { "name", body.Name },
            { "repo", body.Url },
            { "username", string.IsNullOrWhiteSpace(body.Username) ? "" : body.Username },
            { "password", string.IsNullOrWhiteSpace(body.Password) ? "" : body.Password }
        });

        var response = await _client.PostAsync("/api/v1/repositories?validate=false", new StringContent(json)
        {
            Headers = { ContentType = new("application/json") }
        }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            var error = await response.Content.ReadAsStringAsync(cancellationToken);
            if (error.Contains("Invalid username or token"))
            {
                throw new InternalServerError($"Failed to create repository '{body.Name}'. Authentication failed.");
            }
            
            throw new InternalServerError(
                $"Failed to delete repository '{body.Name}'. Response code: {response.StatusCode}.");
        }
        return await GetRepositoryAsync(body.Name, cancellationToken);
    }

    public async ValueTask<Repository> DeleteRepositoryAsync(string name, CancellationToken cancellationToken = default)
    {
        var repository = await GetRepositoryAsync(name, cancellationToken);
        var response =
            await _client.SendAsync(new HttpRequestMessage(HttpMethod.Delete, $"/api/v1/repositories/{Uri.EscapeDataString(repository.Url)}")
            {
                Content = new StringContent("{}"){ Headers = { ContentType = new("application/json") } }
            }, cancellationToken);

        if (!response.IsSuccessStatusCode)
        {
            throw new InternalServerError("Failed to delete repository. Please try again later.");
        }

        return repository;
    }
}