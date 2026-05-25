using System.CommandLine;
using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Commands.Resources;

/// <summary>
/// Base command for deleting a resource.
/// </summary>
/// <typeparam name="TResource">The type of resource to delete (e.g., StackEnvironment, Stack, StackApp)</typeparam>
/// <typeparam name="TArg">The argument type that provides the resource name</typeparam>
public abstract class ResourceDeleteCommand<TResource, TArg> : BaseCommand
    where TArg : Argument<string>, new()
{
    /// <summary>
    /// Creates a new resource delete command.
    /// </summary>
    /// <param name="name">The command name (e.g., "environment", "stack")</param>
    /// <param name="description">The command description</param>
    protected ResourceDeleteCommand(string name, string description)
        : base(name, description)
    {
        Add(new TArg());
        SetAction(ExecuteDeleteResourceAsync);
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
    /// Deletes the resource (synchronous version).
    /// Override this or DeleteResourceInstanceAsync to provide resource deletion.
    /// </summary>
    /// <param name="resource">The resource to delete</param>
    protected virtual void DeleteResourceInstance(TResource resource)
    {
        throw new NotImplementedException($"Either {nameof(DeleteResourceInstance)} or {nameof(DeleteResourceInstanceAsync)} must be overridden.");
    }

    /// <summary>
    /// Deletes the resource (asynchronous version).
    /// Override this method to provide async resource deletion.
    /// </summary>
    /// <param name="resource">The resource to delete</param>
    /// <returns>A task representing the deletion operation</returns>
    protected virtual Task DeleteResourceInstanceAsync(TResource resource)
    {
        DeleteResourceInstance(resource);
        return Task.CompletedTask;
    }

    /// <summary>
    /// Called after successful resource deletion.
    /// </summary>
    /// <param name="resource">The deleted resource</param>
    protected abstract void OnResourceDeleted(TResource resource);

    private async Task ExecuteDeleteResourceAsync(ParseResult parseResult)
    {
        try
        {
            var resource = await LoadResourceAsync(parseResult).ConfigureAwait(false);
            await DeleteResourceInstanceAsync(resource).ConfigureAwait(false);
            OnResourceDeleted(resource);
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
