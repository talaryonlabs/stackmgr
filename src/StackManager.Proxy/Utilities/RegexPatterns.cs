using System.Text.RegularExpressions;

namespace Talaryon.StackManager.Proxy.Utilities;

/// <summary>
/// Shared regex patterns for the application
/// </summary>
public static partial class RegexPatterns
{
    /// <summary>
    /// Kubernetes DNS-1123 label regex pattern
    /// Validates names that are alphanumeric with optional hyphens, max 63 chars
    /// Must start and end with alphanumeric character
    /// </summary>
    [GeneratedRegex(@"^[a-z0-9]([-a-z0-9]*[a-z0-9])?$")]
    private static partial Regex KubernetesNameRegex();
    
    /// <summary>
    /// Validates a Kubernetes DNS-1123 compliant name
    /// </summary>
    /// <param name="name">The name to validate</param>
    /// <returns>True if the name is valid, false otherwise</returns>
    public static bool IsValidKubernetesName(string name)
    {
        if (string.IsNullOrWhiteSpace(name) || name.Length > 63)
            return false;
            
        return KubernetesNameRegex().IsMatch(name);
    }
    
    /// <summary>
    /// Storage size quantity regex pattern
    /// Validates formats like: 10Gi, 500M, 2T, 100
    /// </summary>
    [GeneratedRegex(@"^[0-9]+(E|P|T|G|M|K|Ei|Pi|Ti|Gi|Mi|Ki)?$")]
    private static partial Regex StorageSizeRegex();
    
    /// <summary>
    /// Validates a storage size quantity
    /// </summary>
    /// <param name="size">The size string to validate</param>
    /// <returns>True if the size is valid, false otherwise</returns>
    public static bool IsValidStorageSize(string size) => !string.IsNullOrWhiteSpace(size) && StorageSizeRegex().IsMatch(size);

    /// <summary>
    /// URL regex pattern
    /// Validates absolute URLs
    /// </summary>
    [GeneratedRegex(@"^https?://[^\s/$.?#].[^\s]*$")]
    private static partial Regex UrlRegex();
    
    /// <summary>
    /// Validates a URL
    /// </summary>
    /// <param name="url">The URL to validate</param>
    /// <returns>True if the URL is valid, false otherwise</returns>
    public static bool IsValidUrl(string url) => !string.IsNullOrWhiteSpace(url) && UrlRegex().IsMatch(url);
}