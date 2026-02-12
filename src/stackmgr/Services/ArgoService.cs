using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Talaryon.Toolbox.Extensions;
using Talaryon.Toolbox.Services.ArgoCD.Models;

namespace stackmgr.Services;

public class ArgoService : IDisposable
{
    private readonly StackEnvironmentArgo _argocd;
    private readonly HttpClient _client;

    public ArgoService(StackEnvironment environment)
    {
        _argocd = environment.Argo;
        
        var accessToken = _argocd.GetAccessToken(environment);
        if (accessToken is null or "")
            throw new Exception("No ArgoCD access token provided. Please check your configuration.");
        
        if (_argocd.Url is null or "")
            throw new Exception("No ArgoCD URL provided. Please check your configuration.");

        if (_argocd.Project is null or "")
            throw new Exception("No ArgoCD project provided. Please check your configuration.");

        if (_argocd.Repository is null or "")
            throw new Exception("No ArgoCD repository provided. Please check your configuration.");
        
        _client = new HttpClient();
        _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {accessToken.FromBase64String()}");
        _client.DefaultRequestHeaders.Add("Accept", [
            "application/json",
            "application/x-www-form-urlencoded"
        ]);
    }

    public async Task TestAsync()
    {
        Console.WriteLine($" - API URL: {_argocd.Url}");
        Console.WriteLine($" - Project: {_argocd.Project}");

        var response =
            await _client.GetAsync(
                $"{_argocd.Url}/api/v1/projects/{_argocd.Project}");

        if (response.IsSuccessStatusCode) return;
        throw response.StatusCode switch
        {
            HttpStatusCode.Unauthorized => new Exception("Unauthorized. Please check your access token."),
            HttpStatusCode.NotFound => new Exception("Project not found. Please check your project ID."),
            _ => new Exception($"Failed. Response code: {response.StatusCode}")
        };
    }

    public async Task<List<V1alpha1Application>> GetApplicationsAsync()
    {
        var response =
            await _client.GetAsync(
                $"{_argocd.Url}/api/v1/applications?project={_argocd.Project}");
        
        if (!response.IsSuccessStatusCode)
        {
            HelperMethods.LogError($"Failed to get applications. Response code: {response.StatusCode}");
            return [];
        }
        
        var applications = await response.Content.ReadFromJsonAsync<V1alpha1ApplicationList>();
        return applications?.Items ?? [];
    }

    public async Task<V1alpha1Application?> GetApplicationAsync(Stack stack)
    {
        var apps = await GetApplicationsAsync();
        if(apps.All(x => x.Metadata.Name != stack.Namespace)) return null;
        
        var response =
            await _client.GetAsync(
                $"{_argocd.Url}/api/v1/applications/{stack.Namespace}");

        if (!response.IsSuccessStatusCode)
            throw new Exception($"Failed to get application '{stack.Name}'. Response code: {response.StatusCode}");
        
        var application = await response.Content.ReadFromJsonAsync<V1alpha1Application>();
        if (application is null)
            throw new Exception($"Failed to get application '{stack.Name}'. (unknown error)");
        
        application.Metadata.DeletionTimestamp = DateTimeOffset.Now;
        application.Status.ObservedAt = DateTime.Now;
        
        return application;
    }
    
    public async Task<V1alpha1Application?> CreateApplicationAsync(Stack stack)
    {
        var list = await GetApplicationsAsync();
        if(list.Any(v => v.Metadata.Name == stack.Namespace))
        {
            throw new Exception($"Application '{stack.Namespace}' already exists in ArgoCD.");
        }
        
        var json = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            {
                "metadata", new Dictionary<string, object>()
                {
                    { "name", stack.Namespace }
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
                            { "repoURL", _argocd.Repository },
                            { "path", $"{stack.Environment.Name}/{stack.Name}" },
                            { "targetRevision", "HEAD" }
                        }
                    }
                }
            }
        });
        
        var request = new StringContent(json) { Headers = { ContentType = new("application/json") } };
        var response =
            await _client.PostAsync(
                $"{_argocd.Url}/api/v1/applications?validate=false", request);

        if (response.IsSuccessStatusCode) return await GetApplicationAsync(stack);
        throw new Exception($"Failed to create application '{stack.Namespace}'. Response code: {response.StatusCode} ({await response.Content.ReadAsStringAsync()})");
    }

    public async Task DeleteApplicationAsync(Stack stack)
    {
        var list = await GetApplicationsAsync();
        if(list.All(v => v.Metadata.Name != stack.Namespace))
        {
            throw new Exception($"Application '{stack.Namespace}' not found in ArgoCD.");
        }

        var request = new HttpRequestMessage(HttpMethod.Delete, $"{_argocd.Url}/api/v1/applications/{stack.Namespace}")
        {
            Content = new StringContent("{}"){ Headers = { ContentType = new("application/json") } }
        };
        var response =
            await _client.SendAsync(request);

        if (response.IsSuccessStatusCode) return;
        throw new Exception($"Failed to delete application '{stack.Namespace}'. Response code: {response.StatusCode} ({await response.Content.ReadAsStringAsync()})");
    }

    public async Task SetAutoSyncAsync(Stack stack, bool enabled)
    {
        var application = await GetApplicationAsync(stack);
        if (application is null)
        {
            throw new Exception($"Failed to get application '{stack.Namespace}'. (Unknown error)");
        }

        var json = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            {
                "metadata", new Dictionary<string, object>()
                {
                    { "name", application.Metadata.Name! }
                }
            },
            {
                "spec", new Dictionary<string, object>
                {
                    { "destination", application.Spec.Destination },
                    { "source", application.Spec.Source },
                    {
                        "syncPolicy", new V1alpha1SyncPolicy
                        {
                            Automated = enabled ? new V1alpha1SyncPolicyAutomated
                            {
                                Prune = true,
                                SelfHeal = true
                            } : null!
                        }
                    }
                }
            }
        });

        var request = new StringContent(json) { Headers = { ContentType = new("application/json") } };
        var response =
            await _client.PutAsync($"{_argocd.Url}/api/v1/applications/{stack.Namespace}?validate=false", request);

        if (response.IsSuccessStatusCode) return;
        throw new Exception($"Failed to set auto-sync for application '{stack.Namespace}'. Response code: {response.StatusCode} ({await response.Content.ReadAsStringAsync()})");
    }

    public async Task RefreshApplicationAsync(Stack stack)
    {
        var apps = await GetApplicationsAsync();
        if (apps.All(x => x.Metadata.Name != stack.Namespace))
        {
            throw new Exception($"Application '{stack.Namespace}' not found in ArgoCD.");
        }

        await _client.GetAsync($"{_argocd.Url}/api/v1/applications/{stack.Namespace}?refresh=hard");
    }

    public void Dispose() => _client.Dispose();
}