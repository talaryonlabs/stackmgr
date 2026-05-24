using System.Diagnostics;
using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Services;

/// <summary>
/// Service for validating kustomization.yaml files using the kustomize CLI.
/// </summary>
public class KustomizeService
{
    private readonly LocalConfig _config;

    /// <summary>
    /// Creates a new KustomizeService with default configuration.
    /// </summary>
    public KustomizeService() : this(LocalConfig.Get())
    {
    }

    /// <summary>
    /// Creates a new KustomizeService with the specified configuration.
    /// </summary>
    /// <param name="config">The local configuration</param>
    public KustomizeService(LocalConfig config)
    {
        _config = config;
    }

    /// <summary>
    /// Checks if kustomize is installed on the system.
    /// </summary>
    public static bool IsInstalled
    {
        get
        {
            try
            {
                var process = Process.Start(new ProcessStartInfo("kustomize", "version")
                {
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                });

                if (process is null) return false;
                
                process.WaitForExit(5000);
                return process.ExitCode == 0;
            }
            catch
            {
                return false;
            }
        }
    }

    /// <summary>
    /// Validates a kustomization.yaml file at the specified path using kustomize CLI.
    /// </summary>
    /// <param name="kustomizationPath">Path to the kustomization.yaml file or directory containing it</param>
    /// <returns>List of validation errors (empty if valid)</returns>
    public async Task<List<string>> ValidateAsync(string kustomizationPath)
    {
        var errors = new List<string>();

        if (!IsInstalled)
        {
            errors.Add("kustomize CLI is not installed. Please install it from https://kustomize.io/");
            return errors;
        }

        var fileInfo = GetKustomizationFile(kustomizationPath);

        if (fileInfo is null || !fileInfo.Exists)
        {
            errors.Add($"kustomization.yaml not found at: {kustomizationPath}");
            return errors;
        }

        var directory = fileInfo.Directory!.FullName;

        // Validate using kustomize build (dry-run mode)
        // The build command will fail if the kustomization is invalid
        var buildErrors = await RunKustomizeBuildAsync(directory);
        errors.AddRange(buildErrors);

        // If build succeeded, also run cfg validation
        if (errors.Count == 0)
        {
            var cfgErrors = await RunKustomizeCfgAsync(directory);
            errors.AddRange(cfgErrors);
        }

        return errors;
    }

    /// <summary>
    /// Runs kustomize build to validate the kustomization.
    /// </summary>
    /// <param name="directory">Directory containing kustomization.yaml</param>
    /// <returns>List of errors from the build process</returns>
    private async Task<List<string>> RunKustomizeBuildAsync(string directory)
    {
        var errors = new List<string>();

        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "kustomize",
                Arguments = "build --load-restrictor LoadRestrictionsNone",
                WorkingDirectory = directory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            if (process is null)
            {
                errors.Add("Failed to start kustomize build command");
                return errors;
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var errorOutput = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                if (!string.IsNullOrEmpty(errorOutput))
                {
                    errors.AddRange(errorOutput.Split('\n')
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .Select(line => line.Trim()));
                }
                else if (!string.IsNullOrEmpty(output))
                {
                    errors.AddRange(output.Split('\n')
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .Select(line => line.Trim()));
                }
                else
                {
                    errors.Add("kustomize build failed with no output");
                }
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Error running kustomize build: {ex.Message}");
        }

        return errors;
    }

    /// <summary>
    /// Runs kustomize cfg tree to validate the resource structure.
    /// </summary>
    /// <param name="directory">Directory containing kustomization.yaml</param>
    /// <returns>List of errors from cfg validation</returns>
    private async Task<List<string>> RunKustomizeCfgAsync(string directory)
    {
        var errors = new List<string>();

        try
        {
            var process = Process.Start(new ProcessStartInfo
            {
                FileName = "kustomize",
                Arguments = "cfg tree",
                WorkingDirectory = directory,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            if (process is null)
            {
                return errors;
            }

            var output = await process.StandardOutput.ReadToEndAsync();
            var errorOutput = await process.StandardError.ReadToEndAsync();
            await process.WaitForExitAsync();

            if (process.ExitCode != 0)
            {
                if (!string.IsNullOrEmpty(errorOutput))
                {
                    errors.AddRange(errorOutput.Split('\n')
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .Select(line => line.Trim()));
                }
                else if (!string.IsNullOrEmpty(output))
                {
                    errors.AddRange(output.Split('\n')
                        .Where(line => !string.IsNullOrWhiteSpace(line))
                        .Select(line => line.Trim()));
                }
            }
        }
        catch (Exception ex)
        {
            errors.Add($"Error running kustomize cfg: {ex.Message}");
        }

        return errors;
    }

    /// <summary>
    /// Gets the kustomization.yaml file from a path (file or directory).
    /// </summary>
    /// <param name="path">Path to kustomization.yaml or directory containing it</param>
    /// <returns>FileInfo for kustomization.yaml, or null if not found</returns>
    public FileInfo? GetKustomizationFile(string path)
    {
        var fileInfo = new FileInfo(path);

        if (fileInfo.Exists && fileInfo.Name.Equals("kustomization.yaml", StringComparison.OrdinalIgnoreCase))
        {
            return fileInfo;
        }

        var directoryInfo = fileInfo.Directory ?? new DirectoryInfo(path);
        if (!directoryInfo.Exists)
        {
            return null;
        }

        var kustomizationFile = new FileInfo(Path.Combine(directoryInfo.FullName, "kustomization.yaml"));
        if (kustomizationFile.Exists)
        {
            return kustomizationFile;
        }

        kustomizationFile = new FileInfo(Path.Combine(directoryInfo.FullName, "kustomization.yml"));
        if (kustomizationFile.Exists)
        {
            return kustomizationFile;
        }

        return null;
    }

    /// <summary>
    /// Checks if a kustomization.yaml file exists in the specified directory.
    /// </summary>
    /// <param name="directory">Directory to check</param>
    /// <returns>True if kustomization.yaml exists</returns>
    public bool HasKustomization(DirectoryInfo directory)
    {
        return GetKustomizationFile(directory.FullName) != null;
    }

    /// <summary>
    /// Validates a directory contains a valid kustomization.yaml.
    /// </summary>
    /// <param name="directoryPath">Path to directory to validate</param>
    /// <returns>True if directory contains valid kustomization.yaml</returns>
    public async Task<bool> IsValidKustomizationDirectoryAsync(string directoryPath)
    {
        var errors = await ValidateAsync(directoryPath);
        return errors.Count == 0;
    }

    /// <summary>
    /// Gets the version of the installed kustomize CLI.
    /// </summary>
    /// <returns>Version string, or null if not installed</returns>
    public static string? GetVersion()
    {
        try
        {
            var process = Process.Start(new ProcessStartInfo("kustomize", "version")
            {
                RedirectStandardOutput = true,
                UseShellExecute = false,
                CreateNoWindow = true
            });

            if (process is null) return null;
            
            var output = process.StandardOutput.ReadToEnd();
            process.WaitForExit(5000);
            
            if (process.ExitCode == 0)
            {
                return output.Trim();
            }
        }
        catch
        {
            // Ignore
        }
        
        return null;
    }
}
