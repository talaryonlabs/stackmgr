using System.Security;

namespace Talaryon.StackManager.Services;

public interface IAppService
{
    Task InitializeFromTemplateAsync(StackTemplate template);
    Task MigrateAsync(StackTemplate template);
    Task BuildAsync(DirectoryInfo outputDirectory);
    bool HasImplementation(string relativePath);
    FileInfo? GetEffectiveFile(string relativePath);
}

/// <summary>
/// Service for managing app templates, migration, and file structure.
/// Template files are stored in the '.base' subfolder.
/// User-created files are stored directly in the app root folder.
/// When migrating, template files are copied to '.base' (replacing existing contents).
/// Files matching 'init.*.yaml' in templates are copied to app root (without prefix) instead of .base,
/// and only if they don't already exist.
/// </summary>
public class AppService : IAppService
{
    private readonly StackApp _app;
    
    public DirectoryInfo BaseDirectory { get; }
    
    public AppService(StackApp app)
    {
        _app = app;
        BaseDirectory = new DirectoryInfo(Path.Combine(app.LocalDirectory.FullName, ".base"));
    }
    
    /// <summary>
    /// Initializes the app directory structure with template contents in .base folder.
    /// Files matching 'init.*.yaml' are copied to app root without the 'init.' prefix.
    /// </summary>
    public async Task InitializeFromTemplateAsync(StackTemplate template)
    {
        if (!template.LocalDirectory.Exists)
        {
            throw new DirectoryNotFoundException(
                $"Template directory '{template.LocalDirectory.FullName}' not found.");
        }
        
        // Ensure app directory exists
        if (!_app.LocalDirectory.Exists)
        {
            _app.LocalDirectory.Create();
        }
        
        // Delete existing .base directory if it exists
        if (BaseDirectory.Exists)
        {
            BaseDirectory.Delete(true);
        }
        
        // Create .base directory
        BaseDirectory.Create();
        
        // First, handle init files - copy to app root without prefix
        foreach (var file in template.LocalDirectory.GetFiles("init.*.yaml", SearchOption.TopDirectoryOnly))
        {
            var targetFileName = file.Name["init.".Length..];
            var destPath = Path.Combine(_app.LocalDirectory.FullName, targetFileName);
            var destFile = new FileInfo(destPath);
            
            // Only copy if the file doesn't already exist
            if (!destFile.Exists)
            {
                var content = await File.ReadAllTextAsync(file.FullName);
                await File.WriteAllTextAsync(destFile.FullName, content);
                LogMessage.AsInfo($"Applied init file '{targetFileName}' to app root.");
            }
        }
        
        // Copy template contents to .base folder (excluding init files)
        foreach (var file in template.LocalDirectory.GetFiles("*", SearchOption.AllDirectories))
        {
            if (file.Name.Equals(StackTemplate.FileName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            
            // Skip init files as they were already handled
            if (file.Name.StartsWith("init.", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            
            var relativePath = GetRelativePath(file, template.LocalDirectory);
            var destPath = Path.Combine(BaseDirectory.FullName, relativePath);
            var destFile = new FileInfo(destPath);
            
            if (!destFile.Directory!.Exists)
            {
                destFile.Directory.Create();
            }
            
            var content = await File.ReadAllTextAsync(file.FullName);
            await File.WriteAllTextAsync(destFile.FullName, content);
        }
    }
    
    /// <summary>
    /// Migrates the app by copying template contents to .base folder (deleting existing .base first).
    /// Files matching 'init.*.yaml' are copied to app root without the 'init.' prefix and with variable substitution.
    /// </summary>
    public async Task MigrateAsync(StackTemplate template)
    {
        if (!template.LocalDirectory.Exists)
        {
            throw new DirectoryNotFoundException(
                $"Template directory '{template.LocalDirectory.FullName}' not found.");
        }
        
        // Ensure app directory exists
        if (!_app.LocalDirectory.Exists)
        {
            _app.LocalDirectory.Create();
        }
        
        // Delete existing .base directory
        if (BaseDirectory.Exists)
        {
            BaseDirectory.Delete(true);
        }
        
        // Create .base directory
        BaseDirectory.Create();
        
        // Copy template contents to .base folder with variable substitution
        var vault = _app.Stack.Environment.Vault.EndsWith("/")
            ? _app.Stack.Environment.Vault[..^1]
            : _app.Stack.Environment.Vault;
        
        // First, handle init files - copy to app root without prefix and with variable substitution
        foreach (var file in template.LocalDirectory.GetFiles("init.*.yaml", SearchOption.TopDirectoryOnly))
        {
            var targetFileName = file.Name["init.".Length..];
            var destPath = Path.Combine(_app.LocalDirectory.FullName, targetFileName);
            var destFile = new FileInfo(destPath);
            
            // Only copy if the file doesn't already exist
            if (!destFile.Exists)
            {
                var content = await File.ReadAllTextAsync(file.FullName);
                
                // Apply variable substitution for init files too
                content = content
                    .Replace("{{app-name}}", _app.Name)
                    .Replace("{{stack-name}}", _app.Stack.Name)
                    .Replace("{{env-name}}", _app.Stack.Environment.Name)
                    .Replace("{{vault-path}}", $"{vault}/{_app.Stack.Name}/{_app.Name}");
                
                content = _app.Volumes.Aggregate(content,
                    (current, volume) => current.Replace("{{app-volume." + volume.Key + "}}", volume.Value));
                content = _app.Params.Aggregate(content,
                    (current, param) => current.Replace("{{app-param." + param.Key + "}}", param.Value));
                content = _app.Requirements.Aggregate(content,
                    (current, requirement) =>
                        current.Replace("{{app-requirement." + requirement.Key + "}}", requirement.Value));
                
                await File.WriteAllTextAsync(destFile.FullName, content);
                LogMessage.AsInfo($"Applied init file '{targetFileName}' to app root.");
            }
        }
        
        // Copy template contents to .base folder with variable substitution (excluding init files)
        foreach (var file in template.LocalDirectory.GetFiles("*", SearchOption.AllDirectories))
        {
            if (file.Name.Equals(StackTemplate.FileName, StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            
            // Skip init files as they were already handled
            if (file.Name.StartsWith("init.", StringComparison.OrdinalIgnoreCase))
            {
                continue;
            }
            
            var relativePath = GetRelativePath(file, template.LocalDirectory);
            var destPath = Path.Combine(BaseDirectory.FullName, relativePath);
            var destFile = new FileInfo(destPath);
            
            if (!destFile.Directory!.Exists)
            {
                destFile.Directory.Create();
            }
            
            var content = await File.ReadAllTextAsync(file.FullName);
            
            // Apply variable substitution
            content = content
                .Replace("{{app-name}}", _app.Name)
                .Replace("{{stack-name}}", _app.Stack.Name)
                .Replace("{{env-name}}", _app.Stack.Environment.Name)
                .Replace("{{vault-path}}", $"{vault}/{_app.Stack.Name}/{_app.Name}");
            
            content = _app.Volumes.Aggregate(content,
                (current, volume) => current.Replace("{{app-volume." + volume.Key + "}}", volume.Value));
            content = _app.Params.Aggregate(content,
                (current, param) => current.Replace("{{app-param." + param.Key + "}}", param.Value));
            content = _app.Requirements.Aggregate(content,
                (current, requirement) =>
                    current.Replace("{{app-requirement." + requirement.Key + "}}", requirement.Value));
            
            await File.WriteAllTextAsync(destFile.FullName, content);
            LogMessage.AsInfo($"Applied '{relativePath}' to .base folder.");
        }
    }
    
    /// <summary>
    /// Gets all effective files for building (user files take precedence over .base files).
    /// </summary>
    public Dictionary<string, FileInfo> GetBuildFiles()
    {
        var files = new Dictionary<string, FileInfo>(StringComparer.OrdinalIgnoreCase);
        
        // First, add .base files
        if (BaseDirectory.Exists)
        {
            foreach (var file in BaseDirectory.GetFiles("*", SearchOption.AllDirectories))
            {
                var relativePath = GetRelativePath(file, BaseDirectory);
                files[relativePath] = file;
            }
        }
        
        // Then, add user files from app root (overwriting .base files with same name)
        if (_app.LocalDirectory.Exists)
        {
            foreach (var file in _app.LocalDirectory.GetFiles("*", SearchOption.AllDirectories))
            {
                // Skip .base directory files
                if (file.FullName.StartsWith(BaseDirectory.FullName, StringComparison.OrdinalIgnoreCase))
                {
                    continue;
                }
                
                var relativePath = GetRelativePath(file, _app.LocalDirectory);
                files[relativePath] = file;
            }
        }
        
        return files;
    }
    
    /// <summary>
    /// Builds the app by copying all effective files to the output directory.
    /// </summary>
    public async Task BuildAsync(DirectoryInfo outputDirectory)
    {
        if (!outputDirectory.Exists)
        {
            outputDirectory.Create();
        }
        
        var files = GetBuildFiles();
        
        foreach (var (relativePath, file) in files)
        {
            var destPath = Path.Combine(outputDirectory.FullName, relativePath);
            var destFile = new FileInfo(destPath);
            
            if (!destFile.Directory!.Exists)
            {
                destFile.Directory.Create();
            }
            
            var content = await File.ReadAllTextAsync(file.FullName);
            await File.WriteAllTextAsync(destFile.FullName, content);
        }
    }
    
    /// <summary>
    /// Checks if a user implementation exists for a given relative path.
    /// </summary>
    public bool HasImplementation(string relativePath)
    {
        var filePath = Path.Combine(_app.LocalDirectory.FullName, relativePath);
        
        // Check if file exists outside .base directory
        if (filePath.StartsWith(BaseDirectory.FullName, StringComparison.OrdinalIgnoreCase))
        {
            return false;
        }
        
        return File.Exists(filePath);
    }
    
    /// <summary>
    /// Gets the effective file for a given relative path (user file takes precedence over .base).
    /// </summary>
    public FileInfo? GetEffectiveFile(string relativePath)
    {
        var userPath = Path.Combine(_app.LocalDirectory.FullName, relativePath);
        
        // Check user file first (outside .base)
        if (File.Exists(userPath) && !userPath.StartsWith(BaseDirectory.FullName, StringComparison.OrdinalIgnoreCase))
        {
            return new FileInfo(userPath);
        }
        
        // Check .base directory
        var basePath = Path.Combine(BaseDirectory.FullName, relativePath);
        if (File.Exists(basePath))
        {
            return new FileInfo(basePath);
        }
        
        return null;
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
        
        var relative = fullPath.Substring(rootPath.Length);
        return relative.TrimStart(Path.DirectorySeparatorChar, Path.AltDirectorySeparatorChar);
    }
}
