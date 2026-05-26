using System.Reflection;

namespace Talaryon.StackManager;

public static class HelperMethods
{
    public static string HostToName(string host) => host.Replace(".", "-");

    public static string GenerateRandomHostname() => Guid.NewGuid().ToString("N")[..6];

    public static MethodInfo GetApiMethod<T>(IApiVersionItem apiVersionItem, string namePattern)
    {
        var apiMethods = typeof(T)
            .GetMethods(BindingFlags.Instance | BindingFlags.Static | BindingFlags.NonPublic)
            .Where(m => m.GetCustomAttribute<ApiVersionAttribute>() is not null)
            .Where(m => m.Name.StartsWith(namePattern))
            .ToList();
        
        var defaultMethod = apiMethods.FirstOrDefault(m => m.GetCustomAttribute<ApiVersionAttribute>()!.IsDefault);
        if(defaultMethod is null)
            throw new InvalidOperationException($"No default method found for {typeof(T).Name} with name pattern {namePattern}.");

        if (string.IsNullOrEmpty(apiVersionItem.Version)) return defaultMethod;
        
        var versionedMethod = apiMethods.FirstOrDefault(m => m.GetCustomAttribute<ApiVersionAttribute>()!.Version == apiVersionItem.Version);
        return versionedMethod ?? defaultMethod;
    }
}