using System.Diagnostics;
using Talaryon.StackManager.Models.Kubernetes;

namespace Talaryon.StackManager.Services;

public interface IKustomizeService
{
    IKustomizeServiceActions Directory(DirectoryInfo directory);
    string? GetVersion();
}

public interface IKustomizeServiceActions
{
    Task<List<string>> ValidateAsync();
}

/// <summary>
/// Service for validating kustomization.yaml files using the kustomize CLI.
/// </summary>
public class KustomizeService : IKustomizeService, IKustomizeServiceActions
{
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
    
    private DirectoryInfo? _currentDirectory = new(Environment.CurrentDirectory);
    
    IKustomizeServiceActions IKustomizeService.Directory(DirectoryInfo directory)
    {
        if (!directory.Exists) throw new DirectoryNotFoundException();
        _currentDirectory = directory;
        return this;
    }
    
    string? IKustomizeService.GetVersion()
    {
        try
        {
            var process = StartProcess("version");
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
    
    async Task<List<string>> IKustomizeServiceActions.ValidateAsync()
    {
        var errors = new List<string>();
        var path = Path.Combine(_currentDirectory!.FullName, Kustomization.FileName);
        var file = new FileInfo(path);
        
        if (!IsInstalled)
        {
            errors.Add("kustomize CLI is not installed. Please install it from https://kustomize.io/");
            return errors;
        }

        if (!file.Exists)
        {
            errors.Add($"kustomization.yaml not found at: {path}");
            return errors;
        }

        _currentDirectory.GetFiles("*.yaml", SearchOption.AllDirectories);

        var validationDirectory = _currentDirectory.GetDirectory(".validation");
        if(validationDirectory.Exists)
        {
            validationDirectory.Delete(true);
        }
        validationDirectory.Create();
        
        _currentDirectory.GetDirectories()
            .Where(d => !d.Name.Contains(".base", StringComparison.OrdinalIgnoreCase))
            .Where(d => !d.Name.Contains(".validation", StringComparison.OrdinalIgnoreCase))
            .Select(v =>
                new DirectoryInfo(Path.Combine(validationDirectory.FullName,
                    v.FullName.Replace(_currentDirectory.FullName, validationDirectory.FullName))))
            .ToList()
            .ForEach(v => Directory.CreateDirectory(v.FullName));
        
        _currentDirectory.GetFiles("*.yaml", SearchOption.AllDirectories)
            .Where(v => !v.FullName.Contains(".base", StringComparison.OrdinalIgnoreCase))
            .Where(v => !v.FullName.Contains(".validation", StringComparison.OrdinalIgnoreCase))
            .Where(v => !v.Name.Equals(Stack.FileName, StringComparison.OrdinalIgnoreCase))
            .ToList()
            .ForEach(v =>
            {
                File.Copy(v.FullName, Path.Combine(validationDirectory.FullName, v.FullName.Replace(_currentDirectory.FullName, validationDirectory.FullName)), true);
            });

        // Validate using kustomize build (dry-run mode)
        // The build command will fail if the kustomization is invalid
        var buildErrors = await RunKustomizeBuildAsync();
        errors.AddRange(buildErrors);

        // If build succeeded, also run cfg validation
        if (errors.Count == 0)
        {
            var cfgErrors = await RunKustomizeCfgAsync();
            errors.AddRange(cfgErrors);
        }
        
        validationDirectory.Delete(true);

        return errors;
    }

    private async Task<List<string>> RunKustomizeBuildAsync()
    {
        var errors = new List<string>();

        try
        {
            var process = StartProcess("build .validation");
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

    private async Task<List<string>> RunKustomizeCfgAsync()
    {
        var errors = new List<string>();

        try
        {
            var process = StartProcess("cfg tree .validation");
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

    private Process? StartProcess(string command)
    {
        var process = Process.Start(new ProcessStartInfo
        {
            FileName = "kustomize",
            Arguments = command,
            RedirectStandardOutput = true,
            UseShellExecute = false,
            RedirectStandardError = true,
            CreateNoWindow = true,
            WorkingDirectory = _currentDirectory!.FullName
        });

        return process;
    }
}
