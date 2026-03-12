using Talaryon.StackManager.Proxy.Services;
using Talaryon.Toolbox.Extensions;
using Talaryon.Toolbox.Hosting;
using Talaryon.Toolbox.Hosting.Api;

var builder = WebApplication.CreateBuilder(args);
var requiredConfig = new[]
{
    "STACKMGR_ACCESS_TOKEN",
    "STACKMGR_RKE2_URL", "STACKMGR_RKE2_ACCESS_TOKEN", "STACKMGR_RKE2_PROJECT",
    "STACKMGR_ARGOCD_URL", "STACKMGR_ARGOCD_ACCESS_TOKEN", "STACKMGR_ARGOCD_PROJECT", 
    "STACKMGR_LONGHORN_URL" // "STACKMGR_LONGHORN_ACCESS_TOKEN"
};
var missingConfig = requiredConfig
    .Where(x => builder.Configuration.GetValue<string>(x) == null)
    .ToList();
if(missingConfig.Count != 0)
{
    foreach (var item in missingConfig)
    {
        Console.WriteLine($"Missing required config: {item}");
    }
    throw new Exception("Missing required configuration.");
}

var tokens = builder.Configuration
    .GetChildren()
    .Where(x => x.Key.StartsWith("STACKMGR_ACCESS_TOKEN"))
    .Select(x => x.Value)
    .Where(x => x != null)
    .Select(x => x!)
    .ToList();

var options = new ApiHostingOptions();
options.AccessTokens
    .AddRange(tokens);

builder.Services
    .AddApiComponents(options);

builder.Services
    .AddSingleton<IRancherService, RancherService, RancherOptions>(opt =>
    {
        opt.Url = builder.Configuration["STACKMGR_RKE2_URL"];
        opt.AccessToken = builder.Configuration["STACKMGR_RKE2_ACCESS_TOKEN"]!.FromBase64String();
        opt.Project = builder.Configuration["STACKMGR_RKE2_PROJECT"];
    })
    .AddSingleton<IArgoService, ArgoService, ArgoOptions>(opt =>
    {
        opt.Url = builder.Configuration["STACKMGR_ARGOCD_URL"];
        opt.AccessToken = builder.Configuration["STACKMGR_ARGOCD_ACCESS_TOKEN"]!.FromBase64String();
        opt.Project = builder.Configuration["STACKMGR_ARGOCD_PROJECT"];
    })
    .AddSingleton<ILonghornService, LonghornService, LonghornOptions>(opt =>
    {
        opt.Url = builder.Configuration["STACKMGR_LONGHORN_URL"];
        // opt.AccessToken = builder.Configuration["STACKMGR_LONGHORN_ACCESS_TOKEN"]!.FromBase64String();
    })
    .AddHttpClient();

var app = builder.BuildAsApi(options);

app.Run("http://+:5380");