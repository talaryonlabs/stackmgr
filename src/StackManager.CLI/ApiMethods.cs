using System;
using System.Security.Cryptography;
using System.Text;

namespace Talaryon.StackManager;

public class ApiMethods
{
    [ApiVersion("stack.talaryon.io/v2beta")]
    private static string GetVolumeName(Stack stack, string volume)
    {
        var envName = stack.Environment.Name;
        var stackName = stack.Name;
        
        // Longhorn max name length: 40 chars total
        // Calculate available space: 40 - (env + stack + 2 hyphens)
        var prefix = $"{envName}-{stackName}-";
        var availableForVolume = 40 - prefix.Length;
        
        if (availableForVolume < 8) // Need at least 8 chars for hash
            throw new ArgumentException("Environment and stack names are too long to fit a volume name (Longhorn limit: 40 chars).");
        
        // Truncate volume and append hash if needed
        string volumePart;
        if (volume.Length <= availableForVolume)
        {
            volumePart = volume;
        }
        else
        {
            // Reserve 9 chars for hyphen + 8-char hash
            var maxVolLength = availableForVolume - 9;
            if (maxVolLength < 1)
                throw new ArgumentException("Not enough space for volume name and hash.");
            
            var truncatedVol = volume[..maxVolLength];
            var hash = ComputeHash(volume, 8);
            volumePart = $"{truncatedVol}-{hash}";
        }
        
        var fullName = $"{prefix}{volumePart}";
        
        // Ensure valid Kubernetes DNS-1123
        if (!IsValidKubernetesName(fullName))
        {
            if (fullName.Length > 0 && !char.IsLetterOrDigit(fullName[^1]))
                fullName = fullName[..^1];
            if (fullName.Length > 0 && !char.IsLetterOrDigit(fullName[0]))
                fullName = fullName[1..];
            
            if (!IsValidKubernetesName(fullName))
                throw new ArgumentException($"Generated volume name '{fullName}' is not a valid Kubernetes DNS name.");
        }
        
        return fullName;
    }
    
    private static string ComputeHash(string input, int length)
    {
        using var sha256 = SHA256.Create();
        var hashBytes = sha256.ComputeHash(Encoding.UTF8.GetBytes(input));
        var hex = BitConverter.ToString(hashBytes).Replace("-", "").ToLower();
        return hex[..length];
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
        // For legacy API: only check volume length itself with hash truncation
        // Longhorn limit: 40 chars
        if (volume.Length > 40)
        {
            // Reserve 9 chars for hyphen + 8-char hash
            var maxVolLength = 40 - 9; // 31 chars for volume name
            if (maxVolLength < 1)
                throw new ArgumentException("Volume name too long even with hash truncation.");
            
            var truncatedVol = volume[..maxVolLength];
            var hash = ComputeHash(volume, 8);
            var fullName = $"{truncatedVol}-{hash}";
            
            // Ensure it's valid Kubernetes DNS-1123
            if (!IsValidKubernetesName(fullName))
            {
                if (fullName.Length > 0 && !char.IsLetterOrDigit(fullName[^1]))
                    fullName = fullName[..^1];
                if (fullName.Length > 0 && !char.IsLetterOrDigit(fullName[0]))
                    fullName = fullName[1..];
                
                if (!IsValidKubernetesName(fullName))
                    throw new ArgumentException($"Volume name '{volume}' cannot be made valid for Longhorn (40 char limit).");
            }
            return fullName;
        }
        
        // Validate volume name is a valid Kubernetes DNS-1123 name
        if (!IsValidKubernetesName(volume))
            throw new ArgumentException($"Volume name '{volume}' is not a valid Kubernetes DNS name.");
        
        return volume;
    }
}