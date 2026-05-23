using System.CommandLine;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Exceptions;
using Talaryon.StackManager.Types;

namespace Talaryon.StackManager.Commands.Base;

/// <summary>
/// Base command for creating a new resource.
/// </summary>
/// <typeparam name="TResource">The type of resource to create (e.g., StackEnvironment, Stack, StackApp)</typeparam>
/// <typeparam name="TArg">The argument type that provides the resource name</typeparam>
public abstract class ResourceCreateCommand<TResource, TArg> : StackManagerCommand
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
        SetAction(ExecuteCreateResource);
    }

    /// <summary>
    /// Creates the resource from the parse result.
    /// </summary>
    /// <param name="parseResult">The parse result containing command arguments</param>
    /// <returns>The created resource</returns>
    protected abstract TResource CreateResourceInstance(ParseResult parseResult);

    /// <summary>
    /// Called after successful resource creation.
    /// </summary>
    /// <param name="resource">The created resource</param>
    protected abstract void OnResourceCreated(TResource resource);

    private void ExecuteCreateResource(ParseResult parseResult)
    {
        try
        {
            var resource = CreateResourceInstance(parseResult);
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
