using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Talaryon.Toolbox.Extensions;
using Talaryon.Toolbox.Services.ArgoCD;
using Talaryon.Toolbox.Services.ArgoCD.Models;

namespace stackmgr.Services;

public class ArgoCD
{
    private static HttpClient NewClient(StackEnvironment env)
    {
        var rke2Client = new HttpClient();
        rke2Client.DefaultRequestHeaders.Add("Authorization", $"Bearer {env.ArgoCD.AccessToken.FromBase64String()}");
        rke2Client.DefaultRequestHeaders.Add("Accept", "application/json");

        return rke2Client;
    }

    public static bool CheckRequirements(StackEnvironment env)
    {
        if (env.ArgoCD.Url is null or "")
        {
            Console.WriteLine("No ArgoCD URL provided. Please check your configuration.");
            return false;
        }

        if (env.ArgoCD.AccessToken is null or "")
        {
            Console.WriteLine("No ArgoCD access token provided. Please check your configuration.");
            return false;
        }

        if (env.ArgoCD.Project is null or "")
        {
            Console.WriteLine("No ArgoCD project provided. Please check your configuration.");
            return false;
        }

        if (env.ArgoCD.Repository is null or "")
        {
            Console.WriteLine("No ArgoCD repository provided. Please check your configuration.");
            return false;
        }

        return true;
    }

    public static async Task<bool> TestConnection(StackEnvironment env)
    {
        if (!CheckRequirements(env)) return false;

        Console.WriteLine($" - API URL: {env.ArgoCD.Url}");
        Console.WriteLine($" - Project: {env.ArgoCD.Project}");

        var client = NewClient(env);
        var response =
            await client.GetAsync(
                $"{env.ArgoCD.Url}/api/v1/projects/{env.ArgoCD.Project}");
        switch (response.StatusCode)
        {
            case HttpStatusCode.Unauthorized:
                Console.WriteLine("Unauthorized. Please check your access token.");
                break;
            case HttpStatusCode.NotFound:
                Console.WriteLine("Namespace, service or project not found. Please check your configuration.");
                break;
            default:
                Console.WriteLine(response.IsSuccessStatusCode
                    ? "Success."
                    : $"Failed. Response code: {response.StatusCode}");
                break;
        }

        return response.IsSuccessStatusCode;
    }

    public static async Task<IEnumerable<V1alpha1Application>?> ListApplicationsAsync(StackEnvironment env)
    {
        if (!CheckRequirements(env)) return null;

        using var client = NewClient(env);
        var response =
            await client.GetAsync(
                $"{env.ArgoCD.Url}/api/v1/applications?project={env.ArgoCD.Project}");
        
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to get applications. Response code: {response.StatusCode}");
            return null;
        }
        
        var applications = await response.Content.ReadFromJsonAsync<V1alpha1ApplicationList>();
        return applications?.Items;
    }

    public static async Task<bool> CreateApplication(StackEnvironment env, string name)
    {
        if (!CheckRequirements(env)) return false;


        using var client = NewClient(env);
        var response =
            await client.GetAsync(
                $"{env.ArgoCD.Url}/api/v1/applications");

        var list = await response.Content.ReadFromJsonAsync<V1alpha1ApplicationList>();

        list.Items.ForEach(x => Console.WriteLine($"{x.Metadata.Name}"));


        var application = new V1alpha1Application()
        {
            Metadata = new V1ObjectMeta()
            {
                Name = name
            },
            Spec = new V1alpha1ApplicationSpec()
            {
                SyncPolicy = new V1alpha1SyncPolicy
                {
                    Automated = new V1alpha1SyncPolicyAutomated
                    {
                        Prune = true,
                        SelfHeal = true,
                        AllowEmpty = true
                    }
                },
                Source = new V1alpha1ApplicationSource
                {
                    RepoURL = "https://github.com/talaryonlabs/stackmgr.git",
                    Path = "examples/nginx",
                    TargetRevision = "HEAD"
                },
                Destination = new V1alpha1ApplicationDestination()
                {
                    Server = "https://kubernetes.default.svc"
                }
            }
        };


        return false;
    }

    public static async Task<V1alpha1Application?> GetApplication(StackEnvironment env, string name)
    {
        if (!CheckRequirements(env)) return null;

        using var client = NewClient(env);
        var response =
            await client.GetAsync(
                $"{env.ArgoCD.Url}/api/v1/applications/{name}");

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to get application '{name}'. Response code: {response.StatusCode}");
            return null;
        }
        
        var application = await response.Content.ReadFromJsonAsync<V1alpha1Application>();
        if (application is null)
        {
            Console.WriteLine($"Failed to get application '{name}'. (unknown error)");
            return null;
        }
        
        application.Metadata.DeletionTimestamp = DateTimeOffset.Now;
        application.Status.ObservedAt = DateTime.Now;
        
        return application;
    }

    public static async Task<bool> DeleteApplication(StackEnvironment env, string name)
    {
        return false;
    }

    public static async Task<bool> EnableAutoSync(StackEnvironment env, string name)
    {
        var application = await GetApplication(env, name);
        if (application is null)
        {
            Console.WriteLine($"Failed to get application '{name}'. (Unknown error)");
            return false;
        }

        var json = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            {
                "metadata", new Dictionary<string, object>()
                {
                    { "name", application.Metadata.Name }
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
                            Automated = new V1alpha1SyncPolicyAutomated
                            {
                                Prune = true,
                                SelfHeal = true
                            }
                        }
                    }
                }
            }
        });

        using var client = NewClient(env);
        var request = new StringContent(json) { Headers = { ContentType = new("application/json") } };
        var response =
            await client.PutAsync($"{env.ArgoCD.Url}/api/v1/applications/{name}?validate=false", request);

        if (response.IsSuccessStatusCode) return true;
        Console.WriteLine(
            $"Failed to disable auto sync for application '{name}'. Response code: {response.StatusCode}");
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        return false;
    }

    public static async Task<bool> DisableAutoSync(StackEnvironment env, string name)
    {
        var application = await GetApplication(env, name);
        if (application is null)
        {
            Console.WriteLine($"Failed to get application '{name}'. (Unknown error)");
            return false;
        }

        var json = JsonSerializer.Serialize(new Dictionary<string, object>
        {
            {
                "metadata", new Dictionary<string, object>()
                {
                    { "name", application.Metadata.Name }
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
                            Automated = null!
                        }
                    }
                }
            }
        });

        using var client = NewClient(env);
        var request = new StringContent(json) { Headers = { ContentType = new("application/json") } };
        var response =
            await client.PutAsync($"{env.ArgoCD.Url}/api/v1/applications/{name}?validate=false", request);

        if (response.IsSuccessStatusCode) return true;
        Console.WriteLine(
            $"Failed to disable auto sync for application '{name}'. Response code: {response.StatusCode}");
        Console.WriteLine(await response.Content.ReadAsStringAsync());
        return false;
    }
}

