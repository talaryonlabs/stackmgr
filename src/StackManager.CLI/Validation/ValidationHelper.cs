using System.Text.RegularExpressions;

namespace Talaryon.StackManager.Validation;

public static class ValidationHelper
{
    private static readonly Regex ValidStackNameRegex = new Regex(
        "^[a-z0-9][a-z0-9._-]*[a-z0-9]$", RegexOptions.Compiled);

    private static readonly Regex ValidHostnameRegex = new Regex(
        "^([a-z0-9]([a-z0-9-]*[a-z0-9])?\\.)*[a-z0-9]([a-z0-9-]*[a-z0-9])?$",
        RegexOptions.Compiled | RegexOptions.IgnoreCase);

    private static readonly Regex ValidKubernetesNameRegex = new Regex(
        "^[a-z0-9]([-a-z0-9]*[a-z0-9])?$", RegexOptions.Compiled);

    private static readonly Regex ValidSizeRegex = new Regex(
        "^[0-9]+(Gi|Mi|G|T|m)?$", RegexOptions.Compiled | RegexOptions.IgnoreCase);

    public static void ValidateStackName(string name, string paramName = "stack name")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new StackNameValidationException($"{paramName} cannot be empty.");
        if (name.Length > 63)
            throw new StackNameValidationException($"{paramName} must be <= 63 chars.");

        if (!ValidStackNameRegex.IsMatch(name))
        {
            throw new StackNameValidationException(
                $"{paramName} must start/end with alphanumeric and contain [a-z0-9._-]. Examples: example.com, test.at");
        }
    }

    public static void ValidateEnvironmentName(string name, string paramName = "environment name")
        => ValidateStackName(name, paramName);

    public static void ValidateAppName(string name, string paramName = "app name")
    {
        if (string.IsNullOrWhiteSpace(name))
            throw new AppNameValidationException($"{paramName} cannot be empty.");
        if (name.Length > 63)
            throw new AppNameValidationException($"{paramName} must be <= 63 chars.");
        
        if (!ValidKubernetesNameRegex.IsMatch(name))
        {
            throw new AppNameValidationException(
                $"{paramName} must start/end with alphanumeric and contain [a-z0-9-].");
        }
    }

    public static void ValidateHostname(string hostname, string paramName = "hostname")
    {
        if (string.IsNullOrWhiteSpace(hostname))
            throw new HostnameValidationException($"{paramName} cannot be empty.");
        if (hostname.Length > 253)
            throw new HostnameValidationException($"{paramName} must be <= 253 chars.");
        if (!ValidHostnameRegex.IsMatch(hostname))
            throw new HostnameValidationException($"{paramName} must be a valid hostname.");
        
        var labels = hostname.Split('.');
        foreach (var label in labels)
        {
            if (string.IsNullOrEmpty(label))
                throw new HostnameValidationException($"{paramName} has empty label.");
            if (label.Length > 63)
                throw new HostnameValidationException($"Label '{label}' > 63 chars.");
            if (label.StartsWith('-') || label.EndsWith('-'))
                throw new HostnameValidationException($"Label '{label}' cannot start/end with hyphen.");
        }
    }

    public static string ValidateAndNormalizeSize(string size, string paramName = "size")
    {
        if (string.IsNullOrWhiteSpace(size))
            throw new SizeValidationException($"{paramName} cannot be empty.");
        
        size = size.Trim();
        if (long.TryParse(size, out var parsedSize))
            return $"{parsedSize}Gi";
        
        return !ValidSizeRegex.IsMatch(size)
            ? throw new SizeValidationException($"{paramName} must be valid. Examples: 1Gi, 500Mi, 100.")
            : size;
    }

    public static void ValidateNamespace(string ns, string paramName = "namespace")
    {
        if (string.IsNullOrWhiteSpace(ns))
            throw new NamespaceValidationException($"{paramName} cannot be empty.");
        if (ns.Length > 63)
            throw new NamespaceValidationException($"{paramName} must be <= 63 chars.");
        if (!ValidKubernetesNameRegex.IsMatch(ns))
            throw new NamespaceValidationException($"{paramName} must be valid k8s name.");
    }

    public static void ValidatePort(int port, string paramName = "port")
    {
        if (port is < 1 or > 65535)
            throw new PortValidationException($"{paramName} must be 1-65535.");
    }

    public static int ValidatePort(string portStr, string paramName = "port")
    {
        if (!int.TryParse(portStr, out var port))
            throw new PortValidationException($"{paramName} must be a valid number.");
        
        ValidatePort(port, paramName);
        
        return port;
    }

    public static void ValidateUrl(string url, string paramName = "URL")
    {
        if (string.IsNullOrWhiteSpace(url))
            throw new UrlValidationException($"{paramName} cannot be empty.");
        if (!Uri.TryCreate(url, UriKind.Absolute, out var uriResult))
            throw new UrlValidationException($"{paramName} must be valid absolute URL.");
        if (uriResult.Scheme != Uri.UriSchemeHttp && uriResult.Scheme != Uri.UriSchemeHttps)
            throw new UrlValidationException($"{paramName} must use http:// or https://");
    }

    public static void ValidateImageName(string image, string paramName = "image")
    {
        if (string.IsNullOrWhiteSpace(image))
            throw new ImageNameValidationException($"{paramName} cannot be empty.");
        if (image.Length > 2000)
            throw new ImageNameValidationException($"{paramName} too long.");
    }
}
