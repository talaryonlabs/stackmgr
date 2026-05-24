using System.CommandLine;
using Talaryon.StackManager.Commands.Resources;

namespace Talaryon.StackManager.Commands.Environments;

/// <summary>
/// Command for describing a single environment.
/// </summary>
public class DescribeEnvironmentCommand : ResourceDescribeCommand<StackEnvironment, EnvironmentArgument>
{
    public DescribeEnvironmentCommand() 
        : base("environment", "Describe a environment")
    {
        Aliases.Add("env");
    }

    protected override StackEnvironment LoadResource(ParseResult parseResult)
    {
        return GetEnvironment<EnvironmentArgument>(parseResult);
    }

    protected override void DisplayResource(StackEnvironment resource)
    {
        LogMessage.Separator();

        LogBuilder.Message("Environment: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{resource.Name}").AsColored(ConsoleColor.Cyan))
            .Run();

        LogBuilder.Message("Version: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{resource.Version ?? "(default)"}").AsColored(ConsoleColor.DarkCyan))
            .Run();

        LogBuilder.Message("Remote: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{resource.Remote}").AsColored(ConsoleColor.DarkCyan))
            .Run();

        LogBuilder.Message("Vault: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{resource.Vault}").AsColored(ConsoleColor.DarkCyan))
            .Run();

        LogBuilder.Message("Outpost: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{resource.Outpost}").AsColored(ConsoleColor.DarkCyan))
            .Run();

        LogBuilder.Message("Cert Issuer: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{resource.CertIssuer}").AsColored(ConsoleColor.DarkCyan))
            .Run();

        LogBuilder.Message("Registry Credentials: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{resource.RegistryCredentials}").AsColored(ConsoleColor.DarkCyan))
            .Run();

        LogBuilder.Message("Repository: ")
            .NoNewLineAfter()
            .WaitFor(() => LogBuilder.Message($"{resource.Repository ?? "(none)"}").AsColored(ConsoleColor.DarkCyan))
            .Run();

        LogMessage.Separator();
    }
}
