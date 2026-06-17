using System.Text.RegularExpressions;
using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Builder;

public interface IContentBuilder
{
    IContentBuilder With(string content);
    IContentBuilder With(FileInfo file);
    string Build();
}

public class ContentBuilder(StackApp app) : IContentBuilder
{
    private static readonly Regex VolumeRegex = new("{{app-volume.(?<volumeName>.*?)}}");
    private static readonly Regex ParamRegex = new("{{app-param.(?<paramName>.*?)}}");
    private static readonly Regex RequirementRegex = new("{{app-requirement.(?<requirementName>.*?)}}");
    
    private string _content = string.Empty;

    public IContentBuilder With(string content)
    {
        _content = content;
        return this;
    }

    public IContentBuilder With(FileInfo file)
    {
        _content = File.ReadAllText(file.FullName);
        return this;
    }

    private void BuildVolumes()
    {
        var getVolumeName = HelperMethods.GetApiMethod<ApiMethods>(app.Stack, "GetVolumeName");
        var matches = VolumeRegex.Matches(_content);
        
        foreach (Match match in matches)
        {
            var volumeName = match.Groups["volumeName"].Value;
            var volume = app.Volumes.FirstOrDefault(v => v.Key == volumeName);
            if (volume.Key == null)
            {
                throw new ConfigurationException($"Volume '{volumeName}' not found in application '{app.Name}'");
            }
            _content = _content.Replace(match.Value, (string)getVolumeName.Invoke(volume, [app.Stack, volume.Value])!);
        }
    }
    
    private void BuildParams()
    {
        var matches = ParamRegex.Matches(_content);
        foreach (Match match in matches)
        {
            var paramName = match.Groups["paramName"].Value;
            var param = app.Params.FirstOrDefault(p => p.Key == paramName);
            if (param.Key == null)
            {
                throw new ConfigurationException($"Parameter '{paramName}' not found in application '{app.Name}'");
            }
            _content = _content.Replace(match.Value, param.Value);
        }
    }

    private void BuildRequirements()
    {
        var matches = RequirementRegex.Matches(_content);
        foreach (Match match in matches)
        {
            var requirementName = match.Groups["requirementName"].Value;
            var requirement = app.Requirements.FirstOrDefault(r => r.Key == requirementName);
            if (requirement.Key == null)
            {
                throw new ConfigurationException($"Requirement '{requirementName}' not found in application '{app.Name}'");
            }
            _content = _content.Replace(match.Value, requirement.Value);
        }
    }

    public string Build()
    {
        _content = _content
            .Replace("{{app-name}}", app.Name)
            .Replace("{{stack-name}}", app.Stack.Name)
            .Replace("{{env-name}}", app.Stack.Environment.Name);

        if (_content.Contains("{{vault-path}}"))
        {
            if (string.IsNullOrEmpty(app.Stack.Environment.Vault))
            {
                throw new ConfigurationException("Vault-Path is not configured. Please run 'stackmgr configure env <environment-name> --vault <vault-path>' first.");
            }
            _content = _content.Replace("{{vault-path}}", $"{app.Stack.Environment.Vault}/{app.Stack.Name}/{app.Name}");
        }
         
        BuildVolumes();
        BuildParams();
        BuildRequirements();
        
        return _content;
    }
}