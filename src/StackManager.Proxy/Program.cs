// See https://aka.ms/new-console-template for more information

using System.Net.Http.Headers;
using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.ResponseCompression;
using StackManager.Shared;
using Talaryon.StackManager.Proxy;
using Talaryon.StackManager.Proxy.Services;
using Talaryon.Toolbox.Extensions;
using Talaryon.Toolbox.Hosting.Api;
using Talaryon.Toolbox.Hosting.Api.Filters;

var builder = WebApplication.CreateBuilder(args);
var mediaType = new ApiMediaType();


var tokens = builder.Configuration
    .GetChildren()
    .Where(x => x.Key.StartsWith("STACKMGR_ACCESS_TOKEN"))
    .Select(x => x.Value)
    .ToList();

builder.Services
    .AddAuthentication(BearerTokenDefaults.AuthenticationScheme)
    .AddBearerToken(options =>
    {
        options.Events = new BearerTokenEvents
        {
            OnMessageReceived = context =>
            {
                if (!context.Request.Headers.TryGetValue("Authorization", out var authHeader))
                    return Task.CompletedTask;

                var token = authHeader
                    .ToString()
                    .Replace("Bearer", "")
                    .Trim();
                
                if (!tokens.Contains(token)) return Task.CompletedTask;
                
                var claims = new List<Claim>
                {
                    new(ClaimTypes.Name, "ApiToken")
                };
                var identity = new ClaimsIdentity(claims, BearerTokenDefaults.AuthenticationScheme);
                var principal = new ClaimsPrincipal(identity);

                context.Principal = principal;
                context.Success();

                return Task.CompletedTask;
            }
        };
        options.Validate();
    });
builder.Services
    .AddAuthorization();


builder.Services
    .AddMvcCore()
    .AddMvcOptions(options =>
    {
        options.Filters.Add(new ApiExceptionFilter(mediaType));
    });

builder.Services
    .AddControllers();

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
    options.Providers.Add<BrotliCompressionProvider>();
    options.Providers.Add<GzipCompressionProvider>();
    options.MimeTypes = ResponseCompressionDefaults.MimeTypes.Concat([
        mediaType.MediaType.ToString()
    ]);
});

builder.Services
    .AddSingleton<IRancherService, RancherService, RancherOptions>(options =>
    {
        options.Url = builder.Configuration["STACKMGR_RKE2_URL"] ?? throw new Exception("STACKMGR_RKE2_URL not set.");
        options.AccessToken = (builder.Configuration["STACKMGR_RKE2_ACCESS_TOKEN"] ?? throw new Exception("STACKMGR_RKE2_ACCESS_TOKEN not set.")).FromBase64String();
        options.Project = builder.Configuration["STACKMGR_RKE2_PROJECT"] ?? throw new Exception("STACKMGR_RKE2_PROJECT not set.");
    })
    .AddSingleton<IArgoService, ArgoService, ArgoOptions>(options =>
    {
        options.Url = builder.Configuration["STACKMGR_ARGOCD_URL"] ?? throw new Exception("STACKMGR_ARGOCD_URL not set.");
        options.AccessToken = (builder.Configuration["STACKMGR_ARGOCD_ACCESS_TOKEN"] ?? throw new Exception("STACKMGR_ARGOCD_ACCESS_TOKEN not set.")).FromBase64String();
        options.Project = builder.Configuration["STACKMGR_ARGOCD_PROJECT"] ?? throw new Exception("STACKMGR_ARGOCD_PROJECT not set.");
    })
    .AddSingleton<ILonghornService, LonghornService, LonghornOptions>(options =>
    {
        options.Url = builder.Configuration["STACKMGR_LONGHORN_URL"] ?? throw new Exception("STACKMGR_LONGHORN_URL not set.");
        
        // var token = File.ReadAllText("/var/run/secrets/kubernetes.io/serviceaccount/token");
        // _client.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");
        
        options.AccessToken = (builder.Configuration["STACKMGR_LONGHORN_ACCESS_TOKEN"] ?? throw new Exception("STACKMGR_LONGHORN_ACCESS_TOKEN not set.")).FromBase64String();
    })
    .AddHttpClient();

var app = builder.Build();

app
    .UseAuthentication()
    .UseAuthorization();
    
app.MapControllers();

app.UseResponseCompression();

app.Run("http://+:5380");