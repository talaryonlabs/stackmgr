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
        SetAction(ExecuteDeleteResource);
    }

    /// <summary>
    /// Loads the resource from the parse result.
    /// </summary>
    /// <param name="parseResult">The parse result containing command arguments</param>
    /// <returns>The loaded resource</returns>
    protected abstract TResource LoadResource(ParseResult parseResult);

    /// <summary>
    /// Deletes the resource.
    /// </summary>
    /// <param name="resource">The resource to delete</param>
    protected abstract void DeleteResourceInstance(TResource resource);

    /// <summary>
    /// Called after successful resource deletion.
    /// </summary>
    /// <param name="resource">The deleted resource</param>
    protected abstract void OnResourceDeleted(TResource resource);

    private void ExecuteDeleteResource(ParseResult parseResult)
    {
        try
        {
            var resource = LoadResource(parseResult);
            DeleteResourceInstance(resource);
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
