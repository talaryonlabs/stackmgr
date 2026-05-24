using System.Security;
using Talaryon.StackManager.Exceptions;
using Talaryon.StackManager.Models;
using Talaryon.StackManager.Serialization;
using Talaryon.StackManager.Services;

namespace Talaryon.StackManager.Builder;

public class StackBuilder(Stack stack)
{
    private KustomizeService? _kustomizeService;

    public StackBuilder WithKustomizeValidation(KustomizeService kustomizeService)
    {
        _kustomizeService = kustomizeService;
        return this;
    }

    public async Task BuildAsync()
    {
        BuildRegistryCredentials();
        BuildOutpostService();
        BuildIngressFiles();
        
        var allFiles = stack.LocalDirectory
            .GetFiles("*.yaml", SearchOption.AllDirectories)
            .Where(f => !new List<string> { Kustomization.FileName, Stack.FileName }.Contains(f.Name))
            .ToList();
        
        var baseDirectories = stack.Apps
            .Select(app => new DirectoryInfo(Path.Combine(app.LocalDirectory.FullName, ".base")))
            .Where(d => d.Exists)
            .ToList();
        
        var overrideFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        var excludedBaseFiles = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
        
        // Find all override files and mark corresponding .base files for exclusion
        foreach (var app in stack.Apps)
        {
            if (!app.LocalDirectory.Exists) continue;
            
            foreach (var overrideFile in app.LocalDirectory.GetFiles("override.*.yaml", SearchOption.TopDirectoryOnly))
            {
                // Get the base filename without the "override." prefix
                var overrideFileName = overrideFile.Name;
                if (overrideFileName.StartsWith("override.", StringComparison.OrdinalIgnoreCase))
                {
                    var baseFileName = overrideFileName["override.".Length..];
                    var relativeOverridePath = GetRelativePath(overrideFile, stack.LocalDirectory);
                    var relativeBasePath = GetRelativePath(
                        new FileInfo(Path.Combine(app.LocalDirectory.FullName, ".base", baseFileName)),
                        stack.LocalDirectory
                    );
                    
                    overrideFiles.Add(relativeOverridePath);
                    excludedBaseFiles.Add(relativeBasePath);
                }
            }
        }
        
        var kustomization = new Kustomization
        {
            Namespace = stack.Namespace,
            Images = stack.Images.Select(i => (KustomizationImage)i).ToList(),
            Resources = allFiles
                .Where(f => {
                    var relativePath = GetRelativePath(f, stack.LocalDirectory);
                    
                    // Always include override files
                    if (overrideFiles.Contains(relativePath))
                        return true;
                    
                    // Exclude .base files that have override counterparts
                    if (excludedBaseFiles.Contains(relativePath))
                        return false;
                    
                    return true;
                })
                .Select(f => GetRelativePath(f, stack.LocalDirectory))
                .ToList()
        };
            
        kustomization.Save(stack);

        // Validate the generated kustomization.yaml
        if (_kustomizeService != null)
        {
            var errors = await _kustomizeService.ValidateAsync(stack.LocalDirectory.FullName);
            if (errors.Count > 0)
            {
                throw new CliException(
                    $"Kustomization validation failed:\n" + string.Join("\n", errors.Select(e => $"  - {e}")));
            }
            
            LogMessage.AsInfo("Kustomization validation passed.");
        }
    }
    
    private void BuildRegistryCredentials()
    {
        var path = Path.Combine(stack.LocalDirectory.FullName, "registry-credentials.yaml");
        var file = new FileInfo(path);
        
        if (stack.Environment.RegistryCredentials is { Length: > 0 })
        {
            var credentials = new RegistryCredentials();
            credentials.Metadata.Annotations.Path = stack.Environment.RegistryCredentials;
            LogMessage.AsInfo($"Using registry credentials '{stack.Environment.RegistryCredentials}' for stack '{stack.Name}'.");
            File.WriteAllText(file.FullName, YamlSerializer.Serialize(credentials));
        }
        else if (file.Exists)
        {
            file.Delete();
            LogMessage.AsInfo($"Registry credentials for stack '{stack.Name}' are empty. {file.Name} removed.");
        }
    }
    
    private void BuildOutpostService()
    {
        var path = Path.Combine(stack.LocalDirectory.FullName, "svc.outpost.yaml");
        if (stack.Environment.Outpost is { Length: > 0 } && stack.Ingresses.Any(v => v.IsSecured))
        {
            var service = new Service
            {
                Metadata =
                {
                    Name = $"{stack.Name}-auth"
                },
                Spec =
                {
                    Type = "ExternalName",
                    ExternalName = stack.Environment.Outpost
                }
            };
            File.WriteAllText(path, YamlSerializer.Serialize(service));
            LogMessage.AsInfo($"Apply outpost service '{path}'.");
        }
        else if (File.Exists(path))
        {
            File.Delete(path);
            LogMessage.AsInfo($"Delete outpost service '{path}'.");
        }
    }

    private void BuildIngressFiles()
    {
        // TODO: delete ingress files that are no longer used
        /*
         *         LocalDirectory
               .GetFiles(LocalFile.Name.Replace(".yaml", "*"))
               .ToList()
               .ForEach(v =>
               {
                   v.Delete();
               });
         */
        
        if (stack.Ingresses.Count == 0) return;
        
        if (stack.Ingresses.Any(v => v.IsSecured) && stack.Environment.Outpost is not { Length: >0 })
            throw new Exception("Some ingresses are secured, but there is no environment outpost defined.");

        var folder = new DirectoryInfo(Path.Combine(stack.LocalDirectory.FullName, StackIngress.DirectoryName));
        if (!folder.Exists) folder.Create();
        
        foreach (var ingress in stack.Ingresses)
        {
            ingress.ToIngress().SaveTo(ingress.LocalFile.FullName);
            LogMessage.AsInfo($"Apply ingress file '{ingress.LocalFile.FullName}' for host '{ingress.Hostname}'.");

            var authFile = Path.ChangeExtension(ingress.LocalFile.FullName, "-auth.yaml");
            if (!ingress.IsSecured)
            {
                if(File.Exists(authFile)) File.Delete(authFile);
                continue;
            }
            
            ingress.GetAuthIngress().SaveTo(authFile);
            LogMessage.AsInfo($"Apply ingress file '{authFile}' for host '{ingress.Hostname}'.");
        }
    }

    private static string GetRelativePath(FileInfo file, DirectoryInfo root)
    {
        var fullPath = Path.GetFullPath(file.FullName);
        var rootPath = Path.GetFullPath(root.FullName + Path.DirectorySeparatorChar);
        
        if (!fullPath.StartsWith(rootPath, StringComparison.OrdinalIgnoreCase))
        {
            throw new SecurityException(
                $"File '{file.FullName}' is outside root directory '{root.FullName}'");
        }
        
        var relative = fullPath.Substring(rootPath.Length).Replace("\\", "/");
        return relative;
    }
}