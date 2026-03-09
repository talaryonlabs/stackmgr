using System.Security.Claims;
using Microsoft.AspNetCore.Authentication.BearerToken;
using Microsoft.AspNetCore.ResponseCompression;
using Microsoft.AspNetCore.RateLimiting;
using Talaryon.StackManager.Proxy.Services;
using Talaryon.Toolbox.Extensions;
using Talaryon.Toolbox.Hosting.Api;
using Talaryon.Toolbox.Hosting.Api.Filters;

var builder = WebApplication.CreateBuilder(args);
var mediaType = new ApiMediaType();

var requiredConfig = new[]
{
    "STACKMGR_ACCESS_TOKEN",
    "STACKMGR_RKE2_URL", "STACKMGR_RKE2_ACCESS_TOKEN", "STACKMGR_RKE2_PROJECT",
    "STACKMGR_ARGOCD_URL", "STACKMGR_ARGOCD_ACCESS_TOKEN", "STACKMGR_ARGOCD_PROJECT", 
    "STACKMGR_LONGHORN_URL", "STACKMGR_LONGHORN_ACCESS_TOKEN"
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

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("fixed", opt =>
    {
        opt.Window = TimeSpan.FromMinutes(1);
        opt.PermitLimit = 100; // 100 requests per minute per endpoint
        opt.QueueProcessingOrder = System.Threading.RateLimiting.QueueProcessingOrder.OldestFirst;
        opt.QueueLimit = 2; // Small queue for burst handling
    });
});


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
        options.Url = builder.Configuration["STACKMGR_RKE2_URL"];
        options.AccessToken = builder.Configuration["STACKMGR_RKE2_ACCESS_TOKEN"]!.FromBase64String();
        options.Project = builder.Configuration["STACKMGR_RKE2_PROJECT"];
    })
    .AddSingleton<IArgoService, ArgoService, ArgoOptions>(options =>
    {
        options.Url = builder.Configuration["STACKMGR_ARGOCD_URL"];
        options.AccessToken = builder.Configuration["STACKMGR_ARGOCD_ACCESS_TOKEN"]!.FromBase64String();
        options.Project = builder.Configuration["STACKMGR_ARGOCD_PROJECT"];
    })
    .AddSingleton<ILonghornService, LonghornService, LonghornOptions>(options =>
    {
        options.Url = builder.Configuration["STACKMGR_LONGHORN_URL"];
        options.AccessToken = builder.Configuration["STACKMGR_LONGHORN_ACCESS_TOKEN"]!.FromBase64String();
    })
    .AddHttpClient();

var app = builder.Build();

app.MapControllers();

app
    .UseAuthentication()
    .UseAuthorization()
    .UseRateLimiter()
    .UseResponseCompression()
    .Use((context, next) =>
    {
        context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
        context.Response.Headers.Append("X-Frame-Options", "DENY");
        context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
        context.Response.Headers.Append("Referrer-Policy", "no-referrer");
        context.Response.Headers.Append("Content-Security-Policy", "default-src 'self'");
        return next();
    })
    .Use((context, next) => context.Request.ContentLength > 1024 * 1024
        ? Task.FromResult<object>(context.Response.StatusCode = 413)
        : next());

app.Run("http://+:5380");