using System.CommandLine;
using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Commands.Resources;

/// <summary>
/// Base command for migrating a resource.
/// </summary>
/// <typeparam name="TResource">The type of resource to migrate</typeparam>
/// <typeparam name="TArg">The argument type that provides the resource identifier</typeparam>
public abstract class ResourceMigrateCommand<TResource, TArg> : BaseCommand
    where TArg : Argument<string>, new()
{
    /// <summary>
    /// Creates a new resource migrate command.
    /// </summary>
    /// <param name="name">The command name (e.g., "app", "image")</param>
    /// <param name="description">The command description</param>
    protected ResourceMigrateCommand(string name, string description)
        : base(name, description)
    {
        Add(new TArg());
        SetAction(ExecuteMigrateAsync);
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
    /// Performs the migration on the loaded resource (synchronous version).
    /// Override this or MigrateResourceAsync to provide migration logic.
    /// </summary>
    /// <param name="resource">The resource to migrate</param>
    /// <param name="parseResult">The parse result for accessing additional arguments</param>
    protected virtual void MigrateResource(TResource resource, ParseResult parseResult)
    {
        throw new NotImplementedException($"Either {nameof(MigrateResource)} or {nameof(MigrateResourceAsync)} must be overridden.");
    }

    /// <summary>
    /// Performs the migration on the loaded resource (asynchronous version).
    /// Override this method to provide async migration logic.
    /// </summary>
    /// <param name="resource">The resource to migrate</param>
    /// <param name="parseResult">The parse result for accessing additional arguments</param>
    /// <returns>A task representing the migration operation</returns>
    protected virtual Task MigrateResourceAsync(TResource resource, ParseResult parseResult)
    {
        MigrateResource(resource, parseResult);
        return Task.CompletedTask;
    }

    private async Task ExecuteMigrateAsync(ParseResult parseResult)
    {
        try
        {
            var resource = await LoadResourceAsync(parseResult).ConfigureAwait(false);
            await MigrateResourceAsync(resource, parseResult).ConfigureAwait(false);
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
