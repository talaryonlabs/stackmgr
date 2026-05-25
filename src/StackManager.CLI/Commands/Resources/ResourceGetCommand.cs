using System.CommandLine;
using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Commands.Resources;

/// <summary>
/// Base command for getting/listing resources of a specific type.
/// Supports 0, 1, or multiple parent context options.
/// </summary>
/// <typeparam name="TResource">The type of resource to get</typeparam>
public abstract class ResourceGetCommand<TResource> : BaseCommand
{
    private readonly string _resourceName;

    /// <summary>
    /// Creates a new resource get command.
    /// </summary>
    /// <param name="name">The command name (e.g., "environments", "stacks", "volumes")</param>
    /// <param name="description">The command description</param>
    /// <param name="resourceName">The human-readable resource name for display</param>
    /// <param name="options">Parent context options (e.g., EnvironmentOption, StackOption)</param>
    protected ResourceGetCommand(
        string name, 
        string description, 
        string resourceName,
        params Option[] options)
        : base(name, description)
    {
        _resourceName = resourceName;
        
        foreach (var option in options)
        {
            Add(option);
        }
        
        SetAction(ExecuteGetResourcesAsync);
    }

    /// <summary>
    /// Gets the list of resources to display (synchronous version).
    /// Override this or GetResourcesAsync to provide resource retrieval.
    /// </summary>
    /// <param name="parseResult">The parse result containing command arguments</param>
    /// <returns>The list of resources</returns>
    protected virtual IReadOnlyList<TResource> GetResources(ParseResult parseResult)
    {
        throw new NotImplementedException($"Either {nameof(GetResources)} or {nameof(GetResourcesAsync)} must be overridden.");
    }

    /// <summary>
    /// Gets the list of resources to display (asynchronous version).
    /// Override this method to provide async resource retrieval.
    /// </summary>
    /// <param name="parseResult">The parse result containing command arguments</param>
    /// <returns>A task containing the list of resources</returns>
    protected virtual Task<IReadOnlyList<TResource>> GetResourcesAsync(ParseResult parseResult)
    {
        return Task.FromResult(GetResources(parseResult));
    }

    /// <summary>
    /// Displays a single resource.
    /// </summary>
    /// <param name="resource">The resource to display</param>
    protected abstract void DisplayResource(TResource resource);

    /// <summary>
    /// Gets the name of the resource type for display purposes.
    /// </summary>
    protected virtual string ResourceName => _resourceName;

    private async Task ExecuteGetResourcesAsync(ParseResult parseResult)
    {
        try
        {
            var resources = await GetResourcesAsync(parseResult).ConfigureAwait(false);
            DisplayResources(resources);
        }
        catch (StackManagerException ex)
        {
            LogMessage.AsError(ex.Message);
            throw; // Re-throw to be caught by Program.cs
        }
        catch (Exception ex)
        {
            LogMessage.AsError(ex.Message);
            throw new SystemErrorException(ex.Message, ex);
        }
    }

    /// <summary>
    /// Displays the list of resources with header and empty state handling.
    /// </summary>
    /// <param name="resources">The list of resources to display</param>
    protected virtual void DisplayResources(IReadOnlyList<TResource> resources)
    {
        if (resources.Count == 0)
        {
            LogMessage.AsWarning($"No {ResourceName} found.");
            return;
        }
        
        LogMessage.AsInfo($"{ResourceName}:");
        foreach (var resource in resources)
        {
            DisplayResource(resource);
        }
    }
}
