using System.CommandLine;
using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Commands.Resources;

/// <summary>
/// Base command for creating a new resource.
/// </summary>
/// <typeparam name="TResource">The type of resource to create (e.g., StackEnvironment, Stack, StackApp)</typeparam>
/// <typeparam name="TArg">The argument type that provides the resource name</typeparam>
public abstract class ResourceCreateCommand<TResource, TArg> : BaseCommand
    where TArg : Argument<string>, new()
{
    /// <summary>
    /// Creates a new resource create command.
    /// </summary>
    /// <param name="name">The command name (e.g., "environment", "stack")</param>
    /// <param name="description">The command description</param>
    protected ResourceCreateCommand(string name, string description)
        : base(name, description)
    {
        Add(new TArg());
        SetAction(ExecuteCreateResourceAsync);
    }

    /// <summary>
    /// Creates the resource from the parse result (synchronous version).
    /// Override this or CreateResourceInstanceAsync to provide resource creation.
    /// </summary>
    /// <param name="parseResult">The parse result containing command arguments</param>
    /// <returns>The created resource</returns>
    protected virtual TResource CreateResourceInstance(ParseResult parseResult)
    {
        throw new NotImplementedException($"Either {nameof(CreateResourceInstance)} or {nameof(CreateResourceInstanceAsync)} must be overridden.");
    }

    /// <summary>
    /// Creates the resource from the parse result (asynchronous version).
    /// Override this method to provide async resource creation.
    /// </summary>
    /// <param name="parseResult">The parse result containing command arguments</param>
    /// <returns>A task containing the created resource</returns>
    protected virtual Task<TResource> CreateResourceInstanceAsync(ParseResult parseResult)
    {
        return Task.FromResult(CreateResourceInstance(parseResult));
    }

    /// <summary>
    /// Called after successful resource creation.
    /// </summary>
    /// <param name="resource">The created resource</param>
    protected abstract void OnResourceCreated(TResource resource);

    private async Task ExecuteCreateResourceAsync(ParseResult parseResult)
    {
        try
        {
            var resource = await CreateResourceInstanceAsync(parseResult).ConfigureAwait(false);
            OnResourceCreated(resource);
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
