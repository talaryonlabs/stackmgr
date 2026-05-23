using System.CommandLine;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Exceptions;
using Talaryon.StackManager.Types;

namespace Talaryon.StackManager.Commands.Base;

/// <summary>
/// Base command for describing/displaying detailed information about a single resource.
/// </summary>
/// <typeparam name="TResource">The type of resource to describe (e.g., StackEnvironment, Stack, StackApp)</typeparam>
/// <typeparam name="TArg">The argument type that provides the resource name</typeparam>
public abstract class ResourceDescribeCommand<TResource, TArg> : StackManagerCommand
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
        SetAction(ExecuteDescribeResource);
    }

    /// <summary>
    /// Loads the resource from the parse result.
    /// </summary>
    /// <param name="parseResult">The parse result containing command arguments</param>
    /// <returns>The loaded resource</returns>
    protected abstract TResource LoadResource(ParseResult parseResult);

    /// <summary>
    /// Displays detailed information about the resource.
    /// </summary>
    /// <param name="resource">The resource to display</param>
    protected abstract void DisplayResource(TResource resource);

    private void ExecuteDescribeResource(ParseResult parseResult)
    {
        try
        {
            var resource = LoadResource(parseResult);
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
