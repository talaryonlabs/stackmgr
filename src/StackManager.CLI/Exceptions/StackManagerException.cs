namespace Talaryon.StackManager.Exceptions;

public abstract class StackManagerException(
    string message,
    string resourceType,
    string? resourceName = null,
    string? stackName = null,
    string? environmentName = null)
    : Exception(message)
{
    protected string ResourceType { get; } = resourceType;
    protected string? ResourceName { get; } = resourceName;
    protected string? StackName { get; } = stackName;
    protected string? EnvironmentName { get; } = environmentName;
}
