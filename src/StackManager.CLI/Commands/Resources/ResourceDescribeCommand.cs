using System.CommandLine;
using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Commands.Resources;

/// <summary>
/// Base command for describing/displaying detailed information about a single resource.
/// </summary>
/// <typeparam name="TResource">The type of resource to describe (e.g., StackEnvironment, Stack, StackApp)</typeparam>
/// <typeparam name="TArg">The argument type that provides the resource name</typeparam>
public abstract class ResourceDescribeCommand<TResource, TArg> : BaseCommand
    where TArg : Argument<string>, new()
{
    /// <summary>
    /// Creates a new resource describe command.
    /// </summary>
    /// <param name="name">The command name (e.g., "environment", "stack")</param>
    /// <param name="description">The command description</param>
    protected ResourceDescribeCommand(string name, string description)
        : base(name, description)
    {
        Add(new TArg());
        SetAction(ExecuteDescribeResourceAsync);
    }

    /// <summary>
    /// Loads the resource from the parse result (synchronous version).
    /// Override this or LoadResourceAsync to provide resource loading.
    /// </summary>
    /// <param name="parseResult">The parse result containing command arguments</param>
    /// <returns>The loaded resource</returns>
    protected virtual TResource LoadResource(ParseResult parseResult)
    {
        throw new NotImplementedException($"Either {nameof(LoadResource)} or {nameof(LoadResourceAsync)} must be overridden.");
    }

    /// <summary>
    /// Loads the resource from the parse result (asynchronous version).
    /// Override this method to provide async resource loading.
    /// </summary>
    /// <param name="parseResult">The parse result containing command arguments</param>
    /// <returns>A task containing the loaded resource</returns>
    protected virtual Task<TResource> LoadResourceAsync(ParseResult parseResult)
    {
        return Task.FromResult(LoadResource(parseResult));
    }

    /// <summary>
    /// Displays detailed information about the resource.
    /// </summary>
    /// <param name="resource">The resource to display</param>
    protected abstract void DisplayResource(TResource resource);

    private async Task ExecuteDescribeResourceAsync(ParseResult parseResult)
    {
        try
        {
            var resource = await LoadResourceAsync(parseResult).ConfigureAwait(false);
            DisplayResource(resource);
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
}
