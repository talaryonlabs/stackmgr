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

    public static async Task<bool> CreateApplication(StackEnvironment env, string name)
    {
        if (!CheckRequirements(env)) return false;


        using var client = NewClient(env);
        var response =
            await client.GetAsync(
                $"{env.ArgoCD.Url}/api/v1/applications");

        var list = await response.Content.ReadFromJsonAsync<V1alpha1ApplicationList>();

        list.Items.ForEach(x => Console.WriteLine($"{x.Metadata.Name}"));


        var application = new ArgoCDApplication
        {
            Metadata = new ArgoCDApplicationMetadata
            {
                Name = name
            },
            Spec = new ArgoCDApplicationSpec
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

        return await response.Content.ReadFromJsonAsync<V1alpha1Application>();;
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
        
        application.Spec = new V1alpha1ApplicationSpec
        {
            SyncPolicy = new V1alpha1SyncPolicy
            {
                Automated = new V1alpha1SyncPolicyAutomated
                {
                    Prune = true,
                    SelfHeal = true,
                    AllowEmpty = false
                }
            }
        };
        
        using var client = NewClient(env);
        var response =
            await client.PutAsJsonAsync(
                $"{env.ArgoCD.Url}/api/v1/applications/{name}", application);

        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to enable auto sync for application '{name}'. Response code: {response.StatusCode}");
            return false;
        }

        return true;
    }
    
    public static async Task<bool> DisableAutoSync(StackEnvironment env, string name)
    {
        var application = await GetApplication(env, name);
        if (application is null)
        {
            Console.WriteLine($"Failed to get application '{name}'. (Unknown error)");
            return false;
        }

        var app = (ArgoCDApplication)application;
        
        using var client = NewClient(env);

        var json = JsonSerializer.Serialize(app);
        
        var request = new HttpRequestMessage(HttpMethod.Put, $"{env.ArgoCD.Url}/api/v1/applications/{name}");
        request.Content = new StringContent(json);
        request.Content.Headers.ContentType = new("application/json");
        
        var response =
            await client.SendAsync(request);

        var content = await response.Content.ReadAsStringAsync();
        
        if (!response.IsSuccessStatusCode)
        {
            Console.WriteLine($"Failed to disable auto sync for application '{name}'. Response code: {response.StatusCode}");
            return false;
        }

        return true;
    }
}

