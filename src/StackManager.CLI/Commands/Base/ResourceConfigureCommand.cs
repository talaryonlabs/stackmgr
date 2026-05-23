using System.CommandLine;
using Talaryon.StackManager.Arguments;
using Talaryon.StackManager.Exceptions;

namespace Talaryon.StackManager.Commands.Base;

/// <summary>
/// Base command for configuring a resource.
/// </summary>
/// <typeparam name="TArg">The argument type that provides the resource identifier</typeparam>
public abstract class ResourceConfigureCommand<TArg> : StackManagerCommand
    where TArg : Argument<string>, new()
{
    /// <summary>
    /// Creates a new resource configure command.
    /// </summary>
    /// <param name="name">The command name (e.g., "environment", "stack", "app")</param>
    /// <param name="description">The command description</param>
    protected ResourceConfigureCommand(string name, string description)
        : base(name, description)
    {
        Add(new TArg());
        SetAction(ExecuteConfigure);
    }

    /// <summary>
    /// Configures the resource from the parse result.
    /// </summary>
    /// <param name="parseResult">The parse result containing command arguments</param>
    protected abstract void Configure(ParseResult parseResult);

    private void ExecuteConfigure(ParseResult parseResult)
    {
        try
        {
            Configure(parseResult);
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
