using System;

namespace Talaryon.StackManager;

public class ApiMethods
{
    [ApiVersion("stack.talaryon.io/v2beta")]
    private static string GetVolumeName(Stack stack, string volume)
    {
        var envName = stack.Environment.Name;
        var stackName = stack.Name;
        
        // Kubernetes DNS-1123: max 63 chars total
        // Calculate max allowed volume length: 63 - (env + stack + 2 hyphens)
        var prefixLength = envName.Length + stackName.Length + 2;
        var maxVolumeLength = 63 - prefixLength;
        
        if (maxVolumeLength < 1)
            throw new ArgumentException("Environment and stack names are too long to fit a volume name.");
        
        // Truncate volume to fit within the 63-char limit
        var truncatedVolume = volume.Length <= maxVolumeLength
            ? volume
            : volume[..maxVolumeLength];
        
        var fullName = $"{envName}-{stackName}-{truncatedVolume}";
        
        // Ensure the name is valid Kubernetes DNS-1123
        if (!IsValidKubernetesName(fullName))
        {
            // If truncation broke the pattern, adjust
            if (fullName.Length > 0 && !char.IsLetterOrDigit(fullName[^1]))
                fullName = fullName[..^1];
            if (fullName.Length > 0 && !char.IsLetterOrDigit(fullName[0]))
                fullName = fullName[1..];
            
            // Final check
            if (!IsValidKubernetesName(fullName))
                throw new ArgumentException($"Generated volume name '{fullName}' is not a valid Kubernetes DNS name.");
        }
        
        return fullName;
    }
    
    private static bool IsValidKubernetesName(string name)
    {
        if (string.IsNullOrEmpty(name) || name.Length > 63)
            return false;
        if (!char.IsLetterOrDigit(name[0]) || !char.IsLetterOrDigit(name[^1]))
            return false;
        foreach (var c in name)
        {
            if (!char.IsLetterOrDigit(c) && c != '-')
                return false;
        }
        return true;
    }

    [ApiVersion]
    private static string GetVolumeNameLegacy(Stack stack, string volume)
    {
        // For legacy API: only check and truncate volume length itself
        if (volume.Length > 63)
        {
            // Truncate to 63 characters
            var truncated = volume[..63];
            
            // Ensure it's valid Kubernetes DNS-1123
            if (!IsValidKubernetesName(truncated))
            {
                // Adjust if needed
                if (truncated.Length > 0 && !char.IsLetterOrDigit(truncated[^1]))
                    truncated = truncated[..^1];
                if (truncated.Length > 0 && !char.IsLetterOrDigit(truncated[0]))
                    truncated = truncated[1..];
                
                if (!IsValidKubernetesName(truncated))
                    throw new ArgumentException($"Volume name '{volume}' cannot be made valid for Kubernetes.");
            }
            return truncated;
        }
        
        // Validate volume name is a valid Kubernetes DNS-1123 name
        if (!IsValidKubernetesName(volume))
            throw new ArgumentException($"Volume name '{volume}' is not a valid Kubernetes DNS name.");
        
        return volume;
    }
}