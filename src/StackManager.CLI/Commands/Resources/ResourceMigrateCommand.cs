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
        SetAction(ExecuteMigrate);
    }

    /// <summary>
    /// Loads the resource from the parse result.
    /// </summary>
    /// <param name="parseResult">The parse result containing command arguments</param>
    /// <returns>The loaded resource</returns>
    protected abstract TResource LoadResource(ParseResult parseResult);

    /// <summary>
    /// Performs the migration on the loaded resource.
    /// </summary>
    /// <param name="resource">The resource to migrate</param>
    /// <param name="parseResult">The parse result for accessing additional arguments</param>
    protected abstract void MigrateResource(TResource resource, ParseResult parseResult);

    private void ExecuteMigrate(ParseResult parseResult)
    {
        try
        {
            var resource = LoadResource(parseResult);
            MigrateResource(resource, parseResult);
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
